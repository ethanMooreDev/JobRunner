namespace JobRunner.Domain.Models;

public sealed record AttemptStart(Guid AttemptId, int AttemptNumber, int MaxAttempts);