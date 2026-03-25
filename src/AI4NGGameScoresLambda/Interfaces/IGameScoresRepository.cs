using AI4NGGameScoresLambda.Models.Dtos;
using AI4NGGameScoresLambda.Models.Entities;

namespace AI4NGGameScoresLambda.Interfaces;

public interface IGameScoresRepository
{
    Task CreateScoreAsync(
        GameScoreItem scoreItem,
        ScoreSubmissionLockItem submissionLockItem,
        CancellationToken cancellationToken = default);

    Task<ScoreSubmissionLockItem?> GetSubmissionLockAsync(
        string clientSubmissionId,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<GameScoreItem>> GetScoresByUserAndGameAsync(
        string userId,
        string gameKey,
        DateTime? fromUtc,
        DateTime? toUtc,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<GameScoreItem>> GetScoresByUserGameAndClassifierAsync(
        string userId,
        string gameKey,
        string classifierId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<GameScoreItem>> GetLeaderboardAsync(
        string gameKey,
        string cohort,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default);
    Task<PagedResultDto<GameScoreItem>> GetGlobalLeaderboardAsync(
    string gameKey,
    string experimentId,
    int limit,
    string? nextToken,
    CancellationToken cancellationToken = default);
}