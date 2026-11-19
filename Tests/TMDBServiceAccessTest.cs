using System.Net;
using System.Text;
using System.Text.Json;
using DTO;
using ServiceAccess;

namespace Tests
{
    public class TMDBServiceAccessTest
    {
        private const string CreditsJson = """
            {
              "id": 550,
              "cast": [
                {
                  "adult": false,
                  "gender": 2,
                  "id": 819,
                  "known_for_department": "Acting",
                  "name": "Edward Norton",
                  "original_name": "Edward Norton",
                  "popularity": 26.99,
                  "profile_path": "/edward.jpg",
                  "cast_id": 4,
                  "character": "The Narrator",
                  "credit_id": "cast-credit-1",
                  "order": 0
                },
                {
                  "adult": false,
                  "gender": 2,
                  "id": 287,
                  "known_for_department": "Acting",
                  "name": "Brad Pitt",
                  "original_name": "Brad Pitt",
                  "popularity": 45.202,
                  "profile_path": "/brad.jpg",
                  "cast_id": 5,
                  "character": "Tyler Durden",
                  "credit_id": "cast-credit-2",
                  "order": 1
                }
              ],
              "crew": [
                {
                  "adult": false,
                  "gender": 2,
                  "id": 7467,
                  "known_for_department": "Directing",
                  "name": "David Fincher",
                  "original_name": "David Fincher",
                  "popularity": 20.5,
                  "profile_path": "/fincher.jpg",
                  "credit_id": "crew-credit-1",
                  "department": "Directing",
                  "job": "Director"
                },
                {
                  "adult": false,
                  "gender": 2,
                  "id": 7468,
                  "known_for_department": "Camera",
                  "name": "Jeff Cronenweth",
                  "original_name": "Jeff Cronenweth",
                  "popularity": 3.1,
                  "profile_path": null,
                  "credit_id": "crew-credit-2",
                  "department": "Camera",
                  "job": "Director of Photography"
                },
                {
                  "adult": false,
                  "gender": 2,
                  "id": 376,
                  "known_for_department": "Production",
                  "name": "Arnon Milchan",
                  "original_name": "Arnon Milchan",
                  "popularity": 2.931,
                  "profile_path": "/arnon.jpg",
                  "credit_id": "crew-credit-3",
                  "department": "Production",
                  "job": "Executive Producer"
                }
              ]
            }
            """;

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

        [Fact]
        public async Task GetMovieCreditsAsync_MapsApiResponseToCastAndCrew()
        {
            var handler = new RecordingHandler(_ => JsonResponse(CreditsJson));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieCreditsDTO>(await service.GetMovieCreditsAsync(550));

            Assert.Equal(550, result.Id);
            Assert.Equal(2, result.Cast.Count);
            Assert.Equal(3, result.Crew.Count);

            var actor = result.Cast[0];
            Assert.Equal(819, actor.Id);
            Assert.Equal("Edward Norton", actor.Name);
            Assert.Equal("Edward Norton", actor.OriginalName);
            Assert.Equal("Acting", actor.KnownForDepartment);
            Assert.Equal(2, actor.Gender);
            Assert.False(actor.Adult);
            Assert.Equal(26.99, actor.Popularity);
            Assert.Equal("/edward.jpg", actor.ProfilePath);
            Assert.Equal("cast-credit-1", actor.CreditId);
            Assert.Equal(4, actor.CastId);
            Assert.Equal("The Narrator", actor.Character);
            Assert.Equal(0, actor.Order);

            Assert.Equal("Brad Pitt", result.Cast[1].Name);
            Assert.Equal(1, result.Cast[1].Order);

            var director = result.Crew[0];
            Assert.Equal(7467, director.Id);
            Assert.Equal("David Fincher", director.Name);
            Assert.Equal("Directing", director.KnownForDepartment);
            Assert.Equal("crew-credit-1", director.CreditId);
            Assert.Equal("Directing", director.Department);
            Assert.Equal("Director", director.Job);

            // Unknown/null optional values stay null rather than throwing.
            Assert.Null(result.Crew[1].ProfilePath);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(550)]
        [InlineData(999999)]
        public async Task GetMovieCreditsAsync_RequestsCreditsEndpointForId(int id)
        {
            var handler = new RecordingHandler(_ => JsonResponse("""{ "id": 0 }"""));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await service.GetMovieCreditsAsync(id);

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(new Uri($"https://api.themoviedb.org/3/movie/{id}/credits"), request.RequestUri);
        }

        [Fact]
        public async Task GetMovieCreditsAsync_ReturnsNull_WhenApiReturnsNullBody()
        {
            var handler = new RecordingHandler(_ => JsonResponse("null"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = await service.GetMovieCreditsAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetMovieCreditsAsync_ThrowsHttpRequestException_WhenApiReturnsNotFound()
        {
            var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => service.GetMovieCreditsAsync(1));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Theory]
        [InlineData("""{ "id": 7 }""")]
        [InlineData("""{ "id": 7, "cast": [], "crew": [] }""")]
        public async Task GetMovieCreditsAsync_UsesEmptyCollections_WhenCastAndCrewAreMissingOrEmpty(string json)
        {
            var handler = new RecordingHandler(_ => JsonResponse(json));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieCreditsDTO>(await service.GetMovieCreditsAsync(7));

            Assert.Equal(7, result.Id);
            Assert.NotNull(result.Cast);
            Assert.Empty(result.Cast);
            Assert.NotNull(result.Crew);
            Assert.Empty(result.Crew);
        }

        [Fact]
        public async Task GetMovieCreditsAsync_ProvidesDirectorsForFilteringByJob()
        {
            var handler = new RecordingHandler(_ => JsonResponse(CreditsJson));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var credits = Assert.IsType<MovieCreditsDTO>(await service.GetMovieCreditsAsync(550));

            var directors = credits.Crew.Where(c => c.Job == "Director").ToList();

            var director = Assert.Single(directors);
            Assert.Equal("David Fincher", director.Name);
            Assert.DoesNotContain(directors, c => c.Job == "Director of Photography");
        }

        [Fact]
        public async Task GetAllGenresAsync_ReturnsGenresFromApi()
        {
            const string json = """
            {
              "genres": [
                { "id": 28, "name": "Action" },
                { "id": 12, "name": "Adventure" },
                { "id": 16, "name": "Animation" }
              ]
            }
            """;

            var handler = new RecordingHandler(_ => JsonResponse(json));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var genres = await service.GetAllGenresAsync();

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(new Uri("https://api.themoviedb.org/3/genre/movie/list"), request.RequestUri);

            Assert.Collection(genres,
                g => { Assert.Equal(28, g.Id); Assert.Equal("Action", g.Name); },
                g => { Assert.Equal(12, g.Id); Assert.Equal("Adventure", g.Name); },
                g => { Assert.Equal(16, g.Id); Assert.Equal("Animation", g.Name); });
        }

        [Fact]
        public async Task GetMoviesByGenreAsync_MapsPageMetadataAndResults()
        {
            const string json = """
            {
              "page": 2,
              "results": [
                {
                  "adult": false,
                  "backdrop_path": "/backdrop1.jpg",
                  "genre_ids": [28, 12],
                  "id": 101,
                  "original_language": "en",
                  "original_title": "First",
                  "overview": "First overview",
                  "popularity": 123.4,
                  "poster_path": "/poster1.jpg",
                  "release_date": "2024-01-01",
                  "title": "First Movie",
                  "video": false,
                  "vote_average": 7.1,
                  "vote_count": 456
                },
                {
                  "adult": false,
                  "backdrop_path": null,
                  "genre_ids": [28],
                  "id": 202,
                  "original_language": "en",
                  "original_title": "Second",
                  "overview": "",
                  "popularity": 0.5,
                  "poster_path": null,
                  "release_date": "",
                  "title": "Second Movie",
                  "video": true,
                  "vote_average": 0,
                  "vote_count": 0
                }
              ],
              "total_pages": 500,
              "total_results": 10000
            }
            """;

            var handler = new RecordingHandler(_ => JsonResponse(json));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieListPageDTO>(await service.GetMoviesByGenreAsync(28, 2));

            Assert.Equal(2, result.Page);
            Assert.Equal(500, result.TotalPages);
            Assert.Equal(10000, result.TotalResults);
            Assert.Equal(2, result.Results.Count);
            Assert.True(result.HasPreviousPage);
            Assert.True(result.HasNextPage);

            var first = result.Results[0];
            Assert.Equal(101, first.Id);
            Assert.Equal("First Movie", first.Title);
            Assert.Equal("First overview", first.Overview);
            Assert.Equal("2024-01-01", first.ReleaseDate);
            Assert.Equal("/poster1.jpg", first.PosterPath);
            Assert.Equal("/backdrop1.jpg", first.BackdropPath);
            Assert.False(first.Video);
            Assert.Equal(new[] { 28, 12 }, first.GenreIds);
            Assert.Empty(first.Genres);

            var second = result.Results[1];
            Assert.Equal(202, second.Id);
            Assert.True(second.Video);
            Assert.Null(second.PosterPath);
            Assert.Null(second.BackdropPath);
            Assert.Equal(new[] { 28 }, second.GenreIds);
        }

        [Theory]
        [InlineData(28, 1)]
        [InlineData(28, 2)]
        [InlineData(878, 7)]
        public async Task GetMoviesByGenreAsync_RequestsDiscoverEndpointWithGenreAndPage(int genreId, int page)
        {
            var handler = new RecordingHandler(_ => JsonResponse("{}"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await service.GetMoviesByGenreAsync(genreId, page);

            var request = Assert.Single(handler.Requests);
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(
                new Uri($"https://api.themoviedb.org/3/discover/movie?with_genres={genreId}&page={page}"),
                request.RequestUri);
        }

        [Fact]
        public async Task GetMoviesByGenreAsync_DefaultsToFirstPage_WhenPageOmitted()
        {
            var handler = new RecordingHandler(_ => JsonResponse("{}"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await service.GetMoviesByGenreAsync(28);

            var request = Assert.Single(handler.Requests);
            Assert.Equal(
                new Uri("https://api.themoviedb.org/3/discover/movie?with_genres=28&page=1"),
                request.RequestUri);
        }

        [Fact]
        public async Task GetMoviesByGenreAsync_ReturnsEmptyPage_WhenNoMoviesMatch()
        {
            const string json = """
            { "page": 1, "results": [], "total_pages": 0, "total_results": 0 }
            """;

            var handler = new RecordingHandler(_ => JsonResponse(json));
            var service = new TMDBServiceAccess(CreateClient(handler));

            var result = Assert.IsType<MovieListPageDTO>(await service.GetMoviesByGenreAsync(28));

            Assert.Equal(1, result.Page);
            Assert.Empty(result.Results);
            Assert.False(result.HasPreviousPage);
            Assert.False(result.HasNextPage);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(28, 0)]
        [InlineData(28, -5)]
        public async Task GetMoviesByGenreAsync_Throws_WhenArgumentsAreOutOfRange(int genreId, int page)
        {
            var handler = new RecordingHandler(_ => JsonResponse("{}"));
            var service = new TMDBServiceAccess(CreateClient(handler));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.GetMoviesByGenreAsync(genreId, page));

            Assert.Empty(handler.Requests);
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