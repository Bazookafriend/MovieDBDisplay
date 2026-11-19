using System.Text.Json.Serialization;

namespace DTO
{
    public class VideoListDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("results")]
        public List<VideoDTO> Results { get; set; } = [];
    }
}