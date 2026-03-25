using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Models.Queries;
using AI4NGGameScoresLambda.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AI4NGGameScoresLambda.Controllers;

[ApiController]
[Route("api/leaderboards")]
public sealed class LeaderboardsController : ControllerBase
{
    private readonly IGameScoresService _gameScoresService;
    private readonly IUserContextService _userContextService;


    public LeaderboardsController(
        IGameScoresService gameScoresService,
        IUserContextService userContextService)
    {
        _gameScoresService = gameScoresService;
        _userContextService = userContextService;
    }

    [HttpGet("{gameKey}")]
    [ProducesResponseType(typeof(GetLeaderboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard(
        [FromRoute] string gameKey,
        [FromQuery] string experimentId,
        [FromQuery] int limit = 25,
        [FromQuery] string? nextToken = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _userContextService.GetUserId();

        var query = new LeaderboardQuery
        {
            GameKey = gameKey,
            ExperimentId = experimentId,
            UserId = userId,
            Limit = limit,
            NextToken = nextToken
        };

        var result = await _gameScoresService.GetLeaderboardAsync(query, cancellationToken);

        var response = new GetLeaderboardResponse
        {
            GameKey = gameKey,
            ExperimentId = experimentId,
            Items = result.Items,
            NextToken = result.NextToken
        };

        return Ok(response);
    }

    [HttpGet("{gameKey}/global")]
    [ProducesResponseType(typeof(GetGlobalLeaderboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobalLeaderboard(
        [FromRoute] string gameKey,
        [FromQuery] string experimentId,
        [FromQuery] int limit = 25,
        [FromQuery] string? nextToken = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = new GlobalLeaderboardQuery
        {
            GameKey = gameKey,
            ExperimentId = experimentId,
            Limit = limit,
            NextToken = nextToken
        };

        var result = await _gameScoresService.GetGlobalLeaderboardAsync(query, cancellationToken);

        var response = new GetGlobalLeaderboardResponse
        {
            GameKey = gameKey,
            ExperimentId = experimentId,
            Items = result.Items,
            NextToken = result.NextToken
        };

        return Ok(response);
    }
}