using System.Collections.Generic;
using System.Threading.Tasks;

namespace BalanceServiceBenchmark;

public class BalanceService
{
    private readonly Dictionary<int, decimal> _cache;

    public BalanceService()
    {
        // Simulating some pre-loaded cache
        _cache = new Dictionary<int, decimal>
        {
            { 1, 100.50m },
            { 2, 200.75m },
            { 3, 50.00m },
            { 4, 1500.00m },
            { 5, 0.00m }
        };
    }

    // Método A: Retornando Task<decimal>
    public async Task<decimal> ObterSaldoComTaskAsync(int id, bool forcarIdaAoBanco)
    {
        if (!forcarIdaAoBanco && _cache.TryGetValue(id, out var saldo))
        {
            // Caminho síncrono - O dado já está disponível, mas o compilador ainda precisa 
            // criar uma Task<decimal> (pois a assinatura do método exige). Alocação no heap!
            return saldo;
        }

        // Caminho assíncrono (simulando ida ao banco/cache miss)
        await Task.Delay(10);
        return 10.0m;
    }

    // Método B: Retornando ValueTask<decimal>
    public async ValueTask<decimal> ObterSaldoComValueTaskAsync(int id, bool forcarIdaAoBanco)
    {
        if (!forcarIdaAoBanco && _cache.TryGetValue(id, out var saldo))
        {
            // Caminho síncrono - A máquina de estados cria a struct ValueTask<decimal>
            // evitando a alocação de objeto no heap, na maioria dos casos.
            return saldo; 
        }

        // Caminho assíncrono (simulando ida ao banco/cache miss)
        await Task.Delay(10);
        return 10.0m;
    }

    // Desafio Extra: ValueTask sem 'async'
    // Evita a criação prévia de uma máquina de estados (que tem seu próprio pequeno overhead).
    public ValueTask<decimal> ObterSaldoSemAsync(int id, bool forcarIdaAoBanco)
    {
        if (!forcarIdaAoBanco && _cache.TryGetValue(id, out var saldo))
        {
            // Retorna o struct contendo o resultado sem qualquer overhead de StateMachine.
            // Alocação ZERO e execução extremamente rápida.
            return new ValueTask<decimal>(saldo);
        }

        // Se precisar de async, chama o método privado que gera a máquina de estados.
        return ObterSaldoDoBancoAsync(id);
    }

    private async ValueTask<decimal> ObterSaldoDoBancoAsync(int id)
    {
        await Task.Delay(10);
        return 10.0m;
    }
}
