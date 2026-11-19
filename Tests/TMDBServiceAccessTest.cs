using System.Net;
using System.Text;
using System.Text.Json;
using DTO;
using ServiceAccess;

namespace Tests
{
    public class TMDBServiceAccessTest
    {
        private static HttpClient CreateClient(HttpMessageHandler handler) =>
            new(handler) { BaseAddress = new Uri("https://api.themoviedb.org/3/") };

        private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        [Fact]
        public async Task GetMovieByIdAsync_MapsApiResponseToMovieDTO()
        {
            const string json = """
            {
              "backdrop_path": "/backdrop.jpg",
              "genres": [
                { "id": 28, "name": "Action" },
                { "id": 12, "name": "Adventure" }
              ],
              "id": 12345,
              "overview": "A movie about testing.",
              "poster_path": "/poster.jpg",
              "release_date": "2024-06-14",
              "title": "Test Movie",
              "video": false
            }
            """;

            var handler = new RecordingHandler(_ => JsonResponse(json));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieDTO>(await service.GetMovieByIdAsync(12345));

            Assert.Equal(12345, result.Id);
            Assert.Equal("Test Movie", result.Title);
            Assert.Equal("A movie about testing.", result.Overview);
            Assert.Equal("2024-06-14", result.ReleaseDate);
            Assert.Equal("/backdrop.jpg", result.BackdropPath);
            Assert.Equal("/poster.jpg", result.PosterPath);
            Assert.False(result.Video);
            Assert.Collection(result.Genres,
                g => { Assert.Equal(28, g.Id); Assert.Equal("Action", g.Name); },
                g => { Assert.Equal(12, g.Id); Assert.Equal("Adventure", g.Name); });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(550)]
        [InlineData(999999)]
        public async Task GetMovieByIdAsync_RequestsMovieEndpointForId(int id)
        {
            var handler = new RecordingHandler(_ => JsonResponse("{}"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await service.GetMovieByIdAsync(id);

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(new Uri($"https://api.themoviedb.org/3/movie/{id}"), request.RequestUri);
        }

        [Fact]
        public async Task GetMovieByIdAsync_ReturnsNull_WhenApiReturnsNullBody()
        {
            var handler = new RecordingHandler(_ => JsonResponse("null"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = await service.GetMovieByIdAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetMovieByIdAsync_ThrowsHttpRequestException_WhenApiReturnsNotFound()
        {
            var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetMovieByIdAsync(1));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task GetMovieByIdAsync_ThrowsJsonException_WhenResponseIsNotValidJson()
        {
            var handler = new RecordingHandler(_ => JsonResponse("this is not json"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await Assert.ThrowsAsync<JsonException>(() => service.GetMovieByIdAsync(1));
        }

        [Fact]
        public async Task GetMovieByIdAsync_UsesDefaults_WhenOptionalFieldsAreMissing()
        {
            var handler = new RecordingHandler(_ => JsonResponse("""{ "id": 7 }"""));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieDTO>(await service.GetMovieByIdAsync(7));

            Assert.Equal(7, result.Id);
            Assert.Equal("", result.Title);
            Assert.Equal("", result.Overview);
            Assert.Equal("", result.ReleaseDate);
            Assert.False(result.Video);
            Assert.Null(result.BackdropPath);
            Assert.Null(result.PosterPath);
            Assert.Empty(result.Genres);
        }

        private sealed class RecordingHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            public List<HttpRequestMessage> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(_responder(request));
            }
        }
    }
}