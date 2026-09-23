using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;
using ContaBancaria.Infrastructure.Persistence;

namespace ContaBancaria.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly InMemoryDatabaseContext _context;

        public ClienteRepository(InMemoryDatabaseContext context)
        {
            _context = context;
        }

        public void Adicionar(Cliente cliente)
        {
            _context.Database.Clientes.Add(cliente);
        }

        public Cliente? ObterPorCpf(string cpf)
        {
            var cpfNormalizado = Cliente.NormalizarCpf(cpf);
            return _context.Database.Clientes.FirstOrDefault(c => c.Cpf == cpfNormalizado);
        }

        public Cliente? ObterPorId(Guid id)
        {
            return _context.Database.Clientes.FirstOrDefault(c => c.Id == id);
        }

        public List<Cliente> ListarTodos()
        {
            return _context.Database.Clientes.ToList();
        }
    }
}
