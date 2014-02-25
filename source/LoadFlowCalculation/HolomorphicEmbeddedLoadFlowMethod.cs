using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnalyticContinuation;
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

        public override Vector<Complex> CalculateUnknownVoltages(
            Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents,
            Vector<Complex> knownPowers, out bool voltageCollapse)
        {
            voltageCollapse = false;
            var voltagePowerSeries = CalculateVoltagePowerSeries(admittances, constantCurrents, knownPowers);
            var voltageAnalyticContinuation = CreateVoltageAnalyticContinuation(voltagePowerSeries);
            return CalculateVoltagesWithAnalyticContinuations(voltageAnalyticContinuation);
        }

        public PowerSeriesComplex[] CalculateVoltagePowerSeries(Matrix<Complex> admittances, Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var factorization = admittances.QR();
            List<Vector<Complex>> coefficients;
            List<Vector<Complex>> inverseCoefficients;
            CalculateInitialCoefficients(admittances, constantCurrents, knownPowers, factorization, out coefficients,
                out inverseCoefficients);
            var n = 2;
            double coefficientNorm;

            do
            {
                var newCoefficient = CalculateNextCoefficient(inverseCoefficients[inverseCoefficients.Count - 1], factorization,
                    knownPowers);
                coefficients.Add(newCoefficient);
                var newInverseCoefficient = CalculateNextInverseCoefficient(coefficients, inverseCoefficients);
                inverseCoefficients.Add(newInverseCoefficient);

                coefficientNorm = newCoefficient.SumMagnitudes().Magnitude;
                ++n;
            } while (n < _maximumNumberOfCoefficients && coefficientNorm > _coefficientTerminationCriteria);

            var voltagePowerSeries = CreateVoltagePowerSeriesFromCoefficients(coefficients);
            return voltagePowerSeries;
        }

        private static IAnalyticContinuation<Complex>[] CreateVoltageAnalyticContinuation(PowerSeriesComplex[] powerSeries)
        {
            var nodeCount = powerSeries.Count();
            var analyticContinuations = new IAnalyticContinuation<Complex>[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                analyticContinuations[i] = new EpsilonAlgorithm<Complex>(powerSeries[i]);

            return analyticContinuations;
        }

        private static Vector<Complex> CalculateVoltagesWithAnalyticContinuations(IAnalyticContinuation<Complex>[] padeApproximants)
        {
            var nodeCount = padeApproximants.Count();
            var voltages = new Complex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                voltages[i] = padeApproximants[i].EvaluateAt1();

            return new DenseVector(voltages);
        }

        private static PowerSeriesComplex[] CreateVoltagePowerSeriesFromCoefficients(List<Vector<Complex>> coefficients)
        {
            var coefficientCount = coefficients.Count;
            if (coefficientCount < 1)
                throw new ArgumentOutOfRangeException("coefficients", "there must be at least one coefficient");

            var nodeCount = coefficients[0].Count;
            var voltages = new PowerSeriesComplex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                voltages[i] = new PowerSeriesComplex(coefficientCount); 

            for (var i = 0; i < coefficientCount; ++i)
            {
                var coefficient = coefficients[i];

                for (var j = 0; j < nodeCount; ++j)
                    voltages[j][i] = coefficient[j];
            }

            return voltages;
        }

        private void CalculateInitialCoefficients(Matrix<Complex> admittances, Vector<Complex> constantCurrents, Vector<Complex> knownPowers, ISolver<Complex> factorization,
            out List<Vector<Complex>> coefficients, out List<Vector<Complex>> inverseCoefficients)
        {
            var nodeCount = admittances.RowCount;
            coefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            inverseCoefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);

            var admittanceRowSum = CalculateAdmittanceRowSum(admittances);
            var firstCoefficient = CalculateFirstCoefficient(factorization, admittances, admittanceRowSum);
            Vector<Complex> firstInverseCoefficient = new DenseVector(nodeCount);
            firstCoefficient.DivideByThis(new Complex(1, 0), firstInverseCoefficient);
            coefficients.Add(firstCoefficient);
            inverseCoefficients.Add(firstInverseCoefficient);
            var secondCoefficient = CalculateSecondCoefficient(firstInverseCoefficient, factorization, knownPowers,
                constantCurrents, admittanceRowSum);
            coefficients.Add(secondCoefficient);
            var secondInverseCoefficient = CalculateNextInverseCoefficient(coefficients, inverseCoefficients);
            inverseCoefficients.Add(secondInverseCoefficient);
        }

        private static Vector<Complex> CalculateAdmittanceRowSum(Matrix<Complex> admittances)
        {
            Vector<Complex> rowSum = new DenseVector(admittances.RowCount);

            foreach (var column in admittances.ColumnEnumerator())
                rowSum = rowSum.Add(column.Item2);

            return rowSum;
        }

        private static Vector<Complex> CalculateFirstCoefficient(ISolver<Complex> factorization, Matrix<Complex> admittances, Vector<Complex> admittanceRowSum)
        {
            return factorization.Solve(admittanceRowSum.Multiply(new Complex(-1, 0)));
        }

        private static Vector<Complex> CalculateSecondCoefficient(Vector<Complex> previousInverseCoefficient,
            ISolver<Complex> factorization, Vector<Complex> powers, Vector<Complex> constantCurrents,
            Vector<Complex> admittanceRowSum)
        {
            var ownCurrents = (powers.PointwiseMultiply(previousInverseCoefficient)).Conjugate();
            var totalCurrents = constantCurrents.Add(ownCurrents);
            return factorization.Solve(totalCurrents.Add(admittanceRowSum));
        }

        private static Vector<Complex> CalculateNextCoefficient(Vector<Complex> previousInverseCoefficient, ISolver<Complex> factorization, Vector<Complex> powers)
        {
            var currents = (powers.PointwiseMultiply(previousInverseCoefficient)).Conjugate();
            return factorization.Solve(currents);
        }

        private static Vector<Complex> CalculateNextInverseCoefficient(List<Vector<Complex>> coefficients,
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

            newInverseCoefficient = newInverseCoefficient.PointwiseDivide(coefficients[0]);
            return newInverseCoefficient.Multiply(new Complex(-1, 0));
        }
    }
}