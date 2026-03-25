namespace AI4NGGameScoresLambda.Models.Responses;

public sealed class CreateGameScoreResponse
{
    public string ScoreId { get; set; } = default!;
    public bool Created { get; set; }
    public bool IdempotentReplay { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}