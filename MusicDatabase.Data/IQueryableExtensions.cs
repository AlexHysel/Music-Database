using Microsoft.EntityFrameworkCore;
using MusicDatabase.Common;

namespace MusicDatabase.Data;

public static class IQueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int skip, int take)
    {
        var items = await query.Skip(skip).Take(take + 1).ToListAsync();
        bool hasMore = items.Count > take;
        if (hasMore) items.RemoveAt(items.Count - 1);
        return new PagedResult<T>(items, hasMore);
    }
}