using System;

namespace ExperimentoA.SpanMemory.Models;

public readonly struct LogEntryMemory
{
    public ReadOnlyMemory<char> Timestamp { get; }
    public ReadOnlyMemory<char> Level { get; }
    public ReadOnlyMemory<char> EventId { get; }
    public ReadOnlyMemory<char> Message { get; }

    public LogEntryMemory(
        ReadOnlyMemory<char> timestamp,
        ReadOnlyMemory<char> level,
        ReadOnlyMemory<char> eventId,
        ReadOnlyMemory<char> message)
    {
        Timestamp = timestamp;
        Level = level;
        EventId = eventId;
        Message = message;
    }
}
