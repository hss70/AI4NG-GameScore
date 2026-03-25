namespace AI4NGGameScoresLambda.Models.Dtos;

public sealed class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public string? NextToken { get; set; }
}