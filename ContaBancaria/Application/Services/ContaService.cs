using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;

namespace ContaBancaria.Application.Services
{ 
    public class ContaService
    {
        private readonly IContaRepository _contaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMovimentacaoRepository _movimentacaoRepository;

        public ContaService(
            IContaRepository contaRepository,
            IClienteRepository clienteRepository,
            IMovimentacaoRepository movimentacaoRepository)
        {
            _contaRepository = contaRepository;
            _clienteRepository = clienteRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        public List<Conta> ListarContasPorCpf(string cpf)
        {
            var cliente = _clienteRepository.ObterPorCpf(cpf)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            return _contaRepository.ObterPorClienteId(cliente.Id);
        }

        public void Depositar(int numeroConta, decimal valor)
        {
            var conta = _contaRepository.ObterPorNumero(numeroConta)
                ?? throw new InvalidOperationException("Conta não encontrada.");

            conta.Depositar(valor);
            _contaRepository.SalvarAlteracoes();

            var movimentacao = new Movimentacao(
                conta.Numero,
                "DEPÓSITO",
                valor,
                "Depósito realizado com sucesso",
                conta.Saldo);

            _movimentacaoRepository.Adicionar(movimentacao);
        }

        public void Sacar(int numeroConta, decimal valor)
        {
            var conta = _contaRepository.ObterPorNumero(numeroConta)
                ?? throw new InvalidOperationException("Conta não encontrada.");

            conta.Sacar(valor);
            _contaRepository.SalvarAlteracoes();

            var movimentacao = new Movimentacao(
                conta.Numero,
                "SAQUE",
                valor,
                "Saque realizado com sucesso",
                conta.Saldo);

            _movimentacaoRepository.Adicionar(movimentacao);
        }

        public void Transferir(int origemNumero, int destinoNumero, decimal valor)
        {
            var origem = _contaRepository.ObterPorNumero(origemNumero);
            var destino = _contaRepository.ObterPorNumero(destinoNumero);

            if (origem is null || destino is null)
                throw new InvalidOperationException("Transferência não concluída! Conta não encontrada.");

            if (origem.Numero == destino.Numero && origem.Agencia == destino.Agencia)
                throw new InvalidOperationException("Transferência não concluída! Não é permitido transferir para a mesma conta.");

            origem.TransferirPara(destino, valor);
            _contaRepository.SalvarAlteracoes();

            var movimentacaoOrigem = new Movimentacao(
                origem.Numero,
                "TRANSFERÊNCIA ENVIADA",
                valor,
                $"Transferência para conta {destino.Numero}-{destino.Dac}",
                origem.Saldo);

            var movimentacaoDestino = new Movimentacao(
                destino.Numero,
                "TRANSFERÊNCIA RECEBIDA",
                valor,
                $"Transferência recebida da conta {origem.Numero}-{origem.Dac}",
                destino.Saldo);

            _movimentacaoRepository.Adicionar(movimentacaoOrigem);
            _movimentacaoRepository.Adicionar(movimentacaoDestino);
        }

        public Conta ConsultarSaldo(int numeroConta)
        {
            return _contaRepository.ObterPorNumero(numeroConta)
                ?? throw new InvalidOperationException("Conta não encontrada.");
        }

        public List<Movimentacao> ConsultarExtrato(int numeroConta)
        {
            var conta = _contaRepository.ObterPorNumero(numeroConta)
                ?? throw new InvalidOperationException("Conta não encontrada.");

            return _movimentacaoRepository.ObterPorConta(conta.Numero);
        }
    }
}