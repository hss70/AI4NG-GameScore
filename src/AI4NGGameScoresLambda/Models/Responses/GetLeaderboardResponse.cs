using AI4NGGameScoresLambda.Models.Dtos;

namespace AI4NGGameScoresLambda.Models.Responses;

public sealed class GetLeaderboardResponse
{
    public string GameKey { get; set; } = default!;
    public string ExperimentId { get; set; } = default!;
    public IReadOnlyList<LeaderboardEntryDto> Items { get; set; } = Array.Empty<LeaderboardEntryDto>();
    public string? NextToken { get; set; }
}