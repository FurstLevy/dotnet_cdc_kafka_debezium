using System.Text.Json.Serialization;

namespace Dotnet.CDC.Api.ViewModels
{
    public class ProductCacheViewModel
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("price")] public int Price { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }
    }
}
