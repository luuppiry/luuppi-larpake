namespace LarpakeServer.Models.DatabaseModels;

public readonly record struct UserPoints(Guid UserId, int Points);
public readonly record struct GroupPoints(long GroupId, int Points);
