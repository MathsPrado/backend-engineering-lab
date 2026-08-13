using System;
using ExperimentoA.SpanMemory.Models;

namespace ExperimentoA.SpanMemory.Parsers;

public static class MemoryLogParser
{
    public static LogEntryMemory Parse(ReadOnlyMemory<char> logLine)
    {
        ReadOnlySpan<char> span = logLine.Span;

        int firstPipe = span.IndexOf('|');
        if (firstPipe < 0) throw new ArgumentException("Linha de log inválida.");

        ReadOnlyMemory<char> timestamp = logLine.Slice(0, firstPipe);
        ReadOnlyMemory<char> remainder1 = logLine.Slice(firstPipe + 1);

        int secondPipe = remainder1.Span.IndexOf('|');
        if (secondPipe < 0) throw new ArgumentException("Linha de log inválida.");

        ReadOnlyMemory<char> level = remainder1.Slice(0, secondPipe);
        ReadOnlyMemory<char> remainder2 = remainder1.Slice(secondPipe + 1);

        int thirdPipe = remainder2.Span.IndexOf('|');
        if (thirdPipe < 0)
        {
            return new LogEntryMemory(timestamp, level, remainder2, ReadOnlyMemory<char>.Empty);
        }

        ReadOnlyMemory<char> eventId = remainder2.Slice(0, thirdPipe);
        ReadOnlyMemory<char> message = remainder2.Slice(thirdPipe + 1);

        return new LogEntryMemory(timestamp, level, eventId, message);
    }

    public static ReadOnlyMemory<char> ExtractEventId(ReadOnlyMemory<char> logLine)
    {
        ReadOnlySpan<char> span = logLine.Span;
        int firstPipe = span.IndexOf('|');
        if (firstPipe < 0) return ReadOnlyMemory<char>.Empty;

        ReadOnlyMemory<char> remainder1 = logLine.Slice(firstPipe + 1);
        int secondPipe = remainder1.Span.IndexOf('|');
        if (secondPipe < 0) return ReadOnlyMemory<char>.Empty;

        ReadOnlyMemory<char> remainder2 = remainder1.Slice(secondPipe + 1);
        int thirdPipe = remainder2.Span.IndexOf('|');
        if (thirdPipe < 0) return remainder2;

        return remainder2.Slice(0, thirdPipe);
    }
}
