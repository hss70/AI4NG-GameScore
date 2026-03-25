using System.Globalization;

namespace AI4NGGameScoresLambda.Helpers;

public static class DynamoKeyBuilder
{
    private const long MaxInvertedScore = 9_999_999_999L;

    public static string BuildScorePk(string userId)
        => $"USER#{Require(userId, nameof(userId))}";

    public static string BuildScoreSk(string gameKey, DateTime playedAt, string scoreId)
        => $"GAME#{NormaliseKey(gameKey)}#PLAYEDAT#{ToIsoUtc(playedAt)}#SCORE#{Require(scoreId, nameof(scoreId))}";

    public static string BuildLeaderboardGsi1Pk(string gameKey, string cohort)
        => $"GAME#{NormaliseKey(gameKey)}#COHORT#{NormaliseKey(cohort)}";

    public static string BuildLeaderboardGsi1Sk(int scoreValue, DateTime playedAt, string userId, string scoreId)
        => $"SCORE#{InvertScore(scoreValue):D10}#PLAYEDAT#{ToIsoUtc(playedAt)}#USER#{Require(userId, nameof(userId))}#SCORE#{Require(scoreId, nameof(scoreId))}";

    public static string BuildUserGameGsi2Pk(string userId, string gameKey)
        => $"USER#{Require(userId, nameof(userId))}#GAME#{NormaliseKey(gameKey)}";

    public static string BuildUserGameGsi2Sk(DateTime playedAt, string scoreId)
        => $"PLAYEDAT#{ToIsoUtc(playedAt)}#SCORE#{Require(scoreId, nameof(scoreId))}";

    public static string BuildUserGameClassifierGsi3Pk(string userId, string gameKey, string classifierId)
        => $"USER#{Require(userId, nameof(userId))}#GAME#{NormaliseKey(gameKey)}#CLASSIFIER#{NormaliseKey(classifierId)}";

    public static string BuildUserGameClassifierGsi3Sk(DateTime playedAt, string scoreId)
        => $"PLAYEDAT#{ToIsoUtc(playedAt)}#SCORE#{Require(scoreId, nameof(scoreId))}";

    public static string BuildLeaderboardGsi4Pk(string gameKey, string experimentId)
        => $"GAME#{NormaliseKey(gameKey)}#EXPERIMENT#{NormaliseKey(experimentId)}";

    public static string BuildSubmissionLockPk(string clientSubmissionId)
        => $"CLIENTSUB#{Require(clientSubmissionId, nameof(clientSubmissionId))}";

    public static string BuildSubmissionLockSk()
        => "SCORE";

    public static string BuildPlayedAtLowerBound(DateTime fromUtc)
        => $"PLAYEDAT#{ToIsoUtc(fromUtc)}";

    public static string BuildPlayedAtUpperBound(DateTime toUtc)
        => $"PLAYEDAT#{ToIsoUtc(toUtc)}#~";

    public static string ToIsoUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    public static string NormaliseKey(string value)
        => Require(value, nameof(value)).Trim().ToUpperInvariant();

    public static long InvertScore(int scoreValue)
    {
        if (scoreValue < 0)
            throw new ArgumentOutOfRangeException(nameof(scoreValue), "Score cannot be negative.");

        return MaxInvertedScore - scoreValue;
    }

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", name);

        return value.Trim();
    }

    internal static string BuildExperimentGsi4Pk(string experimentId)
    {
        throw new NotImplementedException();
    }
}