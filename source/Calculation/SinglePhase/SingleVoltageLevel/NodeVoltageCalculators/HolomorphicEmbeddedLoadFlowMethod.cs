using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethod : INodeVoltageCalculator, IDisposable
    {
        #region variables

        private readonly double _targetPrecision;
        private readonly int _numberOfCoefficients;
        private readonly Precision _precision;
        private int _calculator;
        private bool _disposed;

        #endregion

        #region constructor/destructor

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int numberOfCoefficients, Precision precision)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            _numberOfCoefficients = numberOfCoefficients;
            _targetPrecision = targetPrecision;
            _precision = precision;
            _calculator = -1;
            _disposed = false;
        }

        ~HolomorphicEmbeddedLoadFlowMethod()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (_disposed)
                return;

            if (_calculator >= 0)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(_calculator);
            _calculator = -1;

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region public functions

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            if (_calculator >= 0)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.DeleteLoadFlowCalculator(_calculator);

            var nodeCount = admittances.NodeCount;

            switch (_precision.Type)
            {
                case DataType.LongDouble:
                    _calculator = HolomorphicEmbeddedLoadFlowMethodNativeMethods.CreateLoadFlowCalculatorLongDouble(_targetPrecision * nominalVoltage, _numberOfCoefficients, nodeCount,
                        pqBuses.Count, pvBuses.Count, nominalVoltage);
                    break;
                case DataType.MultiPrecision:
                    _calculator = HolomorphicEmbeddedLoadFlowMethodNativeMethods.CreateLoadFlowCalculatorMultiPrecision(_targetPrecision * nominalVoltage, _numberOfCoefficients, nodeCount,
                        pqBuses.Count, pvBuses.Count, nominalVoltage, _precision.BitPrecision);
                    break;
            }

            if (_calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            for (var row = 0; row < admittances.NodeCount; ++row)
            {
                for (var column = 0; column < admittances.NodeCount; ++column)
                {
                    var admittance = admittances[row, column];

                    if (admittance == new Complex())
                        continue;

                    HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetAdmittance(_calculator, row, column, admittance.Real, admittance.Imaginary);
                }

                var totalRowSum = totalAdmittanceRowSums[row];
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetAdmittanceRowSum(_calculator, row, totalRowSum.Real, totalRowSum.Imaginary);
            }

            for (var i = 0; i < nodeCount; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetConstantCurrent(_calculator, i, constantCurrents[i].Real, constantCurrents[i].Imaginary);

            for (var i = 0; i < pqBuses.Count; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetPQBus(_calculator, i, pqBuses[i].ID, pqBuses[i].Power.Real, pqBuses[i].Power.Imaginary);

            for (var i = 0; i < pvBuses.Count; ++i)
                HolomorphicEmbeddedLoadFlowMethodNativeMethods.SetPVBus(_calculator, i, pvBuses[i].ID, pvBuses[i].RealPower, pvBuses[i].VoltageMagnitude);

            HolomorphicEmbeddedLoadFlowMethodNativeMethods.Calculate(_calculator);

            var voltages = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var real = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetVoltageReal(_calculator, i);
                var imaginary = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetVoltageImaginary(_calculator, i);
                voltages[i] = new Complex(real, imaginary);
            }

            return voltages;
        }

        public double GetMaximumPowerError()
        {
            return 0.1;
        }

        public Vector<Complex> GetCoefficients(int step)
        {
            Debug.Assert(_calculator >= 0);
            var nodeCount = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetLastNodeCount(_calculator);
            var result = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new Complex(HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetCoefficientReal(_calculator, step, i), HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetCoefficientImaginary(_calculator, step, i));

            return result;
        }

        public Vector<Complex> GetInverseCoefficients(int step)
        {
            Debug.Assert(_calculator >= 0);
            var nodeCount = HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetLastNodeCount(_calculator);
            var result = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new Complex(HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetInverseCoefficientReal(_calculator, step, i), HolomorphicEmbeddedLoadFlowMethodNativeMethods.GetInverseCoefficientImaginary(_calculator, step, i));

            return result;
        }

        #endregion
    }
}
