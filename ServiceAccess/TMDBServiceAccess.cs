
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
    }
}
