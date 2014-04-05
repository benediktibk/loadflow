using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators
{
    public class HolomorphicEmbeddedLoadFlowMethod : INodeVoltageCalculator, IDisposable
    {
        private delegate void StringCallback(string text);

        private readonly double _targetPrecision;
        private readonly int _numberOfCoefficients;
        private readonly StringCallback _stringCallback;
        private readonly Precision _precision;
        private readonly bool _calculatePartialResults;
        private int _calculator;
        private bool _disposed;

        public HolomorphicEmbeddedLoadFlowMethod(double targetPrecision, int numberOfCoefficients, Precision precision, bool calculatePartialResults)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            _numberOfCoefficients = numberOfCoefficients;
            _targetPrecision = targetPrecision;
            _stringCallback += DebugOutput;
            _precision = precision;
            _calculator = -1;
            _calculatePartialResults = calculatePartialResults;
            _disposed = false;
        }

        ~HolomorphicEmbeddedLoadFlowMethod()
        {
            DisposeInternal();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if (_disposed)
                return;

            if (_calculator >= 0)
                DeleteLoadFlowCalculator(_calculator);
            _calculator = -1;

            _disposed = true;
        }

        private static void DebugOutput(string text)
        {
            Debug.WriteLine(text);
        }

        public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            if (_calculator >= 0)
                DeleteLoadFlowCalculator(_calculator);

            var nodeCount = admittances.RowCount;

            switch (_precision.Type)
            {
                case DataType.LongDouble:
                    _calculator = CreateLoadFlowCalculatorLongDouble(_targetPrecision * nominalVoltage, _numberOfCoefficients, nodeCount,
                        pqBuses.Count, pvBuses.Count, nominalVoltage, _calculatePartialResults);
                    break;
                case DataType.MultiPrecision:
                    _calculator = CreateLoadFlowCalculatorMultiPrecision(_targetPrecision * nominalVoltage, _numberOfCoefficients, nodeCount,
                        pqBuses.Count, pvBuses.Count, nominalVoltage, _precision.BitPrecision, _calculatePartialResults);
                    break;
            }
            
            SetConsoleOutput(_calculator, _stringCallback);

            if (_calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            for (var row = 0; row < admittances.RowCount; ++row)
            {
                for (var column = 0; column < admittances.ColumnCount; ++column)
                {
                    var admittance = admittances[row, column];

                    if (admittance == new Complex())
                        continue;

                    SetAdmittance(_calculator, row, column, admittance.Real, admittance.Imaginary);
                }

                var totalRowSum = totalAdmittanceRowSums[row];
                SetAdmittanceRowSum(_calculator, row, totalRowSum.Real, totalRowSum.Imaginary);
            }

            for (var i = 0; i < nodeCount; ++i)
                SetConstantCurrent(_calculator, i, constantCurrents[i].Real, constantCurrents[i].Imaginary);

            for (var i = 0; i < pqBuses.Count; ++i)
                SetPQBus(_calculator, i, pqBuses[i].ID, pqBuses[i].Power.Real, pqBuses[i].Power.Imaginary);

            for (var i = 0; i < pvBuses.Count; ++i)
                SetPVBus(_calculator, i, pvBuses[i].ID, pvBuses[i].RealPower, pvBuses[i].VoltageMagnitude);

            Calculate(_calculator);

            var voltages = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var real = GetVoltageReal(_calculator, i);
                var imaginary = GetVoltageImaginary(_calculator, i);
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
            var nodeCount = GetLastNodeCount(_calculator);
            var result = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new Complex(GetCoefficientReal(_calculator, step, i),
                    GetCoefficientImaginary(_calculator, step, i));

            return result;
        }

        public Vector<Complex> GetInverseCoefficients(int step)
        {
            Debug.Assert(_calculator >= 0);
            var nodeCount = GetLastNodeCount(_calculator);
            var result = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new Complex(GetInverseCoefficientReal(_calculator, step, i),
                    GetInverseCoefficientImaginary(_calculator, step, i));

            return result;
        }

        #region dll imports
        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateLoadFlowCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, [MarshalAs(UnmanagedType.I1)]bool calculatePartialResults);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateLoadFlowCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision, [MarshalAs(UnmanagedType.I1)]bool calculatePartialResults);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteLoadFlowCalculator(int calculator);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAdmittance(int calculator, int row, int column, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAdmittanceRowSum(int calculator, int row, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetConstantCurrent(int calculator, int node, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Calculate(int calculator);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageReal(int calculator, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageImaginary(int calculator, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double SetConsoleOutput(int calculator, StringCallback function);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetCoefficientReal(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetCoefficientImaginary(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetInverseCoefficientReal(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetInverseCoefficientImaginary(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetLastNodeCount(int calculator);
        #endregion
    }
}
