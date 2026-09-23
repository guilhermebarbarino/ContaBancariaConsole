using System.Text.Json;
using ContaBancaria.Application.Enums;
using ContaBancaria.Application.Services;
using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;
using ContaBancaria.Infrastructure.Persistence;
using ContaBancaria.Infrastructure.Repositories;

namespace ContaBancaria.Tests;

public sealed class BancoTests : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), $"ContaBancariaTests_{Guid.NewGuid():N}");
    private string Arquivo => Path.Combine(_pasta, "database.json");

    private (InMemoryDatabaseContext Contexto, ClienteService Clientes, ContaService Contas) AbrirBanco()
    {
        var contexto = new InMemoryDatabaseContext(new JsonDatabase(Arquivo));
        var clientes = new ClienteRepository(contexto);
        var contas = new ContaRepository(contexto);
        var movimentacoes = new MovimentacaoRepository(contexto);
        return (contexto, new ClienteService(clientes, contas, contexto),
            new ContaService(contas, clientes, movimentacoes, contexto));
    }

    [Fact]
    public void ReabrirBancoPreservaIdentidadesVinculosSaldosEHistorico()
    {
        var banco = AbrirBanco();
        var cliente = banco.Clientes.Cadastrar(" Ana ", "123.456.789-00", new DateTime(1990, 1, 1));
        banco.Clientes.Cadastrar("Bruno", "98765432100", new DateTime(1990, 1, 1));
        banco.Contas.Depositar(1001, 150.75m);
        banco.Contas.Sacar(1001, 20m);
        banco.Contas.Transferir(1001, 1002, 30.50m);
        var historico = banco.Contexto.Database.Movimentacoes.ToList();

        var reaberto = AbrirBanco();

        Assert.Equal(cliente.Id, reaberto.Clientes.ObterPorCpf("12345678900")!.Id);
        Assert.Equal("Ana", reaberto.Clientes.ObterPorCpf("123.456.789-00")!.Nome);
        Assert.Equal(cliente.Id, Assert.Single(reaberto.Contas.ListarContasPorCpf("12345678900")).ClienteId);
        Assert.Equal(100.25m, reaberto.Contas.ConsultarSaldo(1001).Saldo);
        Assert.Equal(30.50m, reaberto.Contas.ConsultarSaldo(1002).Saldo);
        Assert.Equal(4, reaberto.Contexto.Database.Movimentacoes.Count);
        Assert.Equal(historico.Select(m => (m.Id, m.Data, m.NumeroConta, m.Valor, m.SaldoAposMovimentacao)),
            reaberto.Contexto.Database.Movimentacoes.Select(m => (m.Id, m.Data, m.NumeroConta, m.Valor, m.SaldoAposMovimentacao)));
    }

    [Fact]
    public void CpfComOuSemPontuacaoNaoPermiteCadastroDuplicado()
    {
        var banco = AbrirBanco();
        banco.Clientes.Cadastrar("Ana", "123.456.789-00", new DateTime(1990, 1, 1));

        Assert.True(banco.Clientes.CpfJaCadastrado("12345678900"));
        Assert.Throws<InvalidOperationException>(() =>
            banco.Clientes.Cadastrar("Outra pessoa", "12345678900", new DateTime(1990, 1, 1)));
        Assert.Single(banco.Clientes.Listar());
        Assert.Single(AbrirBanco().Contexto.Database.Contas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("1234567890x")]
    [InlineData("123456789000")]
    public void CpfMalformadoNaoAlteraBanco(string cpf)
    {
        var banco = AbrirBanco();
        Assert.Throws<ArgumentException>(() => banco.Clientes.Cadastrar("Ana", cpf, new DateTime(1990, 1, 1)));
        Assert.Empty(banco.Clientes.Listar());
        Assert.Empty(banco.Contexto.Database.Contas);
        Assert.False(File.Exists(Arquivo));
    }

    [Fact]
    public void CadastroAceitaDezoitoAnosExatosERejeitaMenorOuNascimentoFuturo()
    {
        var banco = AbrirBanco();
        Assert.Throws<InvalidOperationException>(() =>
            banco.Clientes.Cadastrar("Menor", "12345678900", DateTime.Today.AddYears(-18).AddDays(1)));
        Assert.Throws<ArgumentException>(() =>
            banco.Clientes.Cadastrar("Futuro", "12345678900", DateTime.Today.AddDays(1)));

        banco.Clientes.Cadastrar("Adulto", "12345678900", DateTime.Today.AddYears(-18));
        Assert.Single(banco.Clientes.Listar());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValoresNaoPositivosNaoAlteramSaldoNemExtrato(int valor)
    {
        var banco = AbrirBanco();
        banco.Clientes.Cadastrar("Ana", "12345678900", new DateTime(1990, 1, 1));
        banco.Clientes.Cadastrar("Bruno", "98765432100", new DateTime(1990, 1, 1));
        var jsonAnterior = File.ReadAllText(Arquivo);

        Assert.Throws<InvalidOperationException>(() => banco.Contas.Depositar(1001, valor));
        Assert.Throws<InvalidOperationException>(() => banco.Contas.Sacar(1001, valor));
        Assert.Throws<InvalidOperationException>(() => banco.Contas.Transferir(1001, 1002, valor));
        Assert.All(banco.Contexto.Database.Contas, conta => Assert.Equal(0m, conta.Saldo));
        Assert.Empty(banco.Contexto.Database.Movimentacoes);
        Assert.Equal(jsonAnterior, File.ReadAllText(Arquivo));
    }

    [Fact]
    public void FalhasDeSaqueETransferenciaPreservamSaldoEHistorico()
    {
        var banco = AbrirBanco();
        banco.Clientes.Cadastrar("Ana", "12345678900", new DateTime(1990, 1, 1));
        banco.Clientes.Cadastrar("Bruno", "98765432100", new DateTime(1990, 1, 1));
        banco.Contas.Depositar(1001, 10m);
        var jsonAnterior = File.ReadAllText(Arquivo);

        Assert.Throws<InvalidOperationException>(() => banco.Contas.Sacar(1001, 11m));
        Assert.Throws<InvalidOperationException>(() => banco.Contas.Transferir(1001, 1002, 11m));
        Assert.Throws<InvalidOperationException>(() => banco.Contas.Transferir(1001, 1001, 1m));
        Assert.Throws<InvalidOperationException>(() => banco.Contas.Transferir(1001, 9999, 1m));
        Assert.Equal(10m, banco.Contas.ConsultarSaldo(1001).Saldo);
        Assert.Equal(0m, banco.Contas.ConsultarSaldo(1002).Saldo);
        Assert.Single(banco.Contexto.Database.Movimentacoes);
        Assert.Equal(jsonAnterior, File.ReadAllText(Arquivo));
    }

    [Fact]
    public void OverflowNoDestinoNaoDebitaContaDeOrigem()
    {
        var origem = new Conta("0001", 1001, 1, Guid.NewGuid(), 10m);
        var destino = new Conta("0001", 1002, 2, Guid.NewGuid(), decimal.MaxValue);

        Assert.Throws<OverflowException>(() => origem.TransferirPara(destino, 1m));
        Assert.Equal(10m, origem.Saldo);
        Assert.Equal(decimal.MaxValue, destino.Saldo);
    }

    [Fact]
    public void ErroAoRegistrarSegundaMovimentacaoDesfazTodaTransferencia()
    {
        var banco = AbrirBanco();
        banco.Clientes.Cadastrar("Ana", "12345678900", new DateTime(1990, 1, 1));
        banco.Clientes.Cadastrar("Bruno", "98765432100", new DateTime(1990, 1, 1));
        banco.Contas.Depositar(1001, 100m);
        var jsonAnterior = File.ReadAllText(Arquivo);
        var servico = new ContaService(new ContaRepository(banco.Contexto), new ClienteRepository(banco.Contexto),
            new MovimentacaoComFalha(new MovimentacaoRepository(banco.Contexto)), banco.Contexto);

        Assert.Throws<IOException>(() => servico.Transferir(1001, 1002, 25m));

        Assert.Equal(100m, banco.Contas.ConsultarSaldo(1001).Saldo);
        Assert.Equal(0m, banco.Contas.ConsultarSaldo(1002).Saldo);
        Assert.Single(banco.Contexto.Database.Movimentacoes);
        Assert.Equal(jsonAnterior, File.ReadAllText(Arquivo));
    }

    [Fact]
    public void FalhaNaGravacaoDesfazCadastroELimpaArquivoTemporario()
    {
        var banco = AbrirBanco();
        Directory.CreateDirectory(Arquivo); // Impede substituir o destino por um arquivo.

        var erro = Record.Exception(() =>
            banco.Clientes.Cadastrar("Ana", "12345678900", new DateTime(1990, 1, 1)));

        Assert.True(erro is IOException or UnauthorizedAccessException);
        Assert.Empty(banco.Clientes.Listar());
        Assert.Empty(banco.Contexto.Database.Contas);
        Assert.Empty(Directory.GetFiles(_pasta, "*.tmp"));
    }

    [Fact]
    public void FalhaNaGravacaoDesfazSaldosEExtratosDeTransferencia()
    {
        var banco = AbrirBanco();
        banco.Clientes.Cadastrar("Ana", "12345678900", new DateTime(1990, 1, 1));
        banco.Clientes.Cadastrar("Bruno", "98765432100", new DateTime(1990, 1, 1));
        banco.Contas.Depositar(1001, 100m);
        var backup = Arquivo + ".bak";
        var jsonAnterior = File.ReadAllText(Arquivo);
        File.Move(Arquivo, backup);
        Directory.CreateDirectory(Arquivo);

        var erro = Record.Exception(() => banco.Contas.Transferir(1001, 1002, 25m));

        Assert.True(erro is IOException or UnauthorizedAccessException);
        Assert.Equal(100m, banco.Contas.ConsultarSaldo(1001).Saldo);
        Assert.Equal(0m, banco.Contas.ConsultarSaldo(1002).Saldo);
        Assert.Single(banco.Contexto.Database.Movimentacoes);
        Assert.Equal(jsonAnterior, File.ReadAllText(backup));
        Assert.Empty(Directory.GetFiles(_pasta, "*.tmp"));

        Directory.Delete(Arquivo);
        File.Move(backup, Arquivo);
        banco.Contas.Transferir(1001, 1002, 25m);
        Assert.Equal(75m, AbrirBanco().Contas.ConsultarSaldo(1001).Saldo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("{invalido")]
    [InlineData("{\"Clientes\": null}")]
    public void BaseInvalidaGeraErroEPreservaArquivo(string conteudo)
    {
        Directory.CreateDirectory(_pasta);
        File.WriteAllText(Arquivo, conteudo);

        Assert.Throws<JsonException>(() => AbrirBanco());
        Assert.Equal(conteudo, File.ReadAllText(Arquivo));
    }

    [Theory]
    [InlineData(TipoExportacaoExtrato.Txt)]
    [InlineData(TipoExportacaoExtrato.Json)]
    public void ExportacoesSucessivasNaoSobrescrevemArquivos(TipoExportacaoExtrato tipo)
    {
        var servico = new ExtratoExportService(Path.Combine(_pasta, "Extratos"));
        var movimentacoes = new List<Movimentacao> { new(1001, "DEPÓSITO", 10.50m, "Depósito", 10.50m) };

        var primeiro = servico.Exportar(movimentacoes, 1001, tipo);
        var segundo = servico.Exportar(movimentacoes, 1001, tipo);

        Assert.NotEqual(primeiro, segundo);
        Assert.True(File.Exists(primeiro));
        Assert.True(File.Exists(segundo));
        if (tipo == TipoExportacaoExtrato.Txt)
        {
            Assert.Contains("10,50", File.ReadAllText(primeiro));
        }
        else
        {
            using var json = JsonDocument.Parse(File.ReadAllText(primeiro));
            Assert.Equal(10.50m, json.RootElement.GetProperty("Movimentacoes")[0].GetProperty("Valor").GetDecimal());
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_pasta))
            Directory.Delete(_pasta, recursive: true);
    }

    private sealed class MovimentacaoComFalha(IMovimentacaoRepository repository) : IMovimentacaoRepository
    {
        private int _adicionadas;

        public void Adicionar(Movimentacao movimentacao)
        {
            if (++_adicionadas == 2)
                throw new IOException("Falha simulada ao registrar a segunda movimentação.");

            repository.Adicionar(movimentacao);
        }

        public List<Movimentacao> ObterPorConta(int numeroConta) => repository.ObterPorConta(numeroConta);
    }
}
