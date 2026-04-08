using ContaBancaria.Domain.Entidades;

namespace ContaBancaria.Domain.Interfaces;

public interface IContaRepository
{
    void Adicionar(Conta conta);
    Conta? ObterPorNumero(int numero);
    List<Conta> ObterPorClienteId(Guid clienteId);
    List<Conta> ListarTodas();
    int ObterProximoNumeroConta();
    void SalvarAlteracoes();
}