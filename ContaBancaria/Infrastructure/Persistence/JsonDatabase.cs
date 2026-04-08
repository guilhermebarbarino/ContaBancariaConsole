using System.Text.Json;

namespace ContaBancaria.Infrastructure.Persistence;

public class JsonDatabase
{
    private readonly string _filePath;

    public JsonDatabase(string filePath)
    {
        _filePath = filePath;
    }

    public DatabaseModel Carregar()
    {
        if (!File.Exists(_filePath))
            return new DatabaseModel();

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new DatabaseModel();

        return JsonSerializer.Deserialize<DatabaseModel>(json) ?? new DatabaseModel();
    }

    public void Salvar(DatabaseModel database)
    {
        var json = JsonSerializer.Serialize(database, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }
}