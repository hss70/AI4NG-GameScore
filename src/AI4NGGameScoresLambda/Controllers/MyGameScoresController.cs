using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Models.Queries;
using AI4NGGameScoresLambda.Models.Requests;
using AI4NGGameScoresLambda.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AI4NGGameScoresLambda.Controllers;

[ApiController]
[Route("api/me/game-scores")]
public sealed class MyGameScoresController : ControllerBase
{
    private readonly IGameScoresService _gameScoresService;
    private readonly IUserContextService _userContextService;

    public MyGameScoresController(
        IGameScoresService gameScoresService,
        IUserContextService userContextService)
    {
        _gameScoresService = gameScoresService;
        _userContextService = userContextService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateGameScoreResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameScoreRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _userContextService.GetUserId();
        var result = await _gameScoresService.CreateScoreAsync(userId, request, cancellationToken);

        var response = new CreateGameScoreResponse
        {
            ScoreId = result.ScoreId,
            Created = result.Created,
            IdempotentReplay = result.IdempotentReplay,
            RecordedAtUtc = result.RecordedAtUtc
        };

        return result.Created
            ? StatusCode(StatusCodes.Status201Created, response)
            : Ok(response);
    }

    [HttpGet("{gameKey}")]
    [ProducesResponseType(typeof(GetGameScoresResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyScores(
        [FromRoute] string gameKey,
        [FromQuery] string? classifierId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 20,
        [FromQuery] string? nextToken = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();

        var query = new GameScoreHistoryQuery
        {
            GameKey = gameKey,
            ClassifierId = classifierId,
            FromUtc = from,
            ToUtc = to,
            Limit = limit,
            NextToken = nextToken
        };

        var result = await _gameScoresService.GetMyScoresAsync(userId, query, cancellationToken);

        var response = new GetGameScoresResponse
        {
            GameKey = gameKey,
            ClassifierId = classifierId,
            FromUtc = from,
            ToUtc = to,
            Items = result.Items,
            NextToken = result.NextToken
        };

        return Ok(response);
    }

    [HttpGet("{gameKey}/best")]
    [ProducesResponseType(typeof(GetBestGameScoresResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyBestScores(
        [FromRoute] string gameKey,
        [FromQuery] string? classifierId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();

        var query = new GameScoreBestQuery
        {
            GameKey = gameKey,
            ClassifierId = classifierId,
            FromUtc = from,
            ToUtc = to,
            Limit = limit
        };

        var items = await _gameScoresService.GetMyBestScoresAsync(userId, query, cancellationToken);

        var response = new GetBestGameScoresResponse
        {
            GameKey = gameKey,
            ClassifierId = classifierId,
            FromUtc = from,
            ToUtc = to,
            Items = items
        };

        return Ok(response);
    }
}