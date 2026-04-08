using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;
using ContaBancaria.Infrastructure.Persistence;

namespace ContaBancaria.Infrastructure.Repositories
{
    public class ContaRepository : IContaRepository
    {
        private readonly InMemoryDatabaseContext _context;

        public ContaRepository(InMemoryDatabaseContext context)
        {
            _context = context;
        }

        public void Adicionar(Conta conta)
        {
            _context.Database.Contas.Add(conta);
            _context.SaveChanges();
        }

        public Conta? ObterPorNumero(int numero)
        {
            return _context.Database.Contas.FirstOrDefault(c => c.Numero == numero);
        }

        public List<Conta> ObterPorClienteId(Guid clienteId)
        {
            return _context.Database.Contas
                .Where(c => c.ClienteId == clienteId)
                .ToList();
        }

        public List<Conta> ListarTodas()
        {
            return _context.Database.Contas;
        }

        public int ObterProximoNumeroConta()
        {
            if (!_context.Database.Contas.Any())
                return 1001;

            return _context.Database.Contas.Max(c => c.Numero) + 1;
        }

        public void SalvarAlteracoes()
        {
            _context.SaveChanges();
        }
    }
}