namespace LarpakeServer.Data;

public record LarpakeAvgPoints(long LarpakeId, int AveragePoints);
public record LarpakeTotalPoints(long LarpakeId, int TotalPoints);
public record GroupTotalPoints(long LarpakeId, long GroupId, int TotalPoints);
