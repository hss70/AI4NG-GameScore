using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AI4NGGameScoresLambda.Helpers;
using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Models.Dtos;
using AI4NGGameScoresLambda.Models.Entities;

namespace AI4NGGameScoresLambda.Services;

public sealed class GameScoresRepository : IGameScoresRepository
{
    private readonly IAmazonDynamoDB _dynamo;
    private readonly string _gameScoresTable;

    public GameScoresRepository(IAmazonDynamoDB dynamo)
    {
        _dynamo = dynamo ?? throw new ArgumentNullException(nameof(dynamo));
        _gameScoresTable = Environment.GetEnvironmentVariable("GAME_SCORES_TABLE")
            ?? throw new InvalidOperationException("GAME_SCORES_TABLE environment variable is not configured.");
    }

    public async Task CreateScoreAsync(
        GameScoreItem scoreItem,
        ScoreSubmissionLockItem submissionLockItem,
        CancellationToken cancellationToken = default)
    {
        await _dynamo.TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new()
                {
                    Put = new Put
                    {
                        TableName = _gameScoresTable,
                        Item = ScoreSubmissionLockItemMapper.ToAttributeMap(submissionLockItem),
                        ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
                    }
                },
                new()
                {
                    Put = new Put
                    {
                        TableName = _gameScoresTable,
                        Item = GameScoreItemMapper.ToAttributeMap(scoreItem),
                        ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
                    }
                }
            }
        }, cancellationToken);
    }

    public async Task<ScoreSubmissionLockItem?> GetSubmissionLockAsync(
        string clientSubmissionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _dynamo.GetItemAsync(new GetItemRequest
        {
            TableName = _gameScoresTable,
            Key = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue { S = DynamoKeyBuilder.BuildSubmissionLockPk(clientSubmissionId) },
                ["SK"] = new AttributeValue { S = DynamoKeyBuilder.BuildSubmissionLockSk() }
            },
            ConsistentRead = true
        }, cancellationToken);

        if (response.Item == null || response.Item.Count == 0)
            return null;

        return ScoreSubmissionLockItemMapper.FromAttributeMap(response.Item);
    }

    public async Task<PagedResultDto<GameScoreItem>> GetScoresByUserAndGameAsync(
        string userId,
        string gameKey,
        DateTime? fromUtc,
        DateTime? toUtc,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _gameScoresTable,
            IndexName = "GSI2",
            KeyConditionExpression = "GSI2PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = DynamoKeyBuilder.BuildUserGameGsi2Pk(userId, gameKey) }
            },
            Limit = limit,
            ExclusiveStartKey = DecodePageToken(nextToken),
            ScanIndexForward = false
        };

        ApplyDateRangeToSortKey(request, "GSI2SK", fromUtc, toUtc);

        var response = await _dynamo.QueryAsync(request, cancellationToken);

        return new PagedResultDto<GameScoreItem>
        {
            Items = response.Items.Select(GameScoreItemMapper.FromAttributeMap).ToArray(),
            NextToken = EncodePageToken(response.LastEvaluatedKey)
        };
    }

    public async Task<PagedResultDto<GameScoreItem>> GetScoresByUserGameAndClassifierAsync(
        string userId,
        string gameKey,
        string classifierId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _gameScoresTable,
            IndexName = "GSI3",
            KeyConditionExpression = "GSI3PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue
                {
                    S = DynamoKeyBuilder.BuildUserGameClassifierGsi3Pk(userId, gameKey, classifierId)
                }
            },
            Limit = limit,
            ExclusiveStartKey = DecodePageToken(nextToken),
            ScanIndexForward = false
        };

        ApplyDateRangeToSortKey(request, "GSI3SK", fromUtc, toUtc);

        var response = await _dynamo.QueryAsync(request, cancellationToken);

        return new PagedResultDto<GameScoreItem>
        {
            Items = response.Items.Select(GameScoreItemMapper.FromAttributeMap).ToArray(),
            NextToken = EncodePageToken(response.LastEvaluatedKey)
        };
    }

    public async Task<PagedResultDto<GameScoreItem>> GetLeaderboardAsync(
        string gameKey,
        string cohort,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _gameScoresTable,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue
                {
                    S = DynamoKeyBuilder.BuildLeaderboardGsi1Pk(gameKey, cohort)
                }
            },
            Limit = limit,
            ExclusiveStartKey = DecodePageToken(nextToken),
            ScanIndexForward = true
        };

        var response = await _dynamo.QueryAsync(request, cancellationToken);

        return new PagedResultDto<GameScoreItem>
        {
            Items = response.Items.Select(GameScoreItemMapper.FromAttributeMap).ToArray(),
            NextToken = EncodePageToken(response.LastEvaluatedKey)
        };
    }

    public async Task<PagedResultDto<GameScoreItem>> GetGlobalLeaderboardAsync(
        string gameKey,
        string experimentId,
        int limit,
        string? nextToken,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _gameScoresTable,
            IndexName = "GSI4",
            KeyConditionExpression = "GSI4PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue
                {
                    S = DynamoKeyBuilder.BuildLeaderboardGsi4Pk(gameKey, experimentId)
                }
            },
            Limit = limit,
            ExclusiveStartKey = DecodePageToken(nextToken),
            ScanIndexForward = true
        };

        var response = await _dynamo.QueryAsync(request, cancellationToken);

        return new PagedResultDto<GameScoreItem>
        {
            Items = response.Items.Select(GameScoreItemMapper.FromAttributeMap).ToArray(),
            NextToken = EncodePageToken(response.LastEvaluatedKey)
        };
    }

    private static void ApplyDateRangeToSortKey(
        QueryRequest request,
        string sortKeyName,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc is null && toUtc is null)
            return;

        if (fromUtc is not null && toUtc is not null)
        {
            request.KeyConditionExpression += $" AND {sortKeyName} BETWEEN :fromSk AND :toSk";
            request.ExpressionAttributeValues[":fromSk"] = new AttributeValue
            {
                S = DynamoKeyBuilder.BuildPlayedAtLowerBound(fromUtc.Value)
            };
            request.ExpressionAttributeValues[":toSk"] = new AttributeValue
            {
                S = DynamoKeyBuilder.BuildPlayedAtUpperBound(toUtc.Value)
            };
            return;
        }

        if (fromUtc is not null)
        {
            request.KeyConditionExpression += $" AND {sortKeyName} >= :fromSk";
            request.ExpressionAttributeValues[":fromSk"] = new AttributeValue
            {
                S = DynamoKeyBuilder.BuildPlayedAtLowerBound(fromUtc.Value)
            };
            return;
        }

        request.KeyConditionExpression += $" AND {sortKeyName} <= :toSk";
        request.ExpressionAttributeValues[":toSk"] = new AttributeValue
        {
            S = DynamoKeyBuilder.BuildPlayedAtUpperBound(toUtc!.Value)
        };
    }

    private static string? EncodePageToken(Dictionary<string, AttributeValue>? lastEvaluatedKey)
    {
        if (lastEvaluatedKey == null || lastEvaluatedKey.Count == 0)
            return null;

        var compact = lastEvaluatedKey.ToDictionary(
            kvp => kvp.Key,
            kvp => new SerializableAttributeValue
            {
                S = kvp.Value.S,
                N = kvp.Value.N,
                BOOL = kvp.Value.BOOL
            });

        var json = JsonSerializer.Serialize(compact);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private static Dictionary<string, AttributeValue>? DecodePageToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var compact = JsonSerializer.Deserialize<Dictionary<string, SerializableAttributeValue>>(json);

        if (compact == null || compact.Count == 0)
            return null;

        return compact.ToDictionary(
            kvp => kvp.Key,
            kvp => new AttributeValue
            {
                S = kvp.Value.S,
                N = kvp.Value.N,
                BOOL = kvp.Value.BOOL
            });
    }

    private sealed class SerializableAttributeValue
    {
        public string? S { get; set; }
        public string? N { get; set; }
        public bool? BOOL { get; set; }
    }
}