
namespace DTO
{
    public class MovieDTO(int id, string title, string description, DateTime releaseDate, List<int> genres, string? posterURL, string? backdropURL)
    {
        public int Id { get; set; } = id;
        public string Title { get; set; } = title;
        public string Description { get; set; } = description;
        public DateTime ReleaseDate { get; set; } = releaseDate;
        public List<int> Genres { get; set; } = genres;
        public string? PosterURL { get; set; } = posterURL;
        public string? BackdropURL { get; set; } = backdropURL;
    }
}
