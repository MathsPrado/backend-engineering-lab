using System;
using ExperimentoA.SpanMemory.Models;

namespace ExperimentoA.SpanMemory.Parsers;

public static class TraditionalLogParser
{
    public static LogEntryTraditional Parse(string logLine)
    {
        // 1. Split aloca um array de strings (string[]) + 4 novas instâncias de string na Heap
        string[] parts = logLine.Split('|');
        if (parts.Length < 4)
        {
            throw new ArgumentException("Linha de log inválida.", nameof(logLine));
        }

        return new LogEntryTraditional(
            parts[0],
            parts[1],
            parts[2],
            parts[3]
        );
    }

    public static string ExtractEventId(string logLine)
    {
        // Método tradicional de extração usando Substring e IndexOf
        // Exemplo: "2026-08-12T16:25:00|WARN|ID_998811|Erro de conexão"
        int firstPipe = logLine.IndexOf('|');
        if (firstPipe < 0) return string.Empty;

        int secondPipe = logLine.IndexOf('|', firstPipe + 1);
        if (secondPipe < 0) return string.Empty;

        int thirdPipe = logLine.IndexOf('|', secondPipe + 1);
        if (thirdPipe < 0)
        {
            return logLine.Substring(secondPipe + 1);
        }

        // Substring cria uma nova instância de string na Heap
        return logLine.Substring(secondPipe + 1, thirdPipe - secondPipe - 1);
    }
}
