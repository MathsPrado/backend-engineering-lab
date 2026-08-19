using System;
using System.Collections;
using System.Collections.Generic;

namespace ExperimentB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Experimento B: Stack vs Heap ===");

            // 1. Instanciando na Heap
            var pointClass = new Point3DClass(1.0, 2.0, 3.0);
            
            // 2. Instanciando no Stack (nenhum custo imediato pro GC)
            var pointStruct = new Point3DStruct(4.0, 5.0, 6.0);
            var pointReadonly = new Point3DReadonlyStruct(7.0, 8.0, 9.0);
            var pointRefStruct = new Point3DRefStruct(10.0, 11.0, 12.0);

            pointClass.Print();
            pointStruct.Print();
            pointReadonly.Print();
            pointRefStruct.Print();

            Console.WriteLine("\n=== Simulando Boxing ===");

            // BOXING: pointStruct é convertido em object (Heap)
            object boxedStruct = pointStruct;
            Console.WriteLine("Boxing em 'object' concluído.");

            // UNBOXING: trazendo de volta para a Stack
            Point3DStruct unboxedStruct = (Point3DStruct)boxedStruct;
            Console.WriteLine("Unboxing de 'object' concluído.");

            // BOXING DISFARÇADO (Interfaces)
            // Mesmo implementando ICoordinate, quando castamos uma struct para a interface, ela vai pra heap!
            Console.WriteLine("\nPassando struct como interface ICoordinate...");
            PrintAnyCoordinate(pointStruct); // BOXING ACONTECE AQUI!

            // ArrayList - Cuidado antigo: Tudo aqui vira object (GC chora)
            ArrayList listA = new ArrayList();
            listA.Add(pointStruct); // BOXING

            // Evitando Boxing: List<T> Genérica salva o dia
            List<Point3DStruct> listB = new List<Point3DStruct>();
            listB.Add(pointStruct); // NÃO HÁ BOXING. Uma cópia da struct vai para o array interno da List, mas SEM object cast.
            
            Console.WriteLine("\nList<Point3DStruct> adicionado sem boxing!");

            Console.WriteLine("\n=== Limitação de Ref Struct ===");
            // pointRefStruct não pode sofrer Boxing por design do .NET.
            // object boxedRef = pointRefStruct; // ERRO: Não pode converter ref struct para object.
            // PrintAnyCoordinate(pointRefStruct); // ERRO: Ref struct não pode implementar interface justamente para evitar boxing!
            
            Console.WriteLine("\nExperimentação da Stack vs Heap concluída.");
          
            Console.ReadKey();
        }

        static void PrintAnyCoordinate(ICoordinate coordinate)
        {
            // Se o que chegou aqui foi uma struct (como Point3DStruct), ela sofreu boxing!
            coordinate.Print();
        }
    }
}
