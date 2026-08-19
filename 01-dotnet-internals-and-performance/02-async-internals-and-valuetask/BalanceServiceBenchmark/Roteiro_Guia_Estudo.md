# Roteiro de Estudo: Programação Assíncrona Profunda no .NET

Este guia foi elaborado para acompanhar a solução implementada no `BalanceServiceBenchmark`. Ele visa aprofundar o entendimento sobre os impactos de performance do uso indiscriminado de código assíncrono em cenários de alta frequência (Hot paths) e como utilizar corretamente as ferramentas oferecidas pelo .NET.

---

## 📚 1. O Problema: `Task<T>` e a Alocação de Memória
A classe `Task<T>` (e a `Task` sem tipo) é um tipo **por referência** (class) dentro do .NET.
Isso significa que, toda vez que uma Task é instanciada e retornada, haverá **alocação no Heap** gerenciada pelo Garbage Collector.

### Quando isso dói?
No nosso contexto de *Fintech*: 90% das chamadas a `ObterSaldoAsync()` caem na condição de **Cache Hit** (dado já em memória). 
Se o método sempre retorna `Task<decimal>`, mesmo que a execução síncrona resolva imediatamente, o `AsyncTaskMethodBuilder` ainda vai ter que criar e alocar o objeto no Heap para acomodar a State Machine e o retorno. Com milhões de chamadas por segundo, criamos as famosas pequenas alocações ("Garbage"), disparando o Gen 0 do Garbage Collector com muita frequência e, por fim, degradando a performance geral da aplicação.

---

## 🛠️ 2. A Solução Inicial: `ValueTask<T>`
Introduzida para resolver esse problema crítico, a `ValueTask<T>` é uma **Struct** (tipo por valor).

### Como ela ajuda?
Ao usar `async ValueTask<decimal>`, ganhamos a flexibilidade de:
1. Retornar um valor diretamente no caminho síncrono que alocará apenas a Struct **(Zero alocações no Heap)**.
2. Contar com a mesma promessa assíncrona da `Task<T>` via invólucro do objeto subjacente (State Machine/Task), caso o modo de espera *await* seja realmente ativado (ex: Cache Miss, indo ao banco de dados).

**Importante:** Nunca utilize `ValueTask` de forma repetida na mesma variável via múltiplos `await`, nem aguarde concurrentemente a mesma `ValueTask`. Ela deve consumida uma única vez.

---

## 🚀 3. O "Ouro" Escondido: Fugindo da Palavra-chave `async`

Você deve ter notado que mesmo a `async ValueTask<T>` gera uma sobrecarga de ~25 ns (contra ~35 ns da `Task<T>`).
Isso ocorre devido ao modificador `async` na assinatura original.
Qualquer método com a palavra-chave `async` obriga o compilador C# a **gerar automaticamente uma Máquina de Estados (State Machine)**, empacotar argumentos e configurar um Builder de Async — há um peso atrelado a esse processo!

### O Padrão "Fast-Path" síncrono e "Slow-Path" assíncrono
Para extrairmos do código performance de ponta (como as encontradas nos pacotes e bibliotecas core do .NET), dividimos as responsabilidades e removemos o `async` da assinatura principal.

**Como implementamos?**
1. Criamos um método simples `ValueTask<T> MeuMetodo()` (sem `async`).
2. Avaliamos nossa condição de Cache (o *Fast-Path*).
3. Se o Cache Hit sucesso, disparamos a Struct pura `return new ValueTask<T>(valor)`. Isso tem peso computacional trivial (~14 ns).
4. Se o Cache der Miss (o *Slow-Path*), encadeamos a chamada para um método auxiliar privado `private async ValueTask<T> MeuMetodoAsyncInterno()`!

Este padrão impede que o runtime suba a State Machine nos 90% dos casos de Cache Hit provando-se ser a implementação definitiva de alta performance.

---

## 🧪 4. Execute você mesmo o Benchmark!

Para observar e validar o conteúdo desse estudo, use o Benchmark que configuramos:

1. Acesse o diretório do experimento:
   ```bash
   cd 02-async-internals-and-valuetask/BalanceServiceBenchmark
   ```
2. Mande rodar o Benchmark no modo de "Lançamento" (Release), fundamental para testes de performance onde os otimizadores estão ligados:
   ```bash
   dotnet run -c Release
   ```
3. Leia a coluna `Allocated` e perceba que Task alocou 160 Bytes por chamada enquanto os ValueTask rodaram limpinhos com 0 B alocados. Analise a coluna `Mean` para validar os ganhos de tempo descritos no tópico 3.

---

## ✨ Próximos passos e aprofundamento
- Pesquise sobre **ConfigureAwait(false)** e como ele previne deadlocks libertando o **SynchronizationContext** clássico (necessário principalmente para bibliotecas).
- Estude mais sobre os novos componentes de alta performance como: `IValueTaskSource`, `Span<T>` e `.AsTask()`.
