namespace ExperimentoA.SpanMemory.Models;

public record LogEntryTraditional(
    string Timestamp,
    string Level,
    string EventId,
    string Message
);
