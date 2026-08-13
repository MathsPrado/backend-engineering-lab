using System;

namespace ExperimentoA.SpanMemory.Models;

public readonly ref struct LogEntrySpan
{
    public ReadOnlySpan<char> Timestamp { get; }
    public ReadOnlySpan<char> Level { get; }
    public ReadOnlySpan<char> EventId { get; }
    public ReadOnlySpan<char> Message { get; }

    public LogEntrySpan(
        ReadOnlySpan<char> timestamp,
        ReadOnlySpan<char> level,
        ReadOnlySpan<char> eventId,
        ReadOnlySpan<char> message)
    {
        Timestamp = timestamp;
        Level = level;
        EventId = eventId;
        Message = message;
    }
}
