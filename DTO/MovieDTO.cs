
using System.Text.Json.Serialization;

namespace DTO
{
    public class MovieDTO()
    {
        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }

        [JsonPropertyName("genres")]
        public List<GenreDTO> Genres { get; set; } = [];

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = "";

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("video")]
        public bool Video { get; set; }
    }
}
