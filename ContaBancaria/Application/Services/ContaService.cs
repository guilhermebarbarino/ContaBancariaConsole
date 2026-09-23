using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;

namespace ContaBancaria.Application.Services;

public class ContaService
{
    private readonly IContaRepository _contaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMovimentacaoRepository _movimentacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContaService(
        IContaRepository contaRepository,
        IClienteRepository clienteRepository,
        IMovimentacaoRepository movimentacaoRepository,
        IUnitOfWork unitOfWork)
    {
        _contaRepository = contaRepository;
        _clienteRepository = clienteRepository;
        _movimentacaoRepository = movimentacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public List<Conta> ListarContasPorCpf(string cpf)
    {
        var cliente = _clienteRepository.ObterPorCpf(cpf)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        return _contaRepository.ObterPorClienteId(cliente.Id);
    }

    public void Depositar(int numeroConta, decimal valor)
    {
        _unitOfWork.Executar(() =>
        {
            var conta = ConsultarSaldo(numeroConta);
            conta.Depositar(valor);

            _movimentacaoRepository.Adicionar(new Movimentacao(
                conta.Numero, "DEPÓSITO", valor, "Depósito realizado com sucesso", conta.Saldo));
        });
    }

    public void Sacar(int numeroConta, decimal valor)
    {
        _unitOfWork.Executar(() =>
        {
            var conta = ConsultarSaldo(numeroConta);
            conta.Sacar(valor);

            _movimentacaoRepository.Adicionar(new Movimentacao(
                conta.Numero, "SAQUE", valor, "Saque realizado com sucesso", conta.Saldo));
        });
    }

    public void Transferir(int origemNumero, int destinoNumero, decimal valor)
    {
        _unitOfWork.Executar(() =>
        {
            var origem = ConsultarSaldo(origemNumero);
            var destino = ConsultarSaldo(destinoNumero);
            origem.TransferirPara(destino, valor);

            _movimentacaoRepository.Adicionar(new Movimentacao(
                origem.Numero,
                "TRANSFERÊNCIA ENVIADA",
                valor,
                $"Transferência para conta {destino.Numero}-{destino.Dac}",
                origem.Saldo));

            _movimentacaoRepository.Adicionar(new Movimentacao(
                destino.Numero,
                "TRANSFERÊNCIA RECEBIDA",
                valor,
                $"Transferência recebida da conta {origem.Numero}-{origem.Dac}",
                destino.Saldo));
        });
    }

    public Conta ConsultarSaldo(int numeroConta)
    {
        return _contaRepository.ObterPorNumero(numeroConta)
            ?? throw new InvalidOperationException("Conta não encontrada.");
    }

    public List<Movimentacao> ConsultarExtrato(int numeroConta)
    {
        var conta = ConsultarSaldo(numeroConta);
        return _movimentacaoRepository.ObterPorConta(conta.Numero);
    }
}
