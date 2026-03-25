namespace AI4NGGameScoresLambda.Models.Dtos;

public sealed class ParticipantScoreProfileDto
{
    public string UserId { get; set; } = default!;
    public string DisplayUsername { get; set; } = default!;
    public string Cohort { get; set; } = default!;
}