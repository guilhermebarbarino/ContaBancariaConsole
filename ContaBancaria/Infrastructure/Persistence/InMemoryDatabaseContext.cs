using System.Text.Json;
using ContaBancaria.Domain.Interfaces;

namespace ContaBancaria.Infrastructure.Persistence;

public class InMemoryDatabaseContext : IUnitOfWork
{
    public DatabaseModel Database { get; private set; }

    private readonly JsonDatabase _jsonDatabase;

    public InMemoryDatabaseContext(JsonDatabase jsonDatabase)
    {
        _jsonDatabase = jsonDatabase;
        Database = _jsonDatabase.Carregar();
    }

    public void Executar(Action operacao)
    {
        ArgumentNullException.ThrowIfNull(operacao);

        var estadoAnterior = JsonSerializer.Deserialize<DatabaseModel>(
            JsonSerializer.SerializeToUtf8Bytes(Database))!;

        try
        {
            operacao();
            _jsonDatabase.Salvar(Database);
        }
        catch
        {
            Database = estadoAnterior;
            throw;
        }
    }
}
