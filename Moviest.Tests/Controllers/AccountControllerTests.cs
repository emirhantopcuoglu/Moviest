using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moviest.Controllers;
using Moviest.Data;
using Moviest.Models;
using Moviest.Services;
using Moviest.Tests.Infrastructure;

namespace Moviest.Tests.Controllers;

public class AccountControllerTests
{
    private static IdentityContext CreateContext() =>
        new(new DbContextOptionsBuilder<IdentityContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static IEmailSender NoOpEmailSender() => new StubEmailSender();
    private static IOptions<EmailSettings> NoOpEmailSettings() => Options.Create(new EmailSettings());

    [Fact]
    public async Task Register_WhenCreationSucceeds_RedirectsToMovies()
    {
        var userManager = new TestUserManager
        {
            CreateAsyncHandler = (user, _) =>
            {
                user.Id = "new-user";
                return Task.FromResult(IdentityResult.Success);
            }
        };
        var signInManager = new TestSignInManager(userManager);
        var signInCalled = false;
        signInManager.SignInAsyncHandler = (_, _) =>
        {
            signInCalled = true;
            return Task.CompletedTask;
        };

        using var ctx = CreateContext();
        var controller = new AccountController(userManager, signInManager, ctx, NoOpEmailSender(), NoOpEmailSettings());
        ControllerTestContext.AttachHttpContext(controller);

        var result = await controller.Register(new RegisterViewModel
        {
            Username = "tester",
            Email = "tester@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Movies", redirect.ControllerName);
        Assert.True(signInCalled);
    }

    [Fact]
    public async Task Login_WhenLockedOut_ReturnsViewWithModelError()
    {
        var userManager = new TestUserManager();
        var signInManager = new TestSignInManager(userManager)
        {
            PasswordSignInAsyncHandler = (_, _, _, _) => Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.LockedOut)
        };

        using var ctx = CreateContext();
        var controller = new AccountController(userManager, signInManager, ctx, NoOpEmailSender(), NoOpEmailSettings());
        ControllerTestContext.AttachHttpContext(controller);

        var result = await controller.Login(new LoginViewModel
        {
            Username = "tester",
            Password = "Password1",
            RememberMe = false
        });

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewResult.Model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
    }
}
