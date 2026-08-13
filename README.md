# 🚀 Backend Engineering Lab

Repositório de laboratório prático focado em **Performance**, **Arquitetura de Software (.NET)**, **Microserviços**, **Mensageria** e **Observabilidade**.

---

## 📂 Estrutura do Repositório

```text
backend-engineering-lab/
├── 01-dotnet-internals-and-performance/
│   ├── 01-span-and-memory-benchmarks/    # Manipulação de Texto, Span<T>, Memory<T> e BenchmarkDotNet
│   └── 02-async-internals-and-valuetask/ # Async state machines, ValueTask vs Task
├── 02-architecture-and-ddd/
│   ├── 01-clean-architecture-template/    # DDD, Clean Architecture e Result Pattern
│   └── 02-cqrs-mediatr-sample/           # CQRS com MediatR e Pipeline Behaviors
├── 03-microservices-and-resilience/      # Resilience (Polly), Rate Limiting, API Gateways
├── 04-messaging-and-event-driven/        # RabbitMQ, Kafka e Event Sourcing
├── 05-cloud-containers-and-observability/# Docker, OpenTelemetry, Jaeger e Prometheus
└── 06-react-frontend/                    # Front-ends e Dashboards de suporte
```

---

## 🛠️ Módulos e Experimentos

### 01. .NET Internals & Performance

#### Experimento 01: Manipulação de Texto & Alocação de Memória (`Span<T>` e `Memory<T>`)
- **Caminho**: `01-dotnet-internals-and-performance/01-span-and-memory-benchmarks/`
- **Objetivo**: Extrair dados e fatiar strings de log de alto volume sem gerar alocações na Heap.
- **Roteiro Didático**: [ROTEIRO_EXPERIMENTO_A.md](file:///Users/matheusprado/Desktop/projeto%206%20meses%20estudo/01-dotnet-internals-and-performance/01-span-and-memory-benchmarks/ROTEIRO_EXPERIMENTO_A.md)

##### Resultados de Benchmark (Empíricos com BenchmarkDotNet em Apple M1 / .NET 10.0):

| Método | Tempo Médio (`Mean`) | Ratio | Gen 0 | Alocado na Heap (`Allocated`) |
| :--- | :---: | :---: | :---: | :---: |
| **`Extract ID - Span<T> (Zero Alloc)`** | **6.76 ns** | **0.51** | **-** | **0 Bytes** |
| **`Parse Full - Span<T> (Zero Alloc)`** | **7.57 ns** | **0.57** | **-** | **0 Bytes** |
| **`Parse Full - Memory<T>`** | **10.39 ns** | **0.78** | **-** | **0 Bytes** |
| **`Extract ID - Traditional (.Substring)`** | **13.35 ns** | **1.00** | **0.0063** | **40 Bytes** |
| **`Parse Full - Traditional (.Split)`** | **56.88 ns** | **4.26** | **0.0573** | **360 Bytes** |

> [!TIP]
> **Ganho Observado**: O método `Span<T>` zerou a alocação de memória na Heap (**0 Bytes** / **Zero-Alloc**) e executou **7.5x mais rápido** que a abordagem tradicional `.Split()`.

---

## 💻 Como Executar os Experimentos

### Experimento 01 - Span & Memory Benchmarks

```bash
# 1. Navegar até o repositório do experimento
cd 01-dotnet-internals-and-performance/01-span-and-memory-benchmarks/ExperimentoA.SpanMemory

# 2. Executar a verificação funcional (Debug)
dotnet run -c Debug

# 3. Executar a suíte de BenchmarkDotNet (Release)
dotnet run -c Release -- --benchmark
```

---

## 📜 Licença
Este repositório é mantido para fins de estudos em .NET.
