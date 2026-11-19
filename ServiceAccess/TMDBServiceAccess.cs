
using DTO;
using System.Net.Http.Json;

namespace ServiceAccess
{
    public class TMDBServiceAccess
    {
        private readonly HttpClient _httpClient;

        public TMDBServiceAccess(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MovieDTO?> GetMovieByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MovieDTO>($"movie/{id}");
        }

        public async Task<MovieCreditsDTO?> GetMovieCreditsAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<MovieCreditsDTO>($"movie/{id}/credits");
        }

        public async Task<List<GenreDTO>> GetAllGenresAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<GenreListDTO>("genre/movie/list");
            return response?.Genres ?? [];
        }

        public async Task<MovieListPageDTO?> GetMoviesByGenreAsync(int genreId, int page = 1)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(genreId, 1);
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);

            return await _httpClient.GetFromJsonAsync<MovieListPageDTO>(
                $"discover/movie?with_genres={genreId}&page={page}");
        }
    }
}
