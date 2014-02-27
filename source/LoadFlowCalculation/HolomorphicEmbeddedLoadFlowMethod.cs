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
        private readonly double _targetPrecision;
        private readonly int _maximumNumberOfCoefficients;
        private List<Vector<Complex>> _coefficients;
        private List<Vector<Complex>> _inverseCoefficients;
        private PowerSeriesComplex[] _voltagePowerSeries;
        private bool[] _voltageConverged;

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int maximumNumberOfCoefficients)
        {
            if (maximumNumberOfCoefficients < 4)
                throw new ArgumentOutOfRangeException("maximumNumberOfCoefficients",
                    "there must be at least 4 coefficients");

            _targetPrecision = targetPrecision;
            _maximumNumberOfCoefficients = maximumNumberOfCoefficients;
        }

        public override Vector<Complex> CalculateUnknownVoltages(
            Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents,
            Vector<Complex> knownPowers, out bool voltageCollapse)
        {
            var factorization = admittances.QR();
            InitializeAlgorithm(admittances, constantCurrents, knownPowers, factorization);
            var voltageAnalyticContinuation = CreateVoltageAnalyticContinuation();
            var lastVoltage = CalculateVoltagesWithAnalyticContinuations(voltageAnalyticContinuation);
            Vector<Complex> currentVoltage;
            bool precisionReached = false;
            bool precisionReachedPrevious;
            var targetPrecisionScaled = _targetPrecision*nominalVoltage;

            do
            {
                CalculateNextCoefficientForVoltagePowerSeries(constantCurrents, knownPowers, factorization);
                voltageAnalyticContinuation = CreateVoltageAnalyticContinuation();
                currentVoltage = CalculateVoltagesWithAnalyticContinuations(voltageAnalyticContinuation);
                precisionReachedPrevious = precisionReached;
                CheckConvergence(currentVoltage, lastVoltage, targetPrecisionScaled, out precisionReached, out voltageCollapse);
                lastVoltage = currentVoltage;
            } while (_coefficients.Count < _maximumNumberOfCoefficients && !voltageCollapse && !(precisionReached && precisionReachedPrevious));
            
            return currentVoltage;
        }

        private void CheckConvergence(Vector<Complex> currentVoltage, Vector<Complex> lastVoltage, double targetPrecisionScaled, out bool precisionReached, out bool voltageCollapse)
        {
            var voltageChange = currentVoltage.Subtract(lastVoltage);
            var maximumVoltageChange = voltageChange.AbsoluteMaximum();
            voltageCollapse = false;

            if (maximumVoltageChange.Magnitude < targetPrecisionScaled)
                precisionReached = true;
            else
            {
                precisionReached = false;

                var lastCoefficient = _coefficients[_coefficients.Count - 1];
                var maximumLastCoefficient = lastCoefficient.AbsoluteMaximum();
                if (Math.Log10(maximumLastCoefficient.Magnitude/targetPrecisionScaled) > 30)
                    voltageCollapse = true;
            }

            for (var i = 0; i < _voltageConverged.Count(); ++i)
                if (!_voltageConverged[i])
                {
                    if (voltageChange[i].Magnitude < targetPrecisionScaled/100)
                        _voltageConverged[i] = true;
                }
        }

        public void CalculateNextCoefficientForVoltagePowerSeries(Vector<Complex> constantCurrents, Vector<Complex> knownPowers, ISolver<Complex> factorization)
        {
            var newCoefficient = CalculateNextCoefficient(_inverseCoefficients[_inverseCoefficients.Count - 1], factorization,
                knownPowers);
            _coefficients.Add(newCoefficient);
            var newInverseCoefficient = CalculateNextInverseCoefficient(_coefficients, _inverseCoefficients);
            _inverseCoefficients.Add(newInverseCoefficient);

            var coefficientIndex = _coefficients.Count - 1;
            for (var i = 0; i < _voltagePowerSeries.Length; ++i)
                if (!_voltageConverged[i])
                    _voltagePowerSeries[i][coefficientIndex] = _coefficients[coefficientIndex][i];
        }

        private IAnalyticContinuation<Complex>[] CreateVoltageAnalyticContinuation()
        {
            var nodeCount = _voltagePowerSeries.Count();
            var analyticContinuations = new IAnalyticContinuation<Complex>[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                analyticContinuations[i] = new EpsilonAlgorithm<Complex>(_voltagePowerSeries[i]);

            return analyticContinuations;
        }

        private static Vector<Complex> CalculateVoltagesWithAnalyticContinuations(IList<IAnalyticContinuation<Complex>> padeApproximants)
        {
            var nodeCount = padeApproximants.Count();
            var voltages = new Complex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                voltages[i] = padeApproximants[i].EvaluateAt1();

            return new DenseVector(voltages);
        }

        private void CreateVoltagePowerSeriesFromCoefficients()
        {
            var coefficientCount = _coefficients.Count;
            var nodeCount = _coefficients[0].Count;
            
            for (var i = 0; i < coefficientCount; ++i)
            {
                var coefficient = _coefficients[i];

                for (var j = 0; j < nodeCount; ++j)
                    _voltagePowerSeries[j][i] = coefficient[j];
            }
        }

        private void InitializeAlgorithm(Matrix<Complex> admittances, Vector<Complex> constantCurrents, Vector<Complex> knownPowers, ISolver<Complex> factorization)
        {
            var nodeCount = admittances.RowCount;
            _coefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            _inverseCoefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            _voltagePowerSeries = new PowerSeriesComplex[nodeCount];
            _voltageConverged = new bool[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                _voltagePowerSeries[i] = new PowerSeriesComplex(_maximumNumberOfCoefficients);

            for (var i = 0; i < nodeCount; ++i)
                _voltageConverged[i] = false;

            var admittanceRowSum = CalculateAdmittanceRowSum(admittances);
            var firstCoefficient = CalculateFirstCoefficient(factorization, admittances, admittanceRowSum);
            Vector<Complex> firstInverseCoefficient = new DenseVector(nodeCount);
            firstCoefficient.DivideByThis(new Complex(1, 0), firstInverseCoefficient);
            _coefficients.Add(firstCoefficient);
            _inverseCoefficients.Add(firstInverseCoefficient);
            var secondCoefficient = CalculateSecondCoefficient(firstInverseCoefficient, factorization, knownPowers,
                constantCurrents, admittanceRowSum);
            _coefficients.Add(secondCoefficient);
            var secondInverseCoefficient = CalculateNextInverseCoefficient(_coefficients, _inverseCoefficients);
            _inverseCoefficients.Add(secondInverseCoefficient);
            CreateVoltagePowerSeriesFromCoefficients();
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

        public PowerSeriesComplex[] VoltagePowerSeries
        {
            get { return _voltagePowerSeries; }
        }
    }
}