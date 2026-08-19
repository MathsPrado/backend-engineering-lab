using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;

namespace BalanceServiceBenchmark;

[MemoryDiagnoser]
public class BalanceBenchmark
{
    private readonly BalanceService _service = new();
    
    // Id 1 está no cache, simulando o cenário onde 90% das requisições dão hit
    private const int ContaId = 1; 

    [Benchmark(Baseline = true)]
    public async Task<decimal> CacheHit_ComTask()
    {
        return await _service.ObterSaldoComTaskAsync(ContaId, forcarIdaAoBanco: false);
    }

    [Benchmark]
    public async ValueTask<decimal> CacheHit_ComValueTask()
    {
        return await _service.ObterSaldoComValueTaskAsync(ContaId, forcarIdaAoBanco: false);
    }
    
    [Benchmark]
    public async ValueTask<decimal> CacheHit_ComValueTask_SemAsyncKeyword()
    {
        // Neste desafio, como a função retorna ValueTask e não Task (sem async no método original),
        // ele só alocará a struct e não invocará nenhuma state machine no método.
        return await _service.ObterSaldoSemAsync(ContaId, forcarIdaAoBanco: false);
    }
}
