using AI4NGGameScoresLambda.Models.Dtos;

namespace AI4NGGameScoresLambda.Models.Responses;

public sealed class GetGameScoresResponse
{
    public string GameKey { get; set; } = default!;
    public string? ClassifierId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public IReadOnlyList<GameScoreDto> Items { get; set; } = Array.Empty<GameScoreDto>();
    public string? NextToken { get; set; }
}