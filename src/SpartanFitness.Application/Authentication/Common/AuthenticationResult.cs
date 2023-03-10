using SpartanFitness.Domain.Aggregates;

namespace SpartanFitness.Application.Authentication.Common;

public record AuthenticationResult(
    User User,
    string Token);