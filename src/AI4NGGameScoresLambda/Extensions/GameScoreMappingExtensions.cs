using AI4NGGameScoresLambda.Models.Dtos;
using AI4NGGameScoresLambda.Models.Entities;

namespace AI4NGGameScoresLambda.Extensions;

public static class GameScoreMappingExtensions
{
    public static GameScoreDto ToDto(this GameScoreItem item)
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
}