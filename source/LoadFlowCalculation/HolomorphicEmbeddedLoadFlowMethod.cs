using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethod :
        LoadFlowCalculator
    {
        private readonly double _coefficientTerminationCriteria;
        private readonly int _maximumNumberOfCoefficients;

        public HolomorphicEmbeddedLoadFlowMethod(double coefficientTerminationCriteria, int maximumNumberOfCoefficients)
        {
            _coefficientTerminationCriteria = coefficientTerminationCriteria;
            _maximumNumberOfCoefficients = maximumNumberOfCoefficients;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var nodeCount = admittances.RowCount;
            var factorization = admittances.QR();
            var c = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            var d = new List<Vector<Complex>>(_maximumNumberOfCoefficients);

            var c0 = factorization.Solve(new SparseVector(nodeCount));
            Vector<Complex> d0 = new DenseVector(nodeCount);
            c0.DivideByThis(new Complex(1, 0), d0);
            c.Add(c0);
            d.Add(d0);
            var n = 0;
            double coefficientNorm;

            do
            {
                ++n;
                var previousD = d[n - 1];
                var ownCurrents = (knownPowers.PointwiseMultiply(previousD)).Conjugate();
                var rightHandSide = constantCurrents.Add(ownCurrents);
                var newC = factorization.Solve(rightHandSide);
                c.Add(newC);
                Vector<Complex> newD = new DenseVector(nodeCount);

                for (var m = 0; m <= n - 1; ++m)
                    newD = newD.Subtract(c[n - m].PointwiseMultiply(d[m]));
                newD = newD.PointwiseDivide(c[0]);

                d.Add(newD);
                coefficientNorm = newC.SumMagnitudes().Magnitude;
            } while (n <= _maximumNumberOfCoefficients && coefficientNorm > _coefficientTerminationCriteria);

            // TODO use an analytic continuation here
            Vector<Complex> voltages = new DenseVector(nodeCount);
            foreach (var ci in c)
                voltages = voltages.Add(ci);

            return voltages;
        }
    }
}
