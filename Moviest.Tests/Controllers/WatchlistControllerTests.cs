using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moviest.Controllers;
using Moviest.Data;
using Moviest.Models;
using Moviest.Tests.Infrastructure;

namespace Moviest.Tests.Controllers;

public class WatchlistControllerTests
{
    [Fact]
    public async Task Add_WhenAjaxRequestAndModelInvalid_ReturnsBadRequestJson()
    {
        await using var dbContext = CreateDbContext();
        var userManager = new TestUserManager();
        var movieService = new TestMovieService();
        var controller = new WatchlistController(dbContext, userManager, movieService);
        ControllerTestContext.AttachHttpContext(controller, ControllerTestContext.CreateAuthenticatedUser());
        controller.ModelState.AddModelError("MovieId", "required");
        controller.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        var result = await controller.Add(new AddToWatchlistRequest());

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, controller.Response.StatusCode);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task Add_UsesMovieServiceDataAndIgnoresExternalReturnUrl()
    {
        await using var dbContext = CreateDbContext();
        var userManager = new TestUserManager();
        var movieService = new TestMovieService
        {
            GetMovieDetailsHandler = movieId => Task.FromResult(new MovieDetails
            {
                Id = movieId,
                Title = "Server Title",
                Overview = "Overview",
                Poster = "/poster.jpg",
                ReleaseDate = "2022-03-04",
                Genres = new List<Genre>(),
                SimilarMovies = new List<Movie>()
            })
        };

        var controller = new WatchlistController(dbContext, userManager, movieService);
        ControllerTestContext.AttachHttpContext(controller, ControllerTestContext.CreateAuthenticatedUser("user-42"));

        var result = await controller.Add(new AddToWatchlistRequest
        {
            MovieId = 99,
            ReturnUrl = "https://malicious.example"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Movies", redirect.ControllerName);

        var savedItem = await dbContext.WatchlistItems.SingleAsync();
        Assert.Equal("Server Title", savedItem.MovieTitle);
        Assert.Equal("2022", savedItem.MovieYear);
        Assert.Equal("/poster.jpg", savedItem.MoviePoster);
        Assert.Equal("user-42", savedItem.UserId);
    }

    [Fact]
    public async Task Add_WhenReturnUrlIsLocal_RedirectsBackLocally()
    {
        await using var dbContext = CreateDbContext();
        var userManager = new TestUserManager();
        var movieService = new TestMovieService
        {
            GetMovieDetailsHandler = movieId => Task.FromResult(new MovieDetails
            {
                Id = movieId,
                Title = "Server Title",
                Overview = "Overview",
                Poster = "/poster.jpg",
                ReleaseDate = "2022-03-04",
                Genres = new List<Genre>(),
                SimilarMovies = new List<Movie>()
            })
        };

        var controller = new WatchlistController(dbContext, userManager, movieService);
        ControllerTestContext.AttachHttpContext(controller, ControllerTestContext.CreateAuthenticatedUser("user-42"));

        var result = await controller.Add(new AddToWatchlistRequest
        {
            MovieId = 77,
            ReturnUrl = "/Movies/Search?query=test"
        });

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Movies/Search?query=test", redirect.Url);
    }

    [Fact]
    public async Task Index_AppliesStatusAndQueryFilters()
    {
        await using var dbContext = CreateDbContext();
        dbContext.WatchlistItems.AddRange(
            new WatchlistItem { UserId = "user-42", MovieId = 1, MovieTitle = "Alpha", IsWatched = true, AddedAt = DateTime.UtcNow.AddDays(-1) },
            new WatchlistItem { UserId = "user-42", MovieId = 2, MovieTitle = "Beta", IsWatched = false, AddedAt = DateTime.UtcNow },
            new WatchlistItem { UserId = "other-user", MovieId = 3, MovieTitle = "Alpha 2", IsWatched = true, AddedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var controller = new WatchlistController(dbContext, new TestUserManager(), new TestMovieService());
        ControllerTestContext.AttachHttpContext(controller, ControllerTestContext.CreateAuthenticatedUser("user-42"));

        var result = await controller.Index("Alpha", "watched", "recent");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WatchlistIndexViewModel>(viewResult.Model);
        Assert.Single(model.Items);
        Assert.Equal("Alpha", model.Items[0].MovieTitle);
        Assert.Equal(2, model.TotalCount);
    }

    private static IdentityContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new IdentityContext(options);
    }
}
