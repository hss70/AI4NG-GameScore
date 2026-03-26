using Amazon.DynamoDBv2.Model;
using AI4NGGameScoresLambda.Models.Entities;
using System.Text.Json;

namespace AI4NGGameScoresLambda.Helpers;

public static class GameScoreItemMapper
{
    public static Dictionary<string, AttributeValue> ToAttributeMap(GameScoreItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var attributes = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = item.PK },
            ["SK"] = new AttributeValue { S = item.SK },

            ["GSI1PK"] = new AttributeValue { S = item.GSI1PK },
            ["GSI1SK"] = new AttributeValue { S = item.GSI1SK },

            ["GSI2PK"] = new AttributeValue { S = item.GSI2PK },
            ["GSI2SK"] = new AttributeValue { S = item.GSI2SK },

            ["GSI3PK"] = new AttributeValue { S = item.GSI3PK },
            ["GSI3SK"] = new AttributeValue { S = item.GSI3SK },

            ["GSI4PK"] = new AttributeValue { S = item.GSI4PK },
            ["GSI4SK"] = new AttributeValue { S = item.GSI4SK },

            ["Type"] = new AttributeValue { S = item.Type },
            ["scoreId"] = new AttributeValue { S = item.ScoreId },
            ["experimentId"] = new AttributeValue { S = item.ExperimentId },
            ["cohort"] = new AttributeValue { S = item.Cohort },
            ["gameKey"] = new AttributeValue { S = item.GameKey },
            ["userId"] = new AttributeValue { S = item.UserId },
            ["username"] = new AttributeValue { S = item.Username },
            ["classifierId"] = new AttributeValue { S = item.ClassifierId },
            ["scoreValue"] = new AttributeValue { N = item.ScoreValue.ToString() },
            ["scoreUnit"] = new AttributeValue { S = item.ScoreUnit },
            ["playedAt"] = new AttributeValue { S = DynamoKeyBuilder.ToIsoUtc(item.PlayedAtUtc) },
            ["recordedAt"] = new AttributeValue { S = DynamoKeyBuilder.ToIsoUtc(item.RecordedAtUtc) },
            ["clientSubmissionId"] = new AttributeValue { S = item.ClientSubmissionId }
        };

        if (!string.IsNullOrWhiteSpace(item.SessionOccurrenceKey))
            attributes["sessionOccurrenceKey"] = new AttributeValue { S = item.SessionOccurrenceKey };

        if (!string.IsNullOrWhiteSpace(item.TaskKey))
            attributes["taskKey"] = new AttributeValue { S = item.TaskKey };

        if (item.Metadata is { Count: > 0 })
            attributes["metadata"] = new AttributeValue { S = JsonSerializer.Serialize(item.Metadata) };

        return attributes;
    }

    public static GameScoreItem FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new GameScoreItem
        {
            PK = GetString(item, "PK") ?? string.Empty,
            SK = GetString(item, "SK") ?? string.Empty,
            GSI1PK = GetString(item, "GSI1PK") ?? string.Empty,
            GSI1SK = GetString(item, "GSI1SK") ?? string.Empty,
            GSI2PK = GetString(item, "GSI2PK") ?? string.Empty,
            GSI2SK = GetString(item, "GSI2SK") ?? string.Empty,
            GSI3PK = GetString(item, "GSI3PK") ?? string.Empty,
            GSI3SK = GetString(item, "GSI3SK") ?? string.Empty,
            Type = GetString(item, "Type") ?? "GameScore",
            ScoreId = GetString(item, "scoreId") ?? string.Empty,
            ExperimentId = GetString(item, "experimentId") ?? string.Empty,
            Cohort = GetString(item, "cohort") ?? string.Empty,
            GameKey = GetString(item, "gameKey") ?? string.Empty,
            UserId = GetString(item, "userId") ?? string.Empty,
            Username = GetString(item, "username") ?? string.Empty,
            ClassifierId = GetString(item, "classifierId") ?? string.Empty,
            ScoreValue = GetInt(item, "scoreValue"),
            ScoreUnit = GetString(item, "scoreUnit") ?? "points",
            PlayedAtUtc = ParseUtc(GetString(item, "playedAt")),
            RecordedAtUtc = ParseUtc(GetString(item, "recordedAt")),
            SessionOccurrenceKey = GetString(item, "sessionOccurrenceKey"),
            TaskKey = GetString(item, "taskKey"),
            ClientSubmissionId = GetString(item, "clientSubmissionId") ?? string.Empty,
            Metadata = ParseMetadata(GetString(item, "metadata"))
        };
    }

    private static string? GetString(Dictionary<string, AttributeValue> item, string key)
        => item.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value.S) ? value.S : null;

    private static int GetInt(Dictionary<string, AttributeValue> item, string key)
        => item.TryGetValue(key, out var value) && int.TryParse(value.N, out var parsed) ? parsed : 0;

    private static DateTime ParseUtc(string? value)
        => DateTime.TryParse(value, out var parsed)
            ? DateTime.SpecifyKind(parsed.ToUniversalTime(), DateTimeKind.Utc)
            : DateTime.UnixEpoch;

    private static Dictionary<string, object>? ParseMetadata(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, object>>(json);
}
