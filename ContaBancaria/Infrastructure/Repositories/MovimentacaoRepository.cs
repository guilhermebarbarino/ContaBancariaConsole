using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;
using ContaBancaria.Infrastructure.Persistence;

namespace ContaBancaria.Infrastructure.Repositories;

public class MovimentacaoRepository : IMovimentacaoRepository
{
    private readonly InMemoryDatabaseContext _context;

    public MovimentacaoRepository(InMemoryDatabaseContext context)
    {
        _context = context;
    }

    public void Adicionar(Movimentacao movimentacao)
    {
        _context.Database.Movimentacoes.Add(movimentacao);
        _context.SaveChanges();
    }

    public List<Movimentacao> ObterPorConta(int numeroConta)
    {
        return _context.Database.Movimentacoes
            .Where(m => m.NumeroConta == numeroConta)
            .OrderBy(m => m.Data)
            .ToList();
    }
}