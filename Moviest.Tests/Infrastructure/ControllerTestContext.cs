using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Moviest.Tests.Infrastructure;

internal static class ControllerTestContext
{
    public static void AttachHttpContext(Controller controller, ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user ?? CreateAnonymousUser()
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
        controller.Url = new TestUrlHelper();
    }

    public static ClaimsPrincipal CreateAuthenticatedUser(string userId = "user-1", string userName = "tester")
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName)
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateAnonymousUser() => new(new ClaimsIdentity());
}
