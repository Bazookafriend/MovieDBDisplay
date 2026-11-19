using System.Text.Json.Serialization;

namespace DTO
{
    public class GenreListDTO
    {
        [JsonPropertyName("genres")]
        public List<GenreDTO> Genres { get; set; } = [];
    }
}