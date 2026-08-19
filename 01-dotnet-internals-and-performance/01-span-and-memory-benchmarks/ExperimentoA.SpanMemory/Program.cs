using System;
using BenchmarkDotNet.Running;
using ExperimentoA.SpanMemory.Benchmarks;
using ExperimentoA.SpanMemory.Models;
using ExperimentoA.SpanMemory.Parsers;
using Spectre.Console;

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

        AnsiConsole.Write(
            new FigletText("Experimento")
                .LeftJustified()
                .Color(Color.DeepSkyBlue1));

        AnsiConsole.MarkupLine("[bold deepskyblue1]Manipulação de Texto e Alocação de Memória[/]");
        AnsiConsole.MarkupLine("Comparativo entre [yellow]Span<T>[/], [yellow]Memory<T>[/] e o [yellow]Método Tradicional[/]\n");

        const string sampleLog = "2026-08-12T16:25:00|WARN|ID_998811|Erro de conexão com banco de dados";

        var logPanel = new Panel($"[bold white]{sampleLog}[/]")
        {
            Header = new PanelHeader("Linha de Log Entrada", Justify.Left),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1)
        };
        AnsiConsole.Write(logPanel);

        // 👇 Aqui a mágica acontece: rodamos na prática os três métodos (Tradicional, Span e Memory)
        // usando a linha de teste aí de cima.

        // 1. Método Tradicional (string.Split / Substring)
        LogEntryTraditional tradEntry = TraditionalLogParser.Parse(sampleLog);
        string extractedIdTrad = TraditionalLogParser.ExtractEventId(sampleLog);

        // 2. Método Span<T> (ReadOnlySpan<char> - Stack Only)
        LogEntrySpan spanEntry = SpanLogParser.Parse(sampleLog.AsSpan());
        ReadOnlySpan<char> extractedIdSpan = SpanLogParser.ExtractEventId(sampleLog.AsSpan());

        // 3. Método Memory<T> (ReadOnlyMemory<char> - Heap Safe / Async Compatible)
        LogEntryMemory memEntry = MemoryLogParser.Parse(sampleLog.AsMemory());
        ReadOnlyMemory<char> extractedIdMem = MemoryLogParser.ExtractEventId(sampleLog.AsMemory());

        // 👇 Pegamos os resultados extraídos agorinha ali em cima e jogamos em um Grid super estiloso pro terminal
        var grid = new Grid()
            .AddColumn(new GridColumn())
            .AddColumn(new GridColumn().PadLeft(2))
            .AddColumn(new GridColumn().PadLeft(2));

        grid.AddRow("[bold]Campos Extraídos[/]", "[bold]Método Tradicional[/]", "[bold]Span<T> / Memory<T>[/]");
        grid.AddRow("Data/Hora", tradEntry.Timestamp, spanEntry.Timestamp.ToString());
        grid.AddRow("Nível", tradEntry.Level, spanEntry.Level.ToString());
        grid.AddRow("Event ID", $"{tradEntry.EventId} ({extractedIdTrad})", $"{spanEntry.EventId.ToString()} ({extractedIdSpan.ToString()})");
        grid.AddRow("Mensagem", tradEntry.Message, spanEntry.Message.ToString());
        
        AnsiConsole.Write(
            new Panel(grid)
            {
                Header = new PanelHeader("Resultados Funcionais", Justify.Left),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            });

        // Barchart for allocations metric
        AnsiConsole.MarkupLine("\n[bold]Gráfico de Alocações (Teóricas, estimadas em Bytes):[/]");
        
        var chart = new BarChart()
            .Label("[green bold underline]Alocações na Heap (Menos é Melhor)[/]")
            .CenterLabel()
            .AddItem("Tradicional", 224, Color.Red)
            .AddItem("Memory<T>", 24, Color.Orange1)
            .AddItem("Span<T>", 0, Color.Green);

        AnsiConsole.Write(chart);

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Método");
        table.AddColumn("Descrição das Alocações");

        table.AddRow("[red]Tradicional[/]", "Aloca um array de strings e 4 novas instâncias de string na Heap!");
        table.AddRow("[orange1]Memory<T>[/]", "Aloca apenas a struct wrapper (se repassada pra Heap), \nzero cópias de caracteres de texto!");
        table.AddRow("[green]Span<T>[/]", "0 BYTES ALOCADOS NA HEAP (Stack-only, Zero-Alloc)!");

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("\n[dim]Dica: Para executar os Benchmarks com precisão, passe a flag '--benchmark' ou rode:[/]");
        AnsiConsole.MarkupLine("[bold cyan]dotnet run -c Release -- --benchmark[/]\n");
    }

    private static void RunBenchmark()
    {
        Console.WriteLine("\nIniciando suíte de BenchmarkDotNet (Modo Release)...");
        BenchmarkRunner.Run<LogParserBenchmark>();
    }
}
