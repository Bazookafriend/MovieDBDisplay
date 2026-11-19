
using System.Text.Json.Serialization;

namespace DTO
{
    public class GenreDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}
