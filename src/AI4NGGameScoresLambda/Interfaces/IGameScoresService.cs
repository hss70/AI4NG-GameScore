using AI4NGGameScoresLambda.Models.Dtos;
using AI4NGGameScoresLambda.Models.Queries;
using AI4NGGameScoresLambda.Models.Requests;

namespace AI4NGGameScoresLambda.Interfaces;

public interface IGameScoresService
{
    Task<CreateGameScoreResultDto> CreateScoreAsync(
        string userId,
        CreateGameScoreRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<GameScoreDto>> GetMyScoresAsync(
        string userId,
        GameScoreHistoryQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameScoreDto>> GetMyBestScoresAsync(
        string userId,
        GameScoreBestQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<LeaderboardEntryDto>> GetLeaderboardAsync(
        LeaderboardQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(
        GlobalLeaderboardQuery query,
        CancellationToken cancellationToken = default);
}