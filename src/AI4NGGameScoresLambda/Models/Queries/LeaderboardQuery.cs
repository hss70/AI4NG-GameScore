namespace AI4NGGameScoresLambda.Models.Queries;

public sealed class LeaderboardQuery
{
    public string GameKey { get; set; } = default!;
    public string ExperimentId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public int Limit { get; set; } = 10;
    public string? NextToken { get; set; }
}