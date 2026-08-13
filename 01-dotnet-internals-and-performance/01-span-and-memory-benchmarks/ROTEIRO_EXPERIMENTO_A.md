# Roteiro de Experimento Prático C# - Experimento A
## Manipulação de Texto e Alocação de Memória (`Span<T>` e `Memory<T>`)

---

### 1. Visão Geral e Fundamentos Teóricos

No ecossistema .NET, a manipulação de strings é uma das maiores fontes de alocação desnecessária de memória na **Heap**. Como a classe `string` é **imutável**, operações comuns como `.Split()`, `.Substring()`, `.Replace()` ou concatenação sempre criam novas instâncias de objetos na memória Heap gerenciada pelo Garbage Collector (GC).

#### O Problema das Alocações e o Garbage Collector (GC)
- **Heap vs. Stack**:
  - **Stack (Pilha)**: Memória ultra-rápida, organizada por escopo de execução (frames de métodos). Desalocada automaticamente assim que o método encerra.
  - **Heap (Monte)**: Memória gerenciada onde objetos vivem. Exige alocação dinâmica e requer a atuação do **Garbage Collector (GC)** para liberar espaço.
- **Gerações do GC (Gen 0, Gen 1, Gen 2)**:
  - Novas alocações de curta duração (como strings geradas por `.Split()`) entram na **Gen 0**.
  - Quanto mais alocações ocorrem na Heap, mais frequentemente o GC precisa pausar a execução (GC Pauses) para coletar a Gen 0, impactando o *throughput* e a latência de aplicações de alta performance.

#### Solução: `Span<T>` e `Memory<T>`
Introduzidos no .NET Core / C#, `Span<T>` e `Memory<T>` representam visões contíguas e seguras de qualquer bloco de memória (strings, arrays, memória nativa ou de pilha).

| Tipo | Localização de Vida | Cenários de Uso | Pode ser usado em `async/await` ou campos de `class`? |
| :--- | :--- | :--- | :---: |
| `Span<T>` / `ReadOnlySpan<char>` | **Apenas na Stack** (`ref struct`) | Algoritmos de parsing rápido, extração síncrona sem alocação (Zero-Alloc). | ❌ Não (regra do `ref struct`) |
| `Memory<T>` / `ReadOnlyMemory<char>` | **Stack ou Heap** (`struct`) | Transmissão de fatias de memória em métodos `async`, tarefas assíncronas ou armazenamento em classes. |  Sim |

---

### 2. O Problema Prático

Dada uma linha de log longa no formato:
`"2026-08-12T16:25:00|WARN|ID_998811|Erro de conexão com o banco de dados principal"`

Precisamos extrair os dados da linha de log (Data/Hora, Nível, ID do Evento e Mensagem) ou isolar apenas o ID do evento (`ID_998811`).

---

### 3. Comparativo de Implementações

#### A. Abordagem Tradicional (`string.Split` e `Substring`)
```csharp
public static LogEntryTraditional Parse(string logLine)
{
    // Aloca 1 array de strings (string[]) + 4 instâncias de string na Heap (360 Bytes)
    string[] parts = logLine.Split('|');
    return new LogEntryTraditional(parts[0], parts[1], parts[2], parts[3]);
}

public static string ExtractEventId(string logLine)
{
    int firstPipe = logLine.IndexOf('|');
    int secondPipe = logLine.IndexOf('|', firstPipe + 1);
    int thirdPipe = logLine.IndexOf('|', secondPipe + 1);

    // Substring aloca uma nova string na Heap (40 Bytes)
    return logLine.Substring(secondPipe + 1, thirdPipe - secondPipe - 1);
}
```

#### B. Abordagem de Alta Performance (`ReadOnlySpan<char>` - Zero Allocation)
```csharp
public static LogEntrySpan Parse(ReadOnlySpan<char> logLine)
{
    int firstPipe = logLine.IndexOf('|');
    ReadOnlySpan<char> timestamp = logLine.Slice(0, firstPipe);
    ReadOnlySpan<char> remainder1 = logLine.Slice(firstPipe + 1);

    int secondPipe = remainder1.IndexOf('|');
    ReadOnlySpan<char> level = remainder1.Slice(0, secondPipe);
    ReadOnlySpan<char> remainder2 = remainder1.Slice(secondPipe + 1);

    int thirdPipe = remainder2.IndexOf('|');
    ReadOnlySpan<char> eventId = remainder2.Slice(0, thirdPipe);
    ReadOnlySpan<char> message = remainder2.Slice(thirdPipe + 1);

    // Nenhuma string nova criada! Retorna visões sobre a memória original (0 Bytes na Heap)
    return new LogEntrySpan(timestamp, level, eventId, message);
}
```

#### C. Abordagem Assíncrona / Heap Safe (`ReadOnlyMemory<char>`)
```csharp
public static LogEntryMemory Parse(ReadOnlyMemory<char> logLine)
{
    ReadOnlySpan<char> span = logLine.Span;
    int firstPipe = span.IndexOf('|');
    ReadOnlyMemory<char> timestamp = logLine.Slice(0, firstPipe);
    ReadOnlyMemory<char> remainder1 = logLine.Slice(firstPipe + 1);

    int secondPipe = remainder1.Span.IndexOf('|');
    ReadOnlyMemory<char> level = remainder1.Slice(0, secondPipe);
    ReadOnlyMemory<char> remainder2 = remainder1.Slice(secondPipe + 1);

    int thirdPipe = remainder2.Span.IndexOf('|');
    ReadOnlyMemory<char> eventId = remainder2.Slice(0, thirdPipe);
    ReadOnlyMemory<char> message = remainder2.Slice(thirdPipe + 1);

    return new LogEntryMemory(timestamp, level, eventId, message);
}
```

---

### 4. Resultados Reais do BenchmarkDotNet (`[MemoryDiagnoser]`)

Ambiente de Teste: Apple M1 (Arm64 RyuJIT), .NET 10.0.3, macOS Tahoe.

```
| Method                                  | Mean      | Error     | StdDev    | Ratio | Rank | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |----------:|----------:|----------:|------:|-----:|-------:|----------:|------------:|
| 'Extract ID - Span<T> (Zero Alloc)'     |  6.761 ns | 0.1307 ns | 0.1453 ns |  0.51 |    1 |      - |         - |        0.00 |
| 'Parse Full - Span<T> (Zero Alloc)'     |  7.576 ns | 0.1186 ns | 0.0926 ns |  0.57 |    2 |      - |         - |        0.00 |
| 'Parse Full - Memory<T>'                | 10.398 ns | 0.2213 ns | 0.1962 ns |  0.78 |    3 |      - |         - |        0.00 |
| 'Extract ID - Memory<T>'                | 10.435 ns | 0.0970 ns | 0.0810 ns |  0.78 |    3 |      - |         - |        0.00 |
| 'Extract ID - Traditional (.Substring)' | 13.357 ns | 0.2208 ns | 0.1724 ns |  1.00 |    4 | 0.0063 |      40 B |        1.00 |
| 'Parse Full - Traditional (.Split)'     | 56.887 ns | 0.2006 ns | 0.1675 ns |  4.26 |    5 | 0.0573 |     360 B |        9.00 |
```

---

### 5. Análise dos Resultados

1. **Alocação na Heap (`Allocated`)**:
   - **`Span<T>`**: **`0 B` (Zero Allocation)**. O fatiamento ocorre exclusivamente na Stack via janela contígua sobre a string original.
   - **`Memory<T>`**: **`0 B`**. O fatiamento cria structs `ReadOnlyMemory<char>` apontando para a string original sem copiar caracteres ou criar novos objetos Heap.
   - **`Traditional (.Substring)`**: **`40 B`** por invocação (aloca uma nova string para o ID).
   - **`Traditional (.Split)`**: **`360 B`** por invocação (aloca o array `string[]` + 4 strings individuais na Heap).

2. **Impacto no Garbage Collector (`Gen 0`)**:
   - `Span<T>` e `Memory<T>` marcam **`-`** (zero coleções de GC).
   - O método `.Split()` gerou **0.0573** coletas na Gen 0 a cada 1.000 chamadas. Em um ambiente com 1.000.000 de logs/segundo, o `.Split()` geraria **360 MB de lixo na Heap**, forçando milhares de coleções do GC.

3. **Tempo de Execução (`Mean`)**:
   - `Parse Full - Span<T>` (**7.57 ns**) é **7.5 vezes mais rápido** que o `Parse Full - Traditional (.Split)` (**56.88 ns**).

---

### 6. Conclusão Prática

- Use **`ReadOnlySpan<char>`** sempre que estiver realizando **parsing síncrono**, validações, cálculos de substring ou tokenização dentro do escopo de um método.
- Use **`ReadOnlyMemory<char>`** quando precisar passar fatias de dados entre chamadas `async/await`, armazená-las em instâncias de `class` ou filas na memória sem duplicar arrays de caracteres.
- Evite **`string.Split()`** e **`Substring()`** em caminhos críticos (hot paths) de aplicações de alto volume (ex: APIs REST, consumidores Kafka/RabbitMQ, leitores de arquivos de log).
