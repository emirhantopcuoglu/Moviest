using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Moviest.Tests.Infrastructure;

internal sealed class TestUserManager : UserManager<IdentityUser>
{
    public Func<IdentityUser, string, Task<IdentityResult>>? CreateAsyncHandler { get; set; }
    public Func<ClaimsPrincipal, Task<IdentityUser?>>? GetUserAsyncHandler { get; set; }

    public TestUserManager()
        : base(
            new TestUserStore(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<IdentityUser>>.Instance)
    {
    }

    public override Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        => CreateAsyncHandler?.Invoke(user, password) ?? Task.FromResult(IdentityResult.Success);

    public override Task<IdentityUser?> GetUserAsync(ClaimsPrincipal principal)
        => GetUserAsyncHandler?.Invoke(principal) ?? Task.FromResult<IdentityUser?>(null);
}

internal sealed class TestSignInManager : SignInManager<IdentityUser>
{
    public Func<string, string, bool, bool, Task<SignInResult>>? PasswordSignInAsyncHandler { get; set; }
    public Func<IdentityUser, bool, Task>? SignInAsyncHandler { get; set; }

    public TestSignInManager(TestUserManager userManager)
        : base(
            userManager,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new TestUserClaimsPrincipalFactory(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            NullLogger<SignInManager<IdentityUser>>.Instance,
            new AuthenticationSchemeProvider(Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions())),
            new DefaultUserConfirmation<IdentityUser>())
    {
    }

    public override Task SignInAsync(IdentityUser user, bool isPersistent, string? authenticationMethod = null)
        => SignInAsyncHandler?.Invoke(user, isPersistent) ?? Task.CompletedTask;

    public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        => PasswordSignInAsyncHandler?.Invoke(userName, password, isPersistent, lockoutOnFailure)
           ?? Task.FromResult(SignInResult.Success);
}

internal sealed class TestUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<IdentityUser>
{
    public Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? "user"),
            new Claim(ClaimTypes.Name, user.UserName ?? "user")
        ], "TestAuth");

        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}

internal sealed class TestUserStore : IUserPasswordStore<IdentityUser>
{
    public void Dispose()
    {
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id ?? string.Empty);
    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<IdentityUser?>(null);
    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<IdentityUser?>(null);
    public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<string?> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
    public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(true);
}
