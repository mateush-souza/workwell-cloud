using Microsoft.AspNetCore.Mvc;
using WorkWell.Application.DTOs;

namespace WorkWell.API.Helpers;

public static class HateoasHelper
{
    public static List<Link> GenerateLinks(IUrlHelper urlHelper, string routeName, object? routeValues, params (string rel, string method)[] actions)
    {
        var links = new List<Link>();

        foreach (var (rel, method) in actions)
        {
            var href = urlHelper.Link(routeName, routeValues);
            if (!string.IsNullOrEmpty(href))
            {
                links.Add(new Link
                {
                    Href = href,
                    Rel = rel,
                    Method = method
                });
            }
        }

        return links;
    }

    public static List<Link> GenerateUserLinks(IUrlHelper urlHelper, int userId)
    {
        return new List<Link>
        {
            new() { Href = urlHelper.Link("GetUsuario", new { id = userId })!, Rel = "self", Method = "GET" },
            new() { Href = urlHelper.Link("UpdateUsuario", new { id = userId })!, Rel = "update", Method = "PUT" },
            new() { Href = urlHelper.Link("DeleteUsuario", new { id = userId })!, Rel = "delete", Method = "DELETE" },
            new() { Href = urlHelper.Link("GetUserCheckins", new { usuarioId = userId })!, Rel = "checkins", Method = "GET" }
        };
    }

    public static List<Link> GeneratePaginationLinks(
        IUrlHelper urlHelper,
        string routeName,
        int pageNumber,
        int pageSize,
        int totalPages,
        object? additionalParams = null)
    {
        var links = new List<Link>();

        // Self
        var selfParams = MergeRouteValues(new { pageNumber, pageSize }, additionalParams);
        links.Add(new Link
        {
            Href = urlHelper.Link(routeName, selfParams)!,
            Rel = "self",
            Method = "GET"
        });

        // First
        var firstParams = MergeRouteValues(new { pageNumber = 1, pageSize }, additionalParams);
        links.Add(new Link
        {
            Href = urlHelper.Link(routeName, firstParams)!,
            Rel = "first",
            Method = "GET"
        });

        // Previous
        if (pageNumber > 1)
        {
            var prevParams = MergeRouteValues(new { pageNumber = pageNumber - 1, pageSize }, additionalParams);
            links.Add(new Link
            {
                Href = urlHelper.Link(routeName, prevParams)!,
                Rel = "previous",
                Method = "GET"
            });
        }

        // Next
        if (pageNumber < totalPages)
        {
            var nextParams = MergeRouteValues(new { pageNumber = pageNumber + 1, pageSize }, additionalParams);
            links.Add(new Link
            {
                Href = urlHelper.Link(routeName, nextParams)!,
                Rel = "next",
                Method = "GET"
            });
        }

        // Last
        var lastParams = MergeRouteValues(new { pageNumber = totalPages, pageSize }, additionalParams);
        links.Add(new Link
        {
            Href = urlHelper.Link(routeName, lastParams)!,
            Rel = "last",
            Method = "GET"
        });

        return links;
    }

    private static object MergeRouteValues(object primary, object? additional)
    {
        if (additional == null) return primary;

        var merged = new Dictionary<string, object?>();

        foreach (var prop in primary.GetType().GetProperties())
        {
            merged[prop.Name] = prop.GetValue(primary);
        }

        foreach (var prop in additional.GetType().GetProperties())
        {
            merged[prop.Name] = prop.GetValue(additional);
        }

        return merged;
    }
}

