namespace AI4NGGameScoresLambda.Models.Requests;

public sealed class CreateGameScoreRequest
{
    public string ExperimentId { get; set; } = default!;
    public string GameKey { get; set; } = default!;
    public string ClassifierId { get; set; } = default!;
    public int ScoreValue { get; set; }
    public string ScoreUnit { get; set; } = "points";
    public DateTime PlayedAtUtc { get; set; }
    public string ClientSubmissionId { get; set; } = default!;
    public string? SessionOccurrenceKey { get; set; }
    public string? TaskKey { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}