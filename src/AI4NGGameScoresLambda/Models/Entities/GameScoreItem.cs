using System.Text.Json.Serialization;

namespace AI4NGGameScoresLambda.Models.Entities;

public sealed class GameScoreItem
{
    public string PK { get; set; } = default!;
    public string SK { get; set; } = default!;

    public string GSI1PK { get; set; } = default!;
    public string GSI1SK { get; set; } = default!;

    public string GSI2PK { get; set; } = default!;
    public string GSI2SK { get; set; } = default!;

    public string GSI3PK { get; set; } = default!;
    public string GSI3SK { get; set; } = default!;
    public string GSI4PK { get; set; } = default!;
    public string GSI4SK { get; set; } = default!;

    public string Type { get; set; } = "GameScore";

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

    public string ClientSubmissionId { get; set; } = default!;

    public Dictionary<string, object>? Metadata { get; set; }
}