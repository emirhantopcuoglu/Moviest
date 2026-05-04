using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moviest.Controllers;
using Moviest.Data;
using Moviest.Models;
using Moviest.Tests.Infrastructure;

namespace Moviest.Tests.Controllers;

public class MoviesControllerTests
{
    private static IdentityContext CreateContext() =>
        new(new DbContextOptionsBuilder<IdentityContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Details_WhenMainMovieCallFails_Returns503()
    {
        var movieService = new TestMovieService
        {
            GetMovieDetailsHandler = _ => throw new HttpRequestException("tmdb down")
        };

        using var ctx = CreateContext();
        var controller = new MoviesController(movieService, ctx)
            .WithAuthenticatedUser();

        var result = await controller.Details(42);

        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(503, statusResult.StatusCode);
    }

    [Fact]
    public async Task Details_WhenSideCallsFail_UsesFallbackCollections()
    {
        var movieService = new TestMovieService
        {
            GetMovieDetailsHandler = _ => Task.FromResult(new MovieDetails
            {
                Id = 42,
                Title = "Movie",
                Overview = "Overview",
                Poster = "/poster.jpg",
                ReleaseDate = "2024-01-01",
                Genres = new List<Genre>(),
                SimilarMovies = new List<Movie>()
            }),
            GetTrailerHandler = _ => throw new HttpRequestException("trailers down"),
            GetSimilarMoviesHandler = (_, _) => throw new HttpRequestException("similar down"),
            GetMovieCreditsHandler = _ => throw new HttpRequestException("credits down")
        };

        using var ctx = CreateContext();
        var controller = new MoviesController(movieService, ctx)
            .WithAuthenticatedUser();

        var result = await controller.Details(42);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovieDetails>(viewResult.Model);
        Assert.Empty(model.Videos);
        Assert.Empty(model.SimilarMovies);
        Assert.Empty(model.Cast);
    }

    [Fact]
    public async Task Search_AppliesRatingFilterAndSortOrder()
    {
        var movieService = new TestMovieService();
        movieService.SearchMoviesHandler = (_, _) => Task.FromResult(new MovieResponse
        {
            Page = 1,
            TotalPages = 1,
            Movies =
            [
                new Movie { Id = 1, Title = "B", VoteAverage = 8.4, ReleaseDate = "2024-01-01" },
                new Movie { Id = 2, Title = "A", VoteAverage = 6.1, ReleaseDate = "2023-01-01" },
                new Movie { Id = 3, Title = "C", VoteAverage = 9.0, ReleaseDate = "2022-01-01" }
            ]
        });

        using var ctx = CreateContext();
        var controller = new MoviesController(movieService, ctx)
            .WithAuthenticatedUser();

        var result = await controller.Search("test", 1, "rating_desc", 7.0);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<SearchResultsViewModel>(viewResult.Model);
        Assert.Equal([3, 1], model.Movies.Select(movie => movie.Id));
    }
}
