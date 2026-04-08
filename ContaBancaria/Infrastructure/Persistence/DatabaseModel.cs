using ContaBancaria.Domain.Entidades;

namespace ContaBancaria.Infrastructure.Persistence;

public class DatabaseModel
{
    public List<Cliente> Clientes { get; set; } = new();
    public List<Conta> Contas { get; set; } = new();
    public List<Movimentacao> Movimentacoes { get; set; } = new();

}