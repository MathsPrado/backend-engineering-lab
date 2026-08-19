using System;
using System.Threading.Tasks;

namespace ExperimentB
{
    public class AsyncLimitation
    {
        // === DESCOMENTE O CÓDIGO ABAIXO PARA VER O ERRO DO COMPILADOR ===
        
        /*
        public async Task TryAsyncRefStruct(Point3DRefStruct p)
        {
            // O compilador constroi uma Máquina de Estado (State Machine) na Heap
            // para salvar o estado da função quando há um `await`.
            // Como 'Point3DRefStruct' é uma `ref struct`, ela nunca pode ir para a Heap!
            
            Console.WriteLine("Iniciando task assícrona com ref struct...");
            await Task.Delay(100);
            
            p.Print(); // Erro de Compilação: CS4008
        }
        */

        public void TrySyncRefStruct(Point3DRefStruct p)
        {
            // Isso funciona perfeitamente, pois é um método síncrono.
            // A execução entra no método (novo frame no Stack), a variável é usada, e o método acaba (limpa o frame do Stack).
            p.Print();
            Console.WriteLine("Método síncrono aceita ref struct no Stack lindamente!");
        }
    }
}
