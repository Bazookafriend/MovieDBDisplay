using System.Text.Json.Serialization;

namespace DTO
{
    public class MovieListPageDTO
    {
        public const int PageSize = 20;

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<MovieDTO> Results { get; set; } = [];

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonIgnore]
        public bool HasPreviousPage => Page > 1;

        [JsonIgnore]
        public bool HasNextPage => Page < TotalPages;
    }
}