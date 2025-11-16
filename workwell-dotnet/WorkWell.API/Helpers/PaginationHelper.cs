using WorkWell.Application.DTOs;

namespace WorkWell.API.Helpers;

public static class PaginationHelper
{
    public static PagedResponse<T> CreatePagedResponse<T>(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords)
    {
        var pagedData = data
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PagedResponse<T>
        {
            Data = pagedData,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TotalRecords = totalRecords
        };
    }
}

