using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethod : LoadFlowCalculator
    {
        private readonly double _targetPrecision;
        private readonly int _maximumNumberOfCoefficients;
        private List<Vector<Complex>> _coefficients;
        private List<Vector<Complex>> _inverseCoefficients;
        private PowerSeriesComplex[] _voltagePowerSeries;
        private readonly bool _finalAccuarcyImprovement;

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int maximumNumberOfCoefficients, bool finalAccuracyImprovement)
            : base(finalAccuracyImprovement ? targetPrecision * 10000 : targetPrecision * 1000000)
        {
            if (maximumNumberOfCoefficients < 4)
                throw new ArgumentOutOfRangeException("maximumNumberOfCoefficients",
                    "there must be at least 4 coefficients");
            
            _targetPrecision = targetPrecision;
            _maximumNumberOfCoefficients = maximumNumberOfCoefficients;
            _finalAccuarcyImprovement = finalAccuracyImprovement;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var factorization = admittances.QR();
            InitializeAlgorithm(admittances, constantCurrents, factorization, pqBuses, pvBuses);
            var voltageAnalyticContinuation = CreateVoltageAnalyticContinuation();
            var lastVoltage = CalculateVoltagesWithAnalyticContinuations(voltageAnalyticContinuation);
            Vector<Complex> currentVoltage;
            var precisionReached = false;
            bool precisionReachedPrevious;
            var targetPrecisionScaled = _targetPrecision*nominalVoltage/10;

            do
            {
                CalculateNextCoefficientForVoltagePowerSeries(constantCurrents, factorization, pqBuses, pvBuses);
                voltageAnalyticContinuation = CreateVoltageAnalyticContinuation();
                currentVoltage = CalculateVoltagesWithAnalyticContinuations(voltageAnalyticContinuation);
                precisionReachedPrevious = precisionReached;
                precisionReached = CheckConvergence(currentVoltage, lastVoltage, targetPrecisionScaled);
                lastVoltage = currentVoltage;
            } while (_coefficients.Count < _maximumNumberOfCoefficients && !(precisionReached && precisionReachedPrevious));

            if (!_finalAccuarcyImprovement) 
                return currentVoltage;

            var currentIteration = new CurrentIteration(_targetPrecision/100, 1000);

            var improvedVoltage = currentIteration.CalculateUnknownVoltages(admittances, nominalVoltage,
                constantCurrents, pqBuses, pvBuses, currentVoltage);

            var voltageImprovement = improvedVoltage - currentVoltage;
            var maximumVoltageImprovement = voltageImprovement.AbsoluteMaximum().Magnitude;

            if (maximumVoltageImprovement < 0.1*nominalVoltage)
                currentVoltage = improvedVoltage;

            return currentVoltage;
        }

        private bool CheckConvergence(Vector<Complex> currentVoltage, Vector<Complex> lastVoltage, double targetPrecisionScaled)
        {
            var voltageChange = currentVoltage.Subtract(lastVoltage);
            var maximumVoltageChange = voltageChange.AbsoluteMaximum();
            return maximumVoltageChange.Magnitude < targetPrecisionScaled;
        }

        public void CalculateNextCoefficientForVoltagePowerSeries(Vector<Complex> constantCurrents, ISolver<Complex> factorization, ICollection<PQBus> pqBuses, ICollection<PVBus> pvBuses)
        {
            var newCoefficient = CalculateNextCoefficient(_inverseCoefficients[_inverseCoefficients.Count - 1], _coefficients[_coefficients.Count - 1], factorization,
                pqBuses, pvBuses);
            _coefficients.Add(newCoefficient);
            var newInverseCoefficient = CalculateNextInverseCoefficient(_coefficients, _inverseCoefficients);
            _inverseCoefficients.Add(newInverseCoefficient);

            var coefficientIndex = _coefficients.Count - 1;
            for (var i = 0; i < _voltagePowerSeries.Length; ++i)
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

        private static Vector<Complex> CalculateVoltagesWithAnalyticContinuations(IList<IAnalyticContinuation<Complex>> analyticContinuations)
        {
            var nodeCount = analyticContinuations.Count();
            var voltages = new Complex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                voltages[i] = analyticContinuations[i].EvaluateAt1();

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

        private void InitializeAlgorithm(Matrix<Complex> admittances, IList<Complex> constantCurrents, ISolver<Complex> factorization, ICollection<PQBus> pqBuses, ICollection<PVBus> pvBuses)
        {
            var nodeCount = admittances.RowCount;
            _coefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            _inverseCoefficients = new List<Vector<Complex>>(_maximumNumberOfCoefficients);
            _voltagePowerSeries = new PowerSeriesComplex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                _voltagePowerSeries[i] = new PowerSeriesComplex(_maximumNumberOfCoefficients);

            var admittanceRowSum = CalculateAdmittanceRowSum(admittances);
            var firstCoefficient = CalculateFirstCoefficient(factorization, admittanceRowSum, pqBuses, pvBuses, constantCurrents);
            Vector<Complex> firstInverseCoefficient = new DenseVector(nodeCount);
            firstCoefficient.DivideByThis(new Complex(1, 0), firstInverseCoefficient);
            _coefficients.Add(firstCoefficient);
            _inverseCoefficients.Add(firstInverseCoefficient);
            var secondCoefficient = CalculateSecondCoefficient(firstInverseCoefficient, firstCoefficient, factorization,
                constantCurrents, admittanceRowSum, pqBuses, pvBuses);
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

        private static Vector<Complex> CalculateFirstCoefficient(ISolver<Complex> factorization, IList<Complex> admittanceRowSum, ICollection<PQBus> pqBuses, ICollection<PVBus> pvBuses, IList<Complex> constantCurrents)
        {
            var rightHandSide = new DenseVector(pqBuses.Count + pvBuses.Count);

            foreach (var bus in pqBuses)
                rightHandSide[bus.ID] = admittanceRowSum[bus.ID]*(-1);

            foreach (var bus in pvBuses)
                rightHandSide[bus.ID] = admittanceRowSum[bus.ID] + constantCurrents[bus.ID];

            return factorization.Solve(rightHandSide);
        }

        private static Vector<Complex> CalculateSecondCoefficient(IList<Complex> previousInverseCoefficients, IList<Complex> previousCoefficients,
            ISolver<Complex> factorization, IList<Complex> constantCurrents,
            IList<Complex> admittanceRowSums, ICollection<PQBus> pqBuses, ICollection<PVBus> pvBuses)
        {
            var rightHandSide = new DenseVector(pqBuses.Count + pvBuses.Count);

            foreach (var bus in pqBuses)
            {
                var ownCurrent = bus.Power*previousInverseCoefficients[bus.ID];
                var constantCurrent = constantCurrents[bus.ID];
                var totalCurrent = ownCurrent.Conjugate() + constantCurrent;
                rightHandSide[bus.ID] = admittanceRowSums[bus.ID] + totalCurrent;
            }

            foreach (var bus in pvBuses)
            {
                var power = bus.RealPower;
                var previousCoefficient = previousCoefficients[bus.ID];
                var previousInverseCoefficient = previousInverseCoefficients[bus.ID];
                var admittanceRowSum = admittanceRowSums[bus.ID];
                var magnitudeSquare = bus.VoltageMagnitude*bus.VoltageMagnitude;
                rightHandSide[bus.ID] = (2*power*previousCoefficient - previousInverseCoefficient)/magnitudeSquare -
                                        admittanceRowSum;
            }

            return factorization.Solve(rightHandSide);
        }

        private static Vector<Complex> CalculateNextCoefficient(IList<Complex> previousInverseCoefficients, IList<Complex> previousCoefficients, ISolver<Complex> factorization, ICollection<PQBus> pqBuses, ICollection<PVBus> pvBuses)
        {
            var rightHandSide = new DenseVector(pqBuses.Count + pvBuses.Count);

            foreach (var bus in pqBuses)
                rightHandSide[bus.ID] = (bus.Power * previousInverseCoefficients[bus.ID]).Conjugate();

            foreach (var bus in pvBuses)
            {
                var power = bus.RealPower;
                var previousCoefficient = previousCoefficients[bus.ID];
                var previousInverseCoefficient = previousInverseCoefficients[bus.ID];
                var magnitudeSquare = bus.VoltageMagnitude * bus.VoltageMagnitude;
                rightHandSide[bus.ID] = (2 * power * previousCoefficient - previousInverseCoefficient) / magnitudeSquare;
            }

            return factorization.Solve(rightHandSide);
        }

        private static Vector<Complex> CalculateNextInverseCoefficient(IReadOnlyList<Vector<Complex>> coefficients,
            IReadOnlyList<Vector<Complex>> inverseCoefficients)
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