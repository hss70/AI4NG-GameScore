using AI4NGGameScoresLambda.Models.Dtos;

namespace AI4NGGameScoresLambda.Interfaces;

public interface IParticipantScoreProfileResolver
{
    Task<ParticipantScoreProfileDto> ResolveAsync(
        string userId,
        string experimentId,
        CancellationToken cancellationToken = default);
}