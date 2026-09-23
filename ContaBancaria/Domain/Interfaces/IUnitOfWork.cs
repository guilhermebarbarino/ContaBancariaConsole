namespace ContaBancaria.Domain.Interfaces;

public interface IUnitOfWork
{
    /// <summary>
    /// Persiste uma operação completa ou restaura o estado anterior se ela falhar.
    /// </summary>
    void Executar(Action operacao);
}
