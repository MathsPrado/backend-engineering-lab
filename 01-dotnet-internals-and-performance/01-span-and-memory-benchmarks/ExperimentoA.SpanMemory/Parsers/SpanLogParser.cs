using System;
using ExperimentoA.SpanMemory.Models;

namespace ExperimentoA.SpanMemory.Parsers;

public static class SpanLogParser
{
    public static LogEntrySpan Parse(ReadOnlySpan<char> logLine)
    {
        // Zero Allocation parsing usando ReadOnlySpan<char> e Slice / IndexOf
        int firstPipe = logLine.IndexOf('|');
        if (firstPipe < 0) throw new ArgumentException("Linha de log inválida.");

        ReadOnlySpan<char> timestamp = logLine.Slice(0, firstPipe);
        ReadOnlySpan<char> remainder1 = logLine.Slice(firstPipe + 1);

        int secondPipe = remainder1.IndexOf('|');
        if (secondPipe < 0) throw new ArgumentException("Linha de log inválida.");

        ReadOnlySpan<char> level = remainder1.Slice(0, secondPipe);
        ReadOnlySpan<char> remainder2 = remainder1.Slice(secondPipe + 1);

        int thirdPipe = remainder2.IndexOf('|');
        if (thirdPipe < 0)
        {
            return new LogEntrySpan(timestamp, level, remainder2, ReadOnlySpan<char>.Empty);
        }

        ReadOnlySpan<char> eventId = remainder2.Slice(0, thirdPipe);
        ReadOnlySpan<char> message = remainder2.Slice(thirdPipe + 1);

        return new LogEntrySpan(timestamp, level, eventId, message);
    }

    public static ReadOnlySpan<char> ExtractEventId(ReadOnlySpan<char> logLine)
    {
        // Extração de sub-região sem nenhuma alocação de memória na Heap
        int firstPipe = logLine.IndexOf('|');
        if (firstPipe < 0) return ReadOnlySpan<char>.Empty;

        ReadOnlySpan<char> remainder1 = logLine.Slice(firstPipe + 1);
        int secondPipe = remainder1.IndexOf('|');
        if (secondPipe < 0) return ReadOnlySpan<char>.Empty;

        ReadOnlySpan<char> remainder2 = remainder1.Slice(secondPipe + 1);
        int thirdPipe = remainder2.IndexOf('|');
        if (thirdPipe < 0) return remainder2;

        return remainder2.Slice(0, thirdPipe);
    }
}
