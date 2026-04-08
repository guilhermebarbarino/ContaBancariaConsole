namespace ContaBancaria.Infrastructure.Persistence;

public class InMemoryDatabaseContext
{
    public DatabaseModel Database { get; }

    private readonly JsonDatabase _jsonDatabase;

    public InMemoryDatabaseContext(JsonDatabase jsonDatabase)
    {
        _jsonDatabase = jsonDatabase;
        Database = _jsonDatabase.Carregar();
    }

    public void SaveChanges()
    {
        _jsonDatabase.Salvar(Database);
    }
}