namespace AI4NGGameScoresLambda.Models.Queries;

public sealed class GameScoreHistoryQuery
{
    public string GameKey { get; set; } = default!;
    public string? ClassifierId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Limit { get; set; } = 20;
    public string? NextToken { get; set; }
}