using System;

namespace ExperimentB
{
    // Interface para testar Boxing
    public interface ICoordinate
    {
        double X { get; }
        double Y { get; }
        double Z { get; }
        void Print();
    }

    // 1. Classe - Tipo de Referência (Alocada na Heap)
    public class Point3DClass : ICoordinate
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3DClass(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Print() => Console.WriteLine($"Class -> X:{X}, Y:{Y}, Z:{Z}");
    }

    // 2. Struct Comum - Tipo de Valor (Alocada na Stack)
    // Permite mutabilidade (o que é uma má prática em structs se não for planejado)
    public struct Point3DStruct : ICoordinate
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3DStruct(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Print() => Console.WriteLine($"Struct -> X:{X}, Y:{Y}, Z:{Z}");
    }

    // 3. Readonly Struct - Tipo de Valor Imutável (Alocada na Stack)
    // Mais otimizada (compilador não precisa criar cópias defensivas em passagens 'in')
    public readonly struct Point3DReadonlyStruct : ICoordinate
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3DReadonlyStruct(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Print() => Console.WriteLine($"Readonly Struct -> X:{X}, Y:{Y}, Z:{Z}");
    }

    // 4. Ref Struct - Vive apenas na vida útil do Stack de quem a chamou
    // (Aviso: Não pode implementar interfaces a partir do C# ~7/8, porém desde C# 13 há novidades.
    // Via de regra (C# até 12/13 base), ref structs não podem implementar interfaces e sofrer boxing,
    // garantindo zero alocações acidentais.)
    public ref struct Point3DRefStruct
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public Point3DRefStruct(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public void Print() => Console.WriteLine($"Ref Struct -> X:{X}, Y:{Y}, Z:{Z}");
    }
}
