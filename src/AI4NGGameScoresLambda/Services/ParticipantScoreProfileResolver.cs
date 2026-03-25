using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Models.Dtos;

namespace AI4NGGameScoresLambda.Services;

public sealed class ParticipantScoreProfileResolver : IParticipantScoreProfileResolver
{
    private readonly IAmazonDynamoDB _dynamo;
    private readonly string _experimentsTable;

    public ParticipantScoreProfileResolver(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo ?? throw new ArgumentNullException(nameof(dynamo));
        _experimentsTable = Environment.GetEnvironmentVariable("EXPERIMENTS_TABLE")
            ?? throw new InvalidOperationException("EXPERIMENTS_TABLE environment variable is not configured.");
    }

    public async Task<ParticipantScoreProfileDto> ResolveAsync(
        string userId,
        string experimentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(experimentId))
            throw new ArgumentException("Experiment ID is required.", nameof(experimentId));

        var response = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = _experimentsTable,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue { S = $"EXPERIMENT#{experimentId.Trim()}" },
                ["SK"] = new AttributeValue { S = $"MEMBER#{userId.Trim()}" }
            },
            ConsistentRead = true
        }, cancellationToken);

        if (response.Item == null || response.Item.Count == 0)
            throw new KeyNotFoundException($"Membership not found for user '{userId}' in experiment '{experimentId}'.");

        var cohort = GetString(response.Item, "cohort");
        var pseudoId = GetString(response.Item, "pseudoId");

        if (string.IsNullOrWhiteSpace(cohort))
            throw new InvalidOperationException("Membership item does not contain 'cohort'.");

        if (string.IsNullOrWhiteSpace(pseudoId))
        {
            var suffix = userId.Length >= 6 ? userId[^6..].ToUpperInvariant() : userId.ToUpperInvariant();
            pseudoId = $"P-{suffix}";
        }

        return new ParticipantScoreProfileDto
        {
            UserId = userId.Trim(),
            Cohort = cohort.Trim().ToUpperInvariant(),
            DisplayUsername = pseudoId.Trim()
        };
    }

    private static string? GetString(Dictionary<string, AttributeValue> item, string key)
    {
        return item.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value.S)
            ? value.S
            : null;
    }
}