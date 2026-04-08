using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;

namespace ContaBancaria.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IContaRepository _contaRepository;

        public ClienteService(
            IClienteRepository clienteRepository,
            IContaRepository contaRepository)
        {
            _clienteRepository = clienteRepository;
            _contaRepository = contaRepository;
        }

        public bool CpfJaCadastrado(string cpf)
        {
            return _clienteRepository.ObterPorCpf(cpf) is not null;
        }

        public Cliente Cadastrar(string nome, string cpf, DateTime dataNascimento)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF é obrigatório.");

            if (CpfJaCadastrado(cpf))
                throw new InvalidOperationException("Já existe cliente cadastrado com esse CPF.");

            var cliente = new Cliente(nome, cpf, dataNascimento);

            if (cliente.ObterIdade() < 18)
                throw new InvalidOperationException("Cliente deve ser maior de 18 anos.");

            _clienteRepository.Adicionar(cliente);

            var numeroConta = _contaRepository.ObterProximoNumeroConta();
            var agencia = "0001";
            var dac = GerarDac(numeroConta);

            var conta = new Conta(
                agencia,
                numeroConta,
                dac,
                cliente.Id,
                0);

            _contaRepository.Adicionar(conta);

            return cliente;
        }

        public Cliente? ObterPorCpf(string cpf)
        {
            return _clienteRepository.ObterPorCpf(cpf);
        }

        public List<Cliente> Listar()
        {
            return _clienteRepository.ListarTodos();
        }

        private int GerarDac(int numeroConta)
        {
            return numeroConta % 10;
        }
    }
}