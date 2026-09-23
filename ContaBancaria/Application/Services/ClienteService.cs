using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;

namespace ContaBancaria.Application.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IContaRepository _contaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(
            IClienteRepository clienteRepository,
            IContaRepository contaRepository,
            IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _contaRepository = contaRepository;
            _unitOfWork = unitOfWork;
        }

        public bool CpfJaCadastrado(string cpf)
        {
            return _clienteRepository.ObterPorCpf(cpf) is not null;
        }

        public Cliente Cadastrar(string nome, string cpf, DateTime dataNascimento)
        {
            var cliente = new Cliente(nome, cpf, dataNascimento);

            if (cliente.ObterIdade() < 18)
                throw new InvalidOperationException("Cliente deve ter pelo menos 18 anos.");

            _unitOfWork.Executar(() =>
            {
                if (CpfJaCadastrado(cliente.Cpf))
                    throw new InvalidOperationException("Já existe cliente cadastrado com esse CPF.");

                var numeroConta = _contaRepository.ObterProximoNumeroConta();
                var conta = new Conta("0001", numeroConta, GerarDac(numeroConta), cliente.Id);

                _clienteRepository.Adicionar(cliente);
                _contaRepository.Adicionar(conta);
            });

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

        private static int GerarDac(int numeroConta)
        {
            return numeroConta % 10;
        }
    }
}
