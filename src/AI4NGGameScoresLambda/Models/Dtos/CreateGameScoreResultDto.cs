namespace AI4NGGameScoresLambda.Models.Dtos;

public sealed class CreateGameScoreResultDto
{
    public string ScoreId { get; set; } = default!;
    public bool Created { get; set; }
    public bool IdempotentReplay { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}