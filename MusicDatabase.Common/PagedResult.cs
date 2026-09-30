namespace MusicDatabase.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, bool HasMore)
{
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToArray(), HasMore);
}