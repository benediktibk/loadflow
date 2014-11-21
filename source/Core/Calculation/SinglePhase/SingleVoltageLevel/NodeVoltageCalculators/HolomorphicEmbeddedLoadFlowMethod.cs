using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethod : INodeVoltageCalculator
    {

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int numberOfCoefficients, int bitPrecision)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            NumberOfCoefficients = numberOfCoefficients;
            TargetPrecision = targetPrecision;
            BitPrecision = bitPrecision;
        }

        public double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        public double TargetPrecision { get; private set; }
        public int NumberOfCoefficients { get; private set; }
        public int BitPrecision { get; private set; }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var calculator = CalculateUnknownVoltagesInternal(admittances, totalAdmittanceRowSums, nominalVoltage, constantCurrents, pqBuses, pvBuses);
            var voltages = FetchVoltages(admittances.NodeCount, calculator);
            HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(calculator);
            return voltages;
        }

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses, out IList<Vector<Complex>> coefficients, int stepCount)
        {
            var calculator = CalculateUnknownVoltagesInternal(admittances, totalAdmittanceRowSums, nominalVoltage, constantCurrents, pqBuses, pvBuses);
            coefficients = new List<Vector<Complex>>(NumberOfCoefficients);

            for (var i = 0; i < stepCount; ++i)
                coefficients.Add(FetchCoefficients(i, admittances.NodeCount, calculator));

            var voltages = FetchVoltages(admittances.NodeCount, calculator);
            HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(calculator);
            return voltages;
        }

        private int CalculateUnknownVoltagesInternal(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums,
            double nominalVoltage, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var nodeCount = admittances.NodeCount;
            var calculator = CreateNewCalculator(nominalVoltage, pqBuses, pvBuses, nodeCount);
            SetAdmittanceValues(admittances, totalAdmittanceRowSums, calculator);
            SetRightHandSideValues(constantCurrents, pqBuses, pvBuses, nodeCount, calculator);

            HolomorphicEmbeddedLoadFlowMethodNativeMethods.Calculate(calculator);
            return calculator;
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

        private void SetRightHandSideValues(Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses, int nodeCount, int calculator)
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

        private void SetAdmittanceValues(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, int calculator)
        {
            for (var row = 0; row < admittances.NodeCount; ++row)
            {
                for (var column = 0; column < admittances.NodeCount; ++column)
                {
                    var admittance = admittances[row, column];

                    if (admittance == new Complex())
                        continue;

                    HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetAdmittance(calculator, row, column, admittance.Real,
                        admittance.Imaginary);
                }

                var totalRowSum = totalAdmittanceRowSums[row];
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
                            pqBuses.Count, pvBuses.Count, nominalVoltage);
            else
                calculator =
                        HolomorphicEmbeddedLoadFlowMethodNativeMethods.CreateLoadFlowCalculatorMultiPrecision(
                            TargetPrecision * nominalVoltage, NumberOfCoefficients, nodeCount,
                            pqBuses.Count, pvBuses.Count, nominalVoltage, BitPrecision);

            if (calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            return calculator;
        }
    }
}
