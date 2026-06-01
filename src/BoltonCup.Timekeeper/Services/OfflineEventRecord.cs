namespace BoltonCup.Timekeeper.Services;

public sealed record OfflineEventRecord
{
    public required Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string PayloadJson { get; init; }
    public required DateTime CreatedAt { get; init; }
}
