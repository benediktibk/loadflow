using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethod : INodeVoltageCalculator
    {
        private int _calculator = -1;
        private readonly Mutex _calculatorMutex;
        private int _maximumPossibleCoefficientCount;

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int numberOfCoefficients, int bitPrecision, bool iterativeSolver)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            NumberOfCoefficients = numberOfCoefficients;
            TargetPrecision = targetPrecision;
            BitPrecision = bitPrecision;
            IterativeSolver = iterativeSolver;
            _calculatorMutex = new Mutex();
            _maximumPossibleCoefficientCount = -1;
        }

        public double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        public double Progress
        {
            get
            {
                _calculatorMutex.WaitOne();
                var result = _calculator < 0 ? 0 : HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetProgress(_calculator);
                _calculatorMutex.ReleaseMutex();
                return result;
            }
        }

        public double RelativePowerError
        {
            get
            {
                _calculatorMutex.WaitOne();
                var result = _calculator < 0 ? 0 : HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetRelativePowerError(_calculator);
                _calculatorMutex.ReleaseMutex();
                return result;
            }
        }

        public string StatusMessage
        {
            get
            {
                var result = "";

                if (_maximumPossibleCoefficientCount >= 0)
                    result = "could only calculate " + _maximumPossibleCoefficientCount +
                                " coefficients because of numerical issues";

                return result;
            }
        }

        public void ResetProgress()
        { }

        public double TargetPrecision { get; private set; }
        public int NumberOfCoefficients { get; private set; }
        public int BitPrecision { get; private set; }
        public bool IterativeSolver { get; private set; }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            CalculateUnknownVoltagesInternal(admittances, totalAdmittanceRowSums, nominalVoltage, constantCurrents, pqBuses, pvBuses);
            var voltages = FetchVoltages(admittances.NodeCount, _calculator);
            _calculatorMutex.WaitOne();
            HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(_calculator);
            _calculator = -1;
            _calculatorMutex.ReleaseMutex();
            return voltages;
        }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses, out IList<Vector<Complex>> coefficients, int stepCount)
        {
            CalculateUnknownVoltagesInternal(admittances, totalAdmittanceRowSums, nominalVoltage, constantCurrents, pqBuses, pvBuses);
            coefficients = new List<Vector<Complex>>(NumberOfCoefficients);

            for (var i = 0; i < stepCount; ++i)
                coefficients.Add(FetchCoefficients(i, admittances.NodeCount, _calculator));

            var voltages = FetchVoltages(admittances.NodeCount, _calculator);
            _calculatorMutex.WaitOne();
            HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(_calculator);
            _calculator = -1;
            _calculatorMutex.ReleaseMutex();
            return voltages;
        }

        private void CalculateUnknownVoltagesInternal(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums,
            double nominalVoltage, IList<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var nodeCount = admittances.NodeCount;
            _calculatorMutex.WaitOne();
            _calculator = CreateNewCalculator(nominalVoltage, pqBuses, pvBuses, nodeCount);
            _calculatorMutex.ReleaseMutex();
            SetAdmittanceValues(admittances, totalAdmittanceRowSums, _calculator);
            SetRightHandSideValues(constantCurrents, pqBuses, pvBuses, nodeCount, _calculator);
            HolomorphicEmbeddedLoadFlowMethodNativeMethods.Calculate(_calculator);
            _maximumPossibleCoefficientCount =
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetMaximumPossibleCoefficientCount(_calculator);
        }

        private Vector<Complex> FetchCoefficients(int step, int nodeCount, int calculator)
        {
            var result = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new Complex(HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetCoefficientReal(calculator, step, i), HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetCoefficientImaginary(calculator, step, i));

            return result;
        }

        private Vector<Complex> FetchVoltages(int nodeCount, int calculator)
        {
            var voltages = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var real = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetVoltageReal(calculator, i);
                var imaginary = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetVoltageImaginary(calculator, i);
                voltages[i] = new Complex(real, imaginary);
            }

            return voltages;
        }

        private static void SetRightHandSideValues(IList<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses, int nodeCount, int calculator)
        {
            for (var i = 0; i < nodeCount; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetConstantCurrent(calculator, i, constantCurrents[i].Real,
                    constantCurrents[i].Imaginary);

            for (var i = 0; i < pqBuses.Count; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetPQBus(calculator, i, pqBuses[i].Index, pqBuses[i].Power.Real,
                    pqBuses[i].Power.Imaginary);

            for (var i = 0; i < pvBuses.Count; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetPVBus(calculator, i, pvBuses[i].Index, pvBuses[i].RealPower,
                    pvBuses[i].VoltageMagnitude);
        }

        private static void SetAdmittanceValues(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, int calculator)
        {
            foreach (var entry in admittances.EnumerateIndexed())
            {
                var value = entry.Item3;
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetAdmittance(calculator, entry.Item1, entry.Item2, value.Real,
                    value.Imaginary);
            }

            var maximumMagnitude = admittances.FindMaximumMagnitude();

            for (var row = 0; row < admittances.NodeCount; ++row)
            {
                var totalRowSum = totalAdmittanceRowSums[row];

                if (totalRowSum.Magnitude < maximumMagnitude*1e-15)
                    totalRowSum = 0;

                /*! 
                 * This modificiation increases the stability of the iterative linear equation solver 
                 * and has no further effect on the result. In fact, this value can be chosen arbitrarly.
                 */
                totalRowSum += new Complex(row, row + 1 - admittances.NodeCount) / admittances.NodeCount * maximumMagnitude * 1e-2;

                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetAdmittanceRowSum(calculator, row, totalRowSum.Real,
                    totalRowSum.Imaginary);
            }
        }

        private int CreateNewCalculator(double nominalVoltage, ICollection<PqNodeWithIndex> pqBuses, ICollection<PvNodeWithIndex> pvBuses, int nodeCount)
        {
            int calculator;

            if (BitPrecision <= 64)
                calculator =
                        HolomorphicEmbeddedLoadFlowMethodNativeMethods.CreateLoadFlowCalculatorLongDouble(
                            TargetPrecision * nominalVoltage, NumberOfCoefficients, nodeCount,
                            pqBuses.Count, pvBuses.Count, nominalVoltage, IterativeSolver);
            else
                calculator =
                        HolomorphicEmbeddedLoadFlowMethodNativeMethods.CreateLoadFlowCalculatorMultiPrecision(
                            TargetPrecision * nominalVoltage, NumberOfCoefficients, nodeCount,
                            pqBuses.Count, pvBuses.Count, nominalVoltage, BitPrecision, IterativeSolver);

            if (calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            return calculator;
        }
    }
}
