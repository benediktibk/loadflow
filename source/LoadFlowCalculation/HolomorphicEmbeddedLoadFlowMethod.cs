using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Factorization;
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

        public override Vector<Complex> CalculateUnknownVoltages(
            Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents,
            Vector<Complex> knownPowers)
        {
            var nodeCount = admittances.RowCount;
            var factorization = admittances.QR();
            var coefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            var inverseCoefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);

            var admittanceRowSum = CalculateAdmittanceRowSum(admittances);
            var firstCoefficient = CalculateFirstCoefficient(factorization, admittances, admittanceRowSum);
            Vector<Complex> firstInverseCoefficient = new DenseVector(nodeCount);
            firstCoefficient.DivideByThis(new Complex(1, 0), firstInverseCoefficient);
            coefficients.Add(firstCoefficient);
            inverseCoefficients.Add(firstInverseCoefficient);
            var n = 0;
            double coefficientNorm;

            do
            {
                ++n;
                var newCoefficient = CalculateNextCoefficient(inverseCoefficients[n - 2], factorization,
                    knownPowers, constantCurrents);
                coefficients.Add(newCoefficient);
                var newInverseCoefficient = CalculateNextInverseCoefficient(coefficients, inverseCoefficients);
                inverseCoefficients.Add(newInverseCoefficient);

                coefficientNorm = newCoefficient.SumMagnitudes().Magnitude;
            } while (n <= _maximumNumberOfCoefficients && coefficientNorm > _coefficientTerminationCriteria);

            // TODO use an analytic continuation here
            Vector<Complex> voltages = new DenseVector(nodeCount);
            foreach (var ci in coefficients)
                voltages = voltages.Add(ci);

            return voltages;
        }

        private Vector<Complex> CalculateAdmittanceRowSum(Matrix<Complex> admittances)
        {
            Vector<Complex> rowSum = new DenseVector(admittances.RowCount);

            foreach (var column in admittances.ColumnEnumerator())
                rowSum = rowSum.Add(column.Item2);

            return rowSum;
        }

        private Vector<Complex> CalculateFirstCoefficient(ISolver<Complex> factorization, Matrix<Complex> admittances, Vector<Complex> admittanceRowSum)
        {
            return factorization.Solve(admittanceRowSum.Multiply(new Complex(-1, 0)));
        }

        private Vector<Complex> CalculateNextCoefficient(Vector<Complex> previousInverseCoefficient, ISolver<Complex> factorization, Vector<Complex> powers, Vector<Complex> constantCurrents)
        {
            var ownCurrents = (powers.PointwiseMultiply(previousInverseCoefficient)).Conjugate();
            var totalCurrents = constantCurrents.Add(ownCurrents);
            return factorization.Solve(totalCurrents);
        }

        private Vector<Complex> CalculateNextInverseCoefficient(List<Vector<Complex>> coefficients,
            List<Vector<Complex>> inverseCoefficients)
        {
            var coefficientCount = coefficients.Count;

            if (coefficientCount < 2)
                throw new ArgumentOutOfRangeException("coefficients",
                    "there must be at least two coefficients already set");

            if (inverseCoefficients.Count != coefficientCount - 1)
                throw new ArgumentOutOfRangeException("inverseCoefficients",
                    "the count of inverse coefficients is invalid");

            var n = coefficientCount - 1;
            var nodeCount = coefficients[0].Count;
            Vector<Complex> newInverseCoefficient = new DenseVector(nodeCount);

            for (var i = 0; i < n; ++i)
            {
                var inverseCoefficient = inverseCoefficients[i];
                var coefficient = coefficients[n - i];
                var summand = inverseCoefficient.PointwiseMultiply(coefficient);
                newInverseCoefficient = newInverseCoefficient.Add(summand);
            }

            return newInverseCoefficient.Multiply(new Complex(-1, 0));
        }
    }
}