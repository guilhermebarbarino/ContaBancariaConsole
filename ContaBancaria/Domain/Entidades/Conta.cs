using System.Text.Json.Serialization;

namespace ContaBancaria.Domain.Entidades;

public class Conta
{
    public string Agencia { get; private set; }
    public int Numero { get; private set; }
    public int Dac { get; private set; }
    public Guid ClienteId { get; private set; }
    public decimal Saldo { get; private set; }

    [JsonConstructor]
    public Conta(string agencia, int numero, int dac, Guid clienteId, decimal saldo = 0)
    {
        if (string.IsNullOrWhiteSpace(agencia))
            throw new ArgumentException("Agência é obrigatória.");

        if (numero <= 0)
            throw new ArgumentException("Número da conta inválido.");

        if (dac < 0 || dac > 9)
            throw new ArgumentException("DAC inválido.");

        if (clienteId == Guid.Empty)
            throw new ArgumentException("Identificador do cliente inválido.");

        if (saldo < 0)
            throw new ArgumentException("Saldo inicial não pode ser negativo.");

        Agencia = agencia;
        Numero = numero;
        Dac = dac;
        ClienteId = clienteId;
        Saldo = saldo;
    }

    public void Depositar(decimal valor)
    {
        if (valor <= 0)
            throw new InvalidOperationException("O valor do depósito deve ser maior que zero.");

        Saldo += valor;
    }

    public void Sacar(decimal valor)
    {
        if (valor <= 0)
            throw new InvalidOperationException("O valor do saque deve ser maior que zero.");

        if (valor > Saldo)
            throw new InvalidOperationException("Saldo insuficiente.");

        Saldo -= valor;
    }

    public void TransferirPara(Conta destino, decimal valor)
    {
        if (destino is null)
            throw new InvalidOperationException("Transferência não concluída! Conta não encontrada.");

        if (destino.Numero == Numero && destino.Agencia == Agencia)
            throw new InvalidOperationException("Transferência não concluída! Não é permitido transferir para a mesma conta.");

        if (valor <= 0)
            throw new InvalidOperationException("O valor da transferência deve ser maior que zero.");

        if (valor > Saldo)
            throw new InvalidOperationException("Saldo insuficiente.");

        // Calcula o crédito antes do débito para não alterar a origem em caso de overflow.
        var saldoDestino = destino.Saldo + valor;
        Saldo -= valor;
        destino.Saldo = saldoDestino;
    }
}
