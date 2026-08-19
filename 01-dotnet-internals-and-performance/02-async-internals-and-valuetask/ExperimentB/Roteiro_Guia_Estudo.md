# 📘 Roteiro de Estudos: Stack vs Heap e Tipos de Valor

Este documento acompanha o código fonte do **Experimento B**. Ele resume de forma direta o que significa cada conceito envolvido no teste, e a regra de ouro de quando utilizar cada um no .NET moderno.

---

## 1. Stack vs Heap (O Básico)

*   **Stack (Pilha):** Memória rápida, de curtíssimo prazo. É onde suas variáveis locais e parâmetros de métodos vivem. Assim que uma função `()` termina (exceção para *asyncs*), a memória do Stack daquela função evapora quase que sem custo algum (zero CPU rate).
*   **Heap (Amontoado):** Memória de mais longo prazo e mais pesada gerenciada pelo longo braço do **Garbage Collector (GC)**. Criar algo ali exige buscar espaço, manter referências, e depois pedir que o GC pare seu programa (mesmo que por uns milissegundos) para varrer e remover os lixos de lá.

---

## 2. Tipos de Variáveis e Quando Usá-las

### 🔹 1. A Classe (`class`)
Uma classe é um **Tipo de Referência**. 
*   **O que é:** O endereço dela fica no *Stack*, mas seus dados volumosos e concretos ficam alocados na *Heap*. Sempre envolverão o Garbage Collector.
*   **Quando USAR:** Para 95% do seu projeto estrutural! (Serviços, Modelos de Entidade com banco de dados, Injeção de dependência, Regras de negócio, Manipuladores HTTP...).
*   **Quando NÃO USAR:** Quando você for criar MILHÕES daquele mesmo objeto minúsculo dentro num pequeno loop muito rápido (ex: coordenadas 3D de partículas em um jogo ou posições `(X, Y)` em um gerador gráfico). Fazer isso "afogará" o GC tentando limpar os bilhões de classe instanciadas rapidamente.

### 🔹 2. A Struct (`struct`)
A Struct é um **Tipo de Valor**.
*   **O que é:** Ela aloca os seus dados *direto no Stack* (sem gastar *Heap*). O GC nunca saberá nem que ela existiu.
*   **Quando USAR:** Para estruturas de dados que são MUITO pequenas (como coordenadas `X, Y, Z`, Cores em RGB `R, G, B`, etc) e que você precisa instanciar infinitamente num loop intensivo.
*   **Quando NÃO USAR:** Para objetos grandes com muitos campos; Se ela passar de 16-24 bytes, será contra produtivo copiá-la o tempo todo de lá pra cá. Outra dica de ouro: **NUNCA DEIXE UMA STRUCT SOFRER UMA MODIFICAÇÃO DE ESTADO APÓS SER CRIADA (MUTABILIDADE)!** Use variáveis públicas mas defina no momento de construir.

### 🔹 3. Readonly Struct (`readonly struct`)
*   **O que é:** Exatamente como a `struct`, porém sendo obrigada pelo compilador e ser **imutável**. O compilador C# passa a ter a confiança que ela não vai sofrer mudanças (mutabilidade), te economizando cópias desnecessárias com modificadores de in/ref.
*   **Quando USAR:** **Sempre que for usar uma *Struct*, dê preferência em criar uma *Readonly Struct*.** Hoje em dia, boas práticas mandam ser a primeira escolha. Se um dado nela precisar mudar, crie uma **nova** struct instanciada copiando parte da antiga, mas sempre construída blindada.

### 🔹 4. Ref Struct (`ref struct`)
*   **O que é:** A Restritamente Stack. Tipos de valores normais (`struct`) correm o risco de cairem na *Heap* de maneira acidental (chamado de **Boxing**). O `ref struct` resolve isso: o compilador C# PROÍBE sob pena de quebra da Build que um obj referenciado caia na *Heap*.
*   **Quando USAR:** Quase exclusivamente no desenvolvimento de alta performance focado em buffers de memórias sem alocações contínuas, ou ao usar `Span<T>` e manipular bytes, text-parsers e renderizadores pesados.
*   **Quando NÃO USAR:** No seu dia a dia comum de criação de API REST C#.
*   **O Grande Problema:** Nunca podem ser usadas dentro de uma instrução `async Task` porque blocos *async* viram classes ocultadas na Heap para suportar re-entrâncias do *await* (Estado de Máquina).

---

## 3. O Vilão das Structs: Boxing e Unboxing

Quando o C# vê que você tentou passar a sua `struct` ultraleve numa variável que espera só um `object` ou para uma interface (`IInterface`), o compilador não quebra, mas ele precisa alocá-la com urgência... E adivinha? A sua Struct foi **empacotada silenciosamente para a Heap (Heap Allocation)**.
Isso se chama **BOXING**. E ao tentar forçar ler ela de volta, o custo ocorre de novo como **UNBOXING**.

Para evitar ao máximo isso no .NET (isso evita centenas de vazamentos de memória na indústria): **Nunca interaja sua struct com listas velhas (`ArrayList`). Sempre declare uma Lista Genérica forte do tamanho exato do formato (`List<SuaStructAqui>`) que resolverá os seus problemas.**
