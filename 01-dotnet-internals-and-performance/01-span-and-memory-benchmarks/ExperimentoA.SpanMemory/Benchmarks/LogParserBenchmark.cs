using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ExperimentoA.SpanMemory.Models;
using ExperimentoA.SpanMemory.Parsers;

namespace ExperimentoA.SpanMemory.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class LogParserBenchmark
{
    private const string LogLine = "2026-08-12T16:25:00|WARN|ID_998811|Erro de conexão com o banco de dados principal";
    private ReadOnlyMemory<char> _logLineMemory;

    [GlobalSetup]
    public void Setup()
    {
        _logLineMemory = LogLine.AsMemory();
    }

    // --- BENCHMARKS DE EXTRAÇÃO APENAS DO ID DO EVENTO ---

    [Benchmark(Baseline = true, Description = "Extract ID - Traditional (.Substring)")]
    public string ExtractEventId_Traditional()
    {
        return TraditionalLogParser.ExtractEventId(LogLine);
    }

    [Benchmark(Description = "Extract ID - Span<T> (Zero Alloc)")]
    public int ExtractEventId_Span()
    {
        ReadOnlySpan<char> span = SpanLogParser.ExtractEventId(LogLine.AsSpan());
        return span.Length;
    }

    [Benchmark(Description = "Extract ID - Memory<T>")]
    public int ExtractEventId_Memory()
    {
        ReadOnlyMemory<char> mem = MemoryLogParser.ExtractEventId(_logLineMemory);
        return mem.Length;
    }

    // --- BENCHMARKS DE PARSING COMPLETO DOS CAMPOS ---

    [Benchmark(Description = "Parse Full - Traditional (.Split)")]
    public LogEntryTraditional ParseFull_Traditional()
    {
        return TraditionalLogParser.Parse(LogLine);
    }

    [Benchmark(Description = "Parse Full - Span<T> (Zero Alloc)")]
    public int ParseFull_Span()
    {
        LogEntrySpan entry = SpanLogParser.Parse(LogLine.AsSpan());
        return entry.Timestamp.Length + entry.Level.Length + entry.EventId.Length + entry.Message.Length;
    }

    [Benchmark(Description = "Parse Full - Memory<T>")]
    public LogEntryMemory ParseFull_Memory()
    {
        return MemoryLogParser.Parse(_logLineMemory);
    }
}
