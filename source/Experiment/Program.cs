using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            var A = new SparseMatrix(2, 2);
            A[0, 1] = new Complex(1, 0);
            A[1, 0] = new Complex(1, 0);
            var b = new SparseVector(2);
            b[0] = new Complex(1, 0);
            b[1] = new Complex(2, 0);
            var factorization = A.LU();
            var x = factorization.Solve(b);
            Console.WriteLine(x.ToString());
            Console.ReadKey();
        }
    }
}
