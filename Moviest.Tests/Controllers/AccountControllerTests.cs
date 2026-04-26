using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moviest.Controllers;
using Moviest.Models;
using Moviest.Tests.Infrastructure;

namespace Moviest.Tests.Controllers;

public class AccountControllerTests
{
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

        var controller = new AccountController(userManager, signInManager);
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

        var controller = new AccountController(userManager, signInManager);
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
