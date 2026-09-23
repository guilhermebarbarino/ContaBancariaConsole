using System.Text.Json;

namespace ContaBancaria.Infrastructure.Persistence;

public class JsonDatabase
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string _filePath;

    public JsonDatabase(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
    }

    public DatabaseModel Carregar()
    {
        if (!File.Exists(_filePath))
            return new DatabaseModel();

        var json = File.ReadAllText(_filePath);

        var database = JsonSerializer.Deserialize<DatabaseModel>(json)
            ?? throw new JsonException("O arquivo de dados não pode conter null.");

        if (database.Clientes is null || database.Contas is null || database.Movimentacoes is null)
            throw new JsonException("O arquivo de dados contém coleções inválidas.");

        return database;
    }

    public void Salvar(DatabaseModel database)
    {
        ArgumentNullException.ThrowIfNull(database);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        var arquivoTemporario = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (var stream = new FileStream(arquivoTemporario, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, database, SerializerOptions);
                stream.Flush(flushToDisk: true);
            }

            // O arquivo temporário fica no mesmo diretório para a substituição ser atômica.
            File.Move(arquivoTemporario, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(arquivoTemporario))
                File.Delete(arquivoTemporario);
        }
    }
}
