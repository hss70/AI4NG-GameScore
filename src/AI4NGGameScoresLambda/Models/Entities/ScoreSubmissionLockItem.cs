namespace AI4NGGameScoresLambda.Models.Entities;

public sealed class ScoreSubmissionLockItem
{
    public string PK { get; set; } = default!;
    public string SK { get; set; } = "SCORE";

    public string Type { get; set; } = "ScoreSubmissionLock";

    public string ClientSubmissionId { get; set; } = default!;
    public string ScoreId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime RecordedAtUtc { get; set; }
}