using System.Text.Json.Serialization;

namespace ContaBancaria.Domain.Entidades;

public class Movimentacao
{
    public Guid Id { get; private set; }
    public int NumeroConta { get; private set; }
    public DateTime Data { get; private set; }
    public string Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public string Descricao { get; private set; }
    public decimal SaldoAposMovimentacao { get; private set; }

    public Movimentacao(
        int numeroConta,
        string tipo,
        decimal valor,
        string descricao,
        decimal saldoAposMovimentacao)
        : this(Guid.NewGuid(), numeroConta, DateTime.Now, tipo, valor, descricao, saldoAposMovimentacao)
    {
    }

    [JsonConstructor]
    public Movimentacao(
        Guid id,
        int numeroConta,
        DateTime data,
        string tipo,
        decimal valor,
        string descricao,
        decimal saldoAposMovimentacao)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Identificador da movimentação inválido.");

        if (numeroConta <= 0)
            throw new ArgumentException("Número da conta inválido.");

        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("Tipo da movimentação é obrigatório.");

        if (valor <= 0)
            throw new ArgumentException("Valor da movimentação deve ser maior que zero.");

        Id = id;
        NumeroConta = numeroConta;
        Data = data;
        Tipo = tipo;
        Valor = valor;
        Descricao = descricao;
        SaldoAposMovimentacao = saldoAposMovimentacao;
    }
}
