using Amazon.DynamoDBv2.Model;
using AI4NGGameScoresLambda.Helpers;
using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Models.Dtos;
using AI4NGGameScoresLambda.Models.Entities;
using AI4NGGameScoresLambda.Models.Queries;
using AI4NGGameScoresLambda.Models.Requests;

namespace AI4NGGameScoresLambda.Services;

public sealed class GameScoresService : IGameScoresService
{
    private readonly IGameScoresRepository _repository;
    private readonly IParticipantScoreProfileResolver _profileResolver;

    public GameScoresService(
        IGameScoresRepository repository,
        IParticipantScoreProfileResolver profileResolver)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _profileResolver = profileResolver ?? throw new ArgumentNullException(nameof(profileResolver));
    }

    public async Task<CreateGameScoreResultDto> CreateScoreAsync(
        string userId,
        CreateGameScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        ValidateCreateRequest(request);

        var profile = await _profileResolver.ResolveAsync(
            userId,
            request.ExperimentId,
            cancellationToken);

        var scoreId = Guid.NewGuid().ToString("N");
        var recordedAtUtc = EnsureUtc(DateTime.UtcNow);
        var playedAtUtc = EnsureUtc(request.PlayedAtUtc);

        var scoreItem = new GameScoreItem
        {
            PK = DynamoKeyBuilder.BuildScorePk(userId),
            SK = DynamoKeyBuilder.BuildScoreSk(request.GameKey, playedAtUtc, scoreId),

            GSI1PK = DynamoKeyBuilder.BuildLeaderboardGsi1Pk(request.GameKey, profile.Cohort),
            GSI1SK = DynamoKeyBuilder.BuildLeaderboardGsi1Sk(request.ScoreValue, playedAtUtc, userId, scoreId),

            GSI2PK = DynamoKeyBuilder.BuildUserGameGsi2Pk(userId, request.GameKey),
            GSI2SK = DynamoKeyBuilder.BuildUserGameGsi2Sk(playedAtUtc, scoreId),

            GSI3PK = DynamoKeyBuilder.BuildUserGameClassifierGsi3Pk(userId, request.GameKey, request.ClassifierId),
            GSI3SK = DynamoKeyBuilder.BuildUserGameClassifierGsi3Sk(playedAtUtc, scoreId),

            GSI4PK = DynamoKeyBuilder.BuildLeaderboardGsi4Pk(request.GameKey, request.ExperimentId),
            GSI4SK = DynamoKeyBuilder.BuildLeaderboardGsi1Sk(request.ScoreValue, playedAtUtc, userId, scoreId),

            Type = "GameScore",
            ScoreId = scoreId,
            ExperimentId = request.ExperimentId.Trim(),
            Cohort = profile.Cohort,
            GameKey = DynamoKeyBuilder.NormaliseKey(request.GameKey),
            UserId = userId.Trim(),
            Username = profile.DisplayUsername,
            ClassifierId = DynamoKeyBuilder.NormaliseKey(request.ClassifierId),
            ScoreValue = request.ScoreValue,
            ScoreUnit = string.IsNullOrWhiteSpace(request.ScoreUnit) ? "points" : request.ScoreUnit.Trim(),
            PlayedAtUtc = playedAtUtc,
            RecordedAtUtc = recordedAtUtc,
            SessionOccurrenceKey = string.IsNullOrWhiteSpace(request.SessionOccurrenceKey) ? null : request.SessionOccurrenceKey.Trim(),
            TaskKey = string.IsNullOrWhiteSpace(request.TaskKey) ? null : DynamoKeyBuilder.NormaliseKey(request.TaskKey),
            ClientSubmissionId = request.ClientSubmissionId.Trim(),
            Metadata = request.Metadata
        };

        var submissionLockItem = new ScoreSubmissionLockItem
        {
            PK = DynamoKeyBuilder.BuildSubmissionLockPk(request.ClientSubmissionId),
            SK = DynamoKeyBuilder.BuildSubmissionLockSk(),
            Type = "ScoreSubmissionLock",
            ClientSubmissionId = request.ClientSubmissionId.Trim(),
            ScoreId = scoreId,
            UserId = userId.Trim(),
            RecordedAtUtc = recordedAtUtc
        };

        try
        {
            await _repository.CreateScoreAsync(scoreItem, submissionLockItem, cancellationToken);

            return new CreateGameScoreResultDto
            {
                ScoreId = scoreId,
                Created = true,
                IdempotentReplay = false,
                RecordedAtUtc = recordedAtUtc
            };
        }
        catch (TransactionCanceledException ex) when (IsIdempotencyCollision(ex))
        {
            var existingLock = await _repository.GetSubmissionLockAsync(
                request.ClientSubmissionId,
                cancellationToken);

            return new CreateGameScoreResultDto
            {
                ScoreId = existingLock?.ScoreId ?? string.Empty,
                Created = false,
                IdempotentReplay = true,
                RecordedAtUtc = existingLock?.RecordedAtUtc ?? recordedAtUtc
            };
        }
    }

    public async Task<PagedResultDto<GameScoreDto>> GetMyScoresAsync(
        string userId,
        GameScoreHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(query);

        var limit = ClampLimit(query.Limit, 20, 100);
        var gameKey = DynamoKeyBuilder.NormaliseKey(query.GameKey);

        PagedResultDto<GameScoreItem> page;

        if (string.IsNullOrWhiteSpace(query.ClassifierId))
        {
            page = await _repository.GetScoresByUserAndGameAsync(
                userId,
                gameKey,
                query.FromUtc,
                query.ToUtc,
                limit,
                query.NextToken,
                cancellationToken);
        }
        else
        {
            page = await _repository.GetScoresByUserGameAndClassifierAsync(
                userId,
                gameKey,
                query.ClassifierId,
                query.FromUtc,
                query.ToUtc,
                limit,
                query.NextToken,
                cancellationToken);
        }

        return new PagedResultDto<GameScoreDto>
        {
            Items = page.Items.Select(MapGameScoreDto).ToArray(),
            NextToken = page.NextToken
        };
    }

    public async Task<IReadOnlyList<GameScoreDto>> GetMyBestScoresAsync(
        string userId,
        GameScoreBestQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(query);

        var allItems = new List<GameScoreDto>();
        string? nextToken = null;

        do
        {
            var page = await GetMyScoresAsync(userId, new GameScoreHistoryQuery
            {
                GameKey = query.GameKey,
                ClassifierId = query.ClassifierId,
                FromUtc = query.FromUtc,
                ToUtc = query.ToUtc,
                Limit = 100,
                NextToken = nextToken
            }, cancellationToken);

            allItems.AddRange(page.Items);
            nextToken = page.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return allItems
            .OrderByDescending(x => x.ScoreValue)
            .ThenByDescending(x => x.PlayedAtUtc)
            .Take(ClampLimit(query.Limit, 10, 100))
            .ToArray();
    }

    public async Task<PagedResultDto<LeaderboardEntryDto>> GetLeaderboardAsync(
        LeaderboardQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var profile = await _profileResolver.ResolveAsync(
            query.UserId,
            query.ExperimentId,
            cancellationToken);

        var page = await _repository.GetLeaderboardAsync(
            query.GameKey,
            profile.Cohort,
            ClampLimit(query.Limit, 10, 100),
            query.NextToken,
            cancellationToken);

        var items = page.Items
            .Select(MapLeaderboardEntryDto)
            .ToArray();

        for (var i = 0; i < items.Length; i++)
            items[i].Rank = i + 1;

        return new PagedResultDto<LeaderboardEntryDto>
        {
            Items = items,
            NextToken = page.NextToken
        };
    }

    public async Task<PagedResultDto<LeaderboardEntryDto>> GetGlobalLeaderboardAsync(
        GlobalLeaderboardQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = await _repository.GetGlobalLeaderboardAsync(
            query.GameKey,
            query.ExperimentId,
            ClampLimit(query.Limit, 10, 100),
            query.NextToken,
            cancellationToken);

        var items = page.Items
            .Select(MapLeaderboardEntryDto)
            .ToArray();

        for (var i = 0; i < items.Length; i++)
            items[i].Rank = i + 1;

        return new PagedResultDto<LeaderboardEntryDto>
        {
            Items = items,
            NextToken = page.NextToken
        };
    }

    private static void ValidateCreateRequest(CreateGameScoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExperimentId))
            throw new ArgumentException("ExperimentId is required.", nameof(request.ExperimentId));

        if (string.IsNullOrWhiteSpace(request.GameKey))
            throw new ArgumentException("GameKey is required.", nameof(request.GameKey));

        if (string.IsNullOrWhiteSpace(request.ClassifierId))
            throw new ArgumentException("ClassifierId is required.", nameof(request.ClassifierId));

        if (string.IsNullOrWhiteSpace(request.ClientSubmissionId))
            throw new ArgumentException("ClientSubmissionId is required.", nameof(request.ClientSubmissionId));

        if (request.ScoreValue < 0)
            throw new ArgumentOutOfRangeException(nameof(request.ScoreValue), "ScoreValue cannot be negative.");
    }

    private static GameScoreDto MapGameScoreDto(GameScoreItem item)
    {
        return new GameScoreDto
        {
            ScoreId = item.ScoreId,
            ExperimentId = item.ExperimentId,
            Cohort = item.Cohort,
            GameKey = item.GameKey,
            UserId = item.UserId,
            Username = item.Username,
            ClassifierId = item.ClassifierId,
            ScoreValue = item.ScoreValue,
            ScoreUnit = item.ScoreUnit,
            PlayedAtUtc = item.PlayedAtUtc,
            RecordedAtUtc = item.RecordedAtUtc,
            SessionOccurrenceKey = item.SessionOccurrenceKey,
            TaskKey = item.TaskKey,
            Metadata = item.Metadata
        };
    }

    private static LeaderboardEntryDto MapLeaderboardEntryDto(GameScoreItem item)
    {
        return new LeaderboardEntryDto
        {
            ScoreId = item.ScoreId,
            Username = item.Username,
            ClassifierId = item.ClassifierId,
            ScoreValue = item.ScoreValue,
            ScoreUnit = item.ScoreUnit,
            PlayedAtUtc = item.PlayedAtUtc
        };
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static int ClampLimit(int value, int defaultValue, int maxValue)
    {
        if (value <= 0)
            return defaultValue;

        return Math.Min(value, maxValue);
    }

    private static bool IsIdempotencyCollision(TransactionCanceledException ex)
    {
        return ex.CancellationReasons?.Any(r => string.Equals(r.Code, "ConditionalCheckFailed", StringComparison.Ordinal)) == true;
    }
}
