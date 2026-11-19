using System.Text.Json.Serialization;

namespace DTO
{
    public class VideoDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("iso_639_1")]
        public string Language { get; set; } = "";

        [JsonPropertyName("iso_3166_1")]
        public string Country { get; set; } = "";

        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("site")]
        public string Site { get; set; } = "";

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("official")]
        public bool Official { get; set; }

        [JsonPropertyName("published_at")]
        public string PublishedAt { get; set; } = "";
    }
}