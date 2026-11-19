
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
    }
}
