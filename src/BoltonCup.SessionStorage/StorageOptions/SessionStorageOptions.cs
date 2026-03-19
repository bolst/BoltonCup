using System.Text.Json;

namespace BoltonCup.SessionStorage.StorageOptions;

public class SessionStorageOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
}
