namespace AI4NGGameScoresLambda.Models.Dtos;

public sealed class GameScoreDto
{
    public string ScoreId { get; set; } = default!;
    public string ExperimentId { get; set; } = default!;
    public string Cohort { get; set; } = default!;
    public string GameKey { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string ClassifierId { get; set; } = default!;
    public int ScoreValue { get; set; }
    public string ScoreUnit { get; set; } = default!;
    public DateTime PlayedAtUtc { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    public string? SessionOccurrenceKey { get; set; }
    public string? TaskKey { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}