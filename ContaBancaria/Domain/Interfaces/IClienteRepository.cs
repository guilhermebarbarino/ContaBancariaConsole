using ContaBancaria.Domain.Entidades;

namespace ContaBancaria.Domain.Interfaces;

public interface IClienteRepository
{
    void Adicionar(Cliente cliente);
    Cliente? ObterPorCpf(string cpf);
    Cliente? ObterPorId(Guid id);
    List<Cliente> ListarTodos();
}