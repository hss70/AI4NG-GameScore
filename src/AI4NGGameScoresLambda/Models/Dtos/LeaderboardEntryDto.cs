namespace AI4NGGameScoresLambda.Models.Dtos;

public sealed class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string ScoreId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string ClassifierId { get; set; } = default!;
    public int ScoreValue { get; set; }
    public string ScoreUnit { get; set; } = default!;
    public DateTime PlayedAtUtc { get; set; }
}