using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Moviest.Tests.Infrastructure;

internal sealed class TestUrlHelper : IUrlHelper
{
    public ActionContext ActionContext => new();

    public string? Action(UrlActionContext actionContext) => null;

    public string? Content(string? contentPath) => contentPath;

    public bool IsLocalUrl(string? url)
        => !string.IsNullOrWhiteSpace(url)
           && (url.StartsWith('/') && !url.StartsWith("//") && !url.StartsWith("/\\"));

    public string? Link(string? routeName, object? values) => null;

    public string? RouteUrl(UrlRouteContext routeContext) => null;
}
