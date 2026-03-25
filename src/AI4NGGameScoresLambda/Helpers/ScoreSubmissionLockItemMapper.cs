using Amazon.DynamoDBv2.Model;
using AI4NGGameScoresLambda.Models.Entities;

namespace AI4NGGameScoresLambda.Helpers;

public static class ScoreSubmissionLockItemMapper
{
    public static Dictionary<string, AttributeValue> ToAttributeMap(ScoreSubmissionLockItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = item.PK },
            ["SK"] = new AttributeValue { S = item.SK },
            ["Type"] = new AttributeValue { S = item.Type },
            ["clientSubmissionId"] = new AttributeValue { S = item.ClientSubmissionId },
            ["scoreId"] = new AttributeValue { S = item.ScoreId },
            ["userId"] = new AttributeValue { S = item.UserId },
            ["recordedAt"] = new AttributeValue { S = DynamoKeyBuilder.ToIsoUtc(item.RecordedAtUtc) }
        };
    }

    public static ScoreSubmissionLockItem FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ScoreSubmissionLockItem
        {
            PK = GetString(item, "PK") ?? string.Empty,
            SK = GetString(item, "SK") ?? "SCORE",
            Type = GetString(item, "Type") ?? "ScoreSubmissionLock",
            ClientSubmissionId = GetString(item, "clientSubmissionId") ?? string.Empty,
            ScoreId = GetString(item, "scoreId") ?? string.Empty,
            UserId = GetString(item, "userId") ?? string.Empty,
            RecordedAtUtc = ParseUtc(GetString(item, "recordedAt"))
        };
    }

    private static string? GetString(Dictionary<string, AttributeValue> item, string key)
        => item.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value.S) ? value.S : null;

    private static DateTime ParseUtc(string? value)
        => DateTime.TryParse(value, out var parsed)
            ? DateTime.SpecifyKind(parsed.ToUniversalTime(), DateTimeKind.Utc)
            : DateTime.UnixEpoch;
}