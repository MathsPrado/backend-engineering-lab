using System;
using BenchmarkDotNet.Running;
using ExperimentoA.SpanMemory.Benchmarks;
using ExperimentoA.SpanMemory.Models;
using ExperimentoA.SpanMemory.Parsers;

namespace ExperimentoA.SpanMemory;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("--benchmark", StringComparison.OrdinalIgnoreCase))
        {
            RunBenchmark();
            return;
        }

        Console.WriteLine("==========================================================================");
        Console.WriteLine("  EXPERIMENTO A: Manipulação de Texto e Alocação de Memória");
        Console.WriteLine("  Span<T> vs Memory<T> vs Método Tradicional (.Split/.Substring)");
        Console.WriteLine("==========================================================================");
        Console.WriteLine();

        const string sampleLog = "2026-08-12T16:25:00|WARN|ID_998811|Erro de conexão com banco de dados";

        Console.WriteLine($"Linha de Log Entrada: \"{sampleLog}\"");
        Console.WriteLine(new string('-', 74));

        // 1. Método Tradicional
        Console.WriteLine("\n[1] Método Tradicional (string.Split / Substring):");
        LogEntryTraditional tradEntry = TraditionalLogParser.Parse(sampleLog);
        string extractedIdTrad = TraditionalLogParser.ExtractEventId(sampleLog);
        Console.WriteLine($"  - Data/Hora : {tradEntry.Timestamp}");
        Console.WriteLine($"  - Nível     : {tradEntry.Level}");
        Console.WriteLine($"  - Event ID  : {tradEntry.EventId} (Extraído solo: {extractedIdTrad})");
        Console.WriteLine($"  - Mensagem  : {tradEntry.Message}");
        Console.WriteLine($"  - Alocações : Aloca array string[] + 4 novas instâncias de string na Heap!");

        // 2. Método Span<T>
        Console.WriteLine("\n[2] Método Span<T> (ReadOnlySpan<char> - Stack Only):");
        LogEntrySpan spanEntry = SpanLogParser.Parse(sampleLog.AsSpan());
        ReadOnlySpan<char> extractedIdSpan = SpanLogParser.ExtractEventId(sampleLog.AsSpan());
        Console.WriteLine($"  - Data/Hora : {spanEntry.Timestamp}");
        Console.WriteLine($"  - Nível     : {spanEntry.Level}");
        Console.WriteLine($"  - Event ID  : {spanEntry.EventId} (Extraído solo: {extractedIdSpan})");
        Console.WriteLine($"  - Mensagem  : {spanEntry.Message}");
        Console.WriteLine($"  - Alocações : 0 BYTES ALOCADOS NA HEAP (Zero-Alloc)!");

        // 3. Método Memory<T>
        Console.WriteLine("\n[3] Método Memory<T> (ReadOnlyMemory<char> - Heap Safe / Async Compatible):");
        LogEntryMemory memEntry = MemoryLogParser.Parse(sampleLog.AsMemory());
        ReadOnlyMemory<char> extractedIdMem = MemoryLogParser.ExtractEventId(sampleLog.AsMemory());
        Console.WriteLine($"  - Data/Hora : {memEntry.Timestamp.Span}");
        Console.WriteLine($"  - Nível     : {memEntry.Level.Span}");
        Console.WriteLine($"  - Event ID  : {memEntry.EventId.Span} (Extraído solo: {extractedIdMem.Span})");
        Console.WriteLine($"  - Mensagem  : {memEntry.Message.Span}");
        Console.WriteLine($"  - Alocações : Aloca apenas a struct wrapper (se mantida em Heap), zero cópias de caracteres!");

        Console.WriteLine("\n" + new string('=', 74));
        Console.WriteLine("Execução de verificação funcional concluída com sucesso!");
        Console.WriteLine("Dica: Para executar os Benchmarks diretamente, passe a flag '--benchmark' ou rode:");
        Console.WriteLine("      dotnet run -c Release -- --benchmark");
        Console.WriteLine(new string('=', 74));
    }

    private static void RunBenchmark()
    {
        Console.WriteLine("\nIniciando suíte de BenchmarkDotNet (Modo Release)...");
        BenchmarkRunner.Run<LogParserBenchmark>();
    }
}
