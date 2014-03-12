using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethodHighAccuracy : LoadFlowCalculator
    {
        private delegate void StringCallback(string text);

        private readonly double _targetPrecision;
        private readonly int _numberOfCoefficients;
        private readonly StringCallback _stringCallback;

        public HolomorphicEmbeddedLoadFlowMethodHighAccuracy(double targetPrecision, int numberOfCoefficients)
            : base(targetPrecision * 10000)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            _numberOfCoefficients = numberOfCoefficients;
            _targetPrecision = targetPrecision;
            _stringCallback += DebugOutput;
        }

        private void DebugOutput(string text)
        {
            Debug.WriteLine(text);
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses,
            IList<PVBus> pvBuses)
        {
            var nodeCount = admittances.RowCount;
            var calculator = CreateLoadFlowCalculator(_targetPrecision * nominalVoltage / 10, _numberOfCoefficients, nodeCount,
                pqBuses.Count, pvBuses.Count);

            SetConsoleOutput(calculator, _stringCallback);

            if (calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            for (var row = 0; row < admittances.RowCount; ++row)
                for (var column = 0; column < admittances.ColumnCount; ++column)
                {
                    var admittance = admittances[row, column];

                    if (admittance == new Complex()) 
                        continue;

                    SetAdmittance(calculator, row, column, admittance.Real, admittance.Imaginary);
                }

            for (var i = 0; i < nodeCount; ++i)
                SetConstantCurrent(calculator, i, constantCurrents[i].Real, constantCurrents[i].Imaginary);

            for (var i = 0; i < pqBuses.Count; ++i)
                SetPQBus(calculator, i, pqBuses[i].ID, pqBuses[i].Power.Real, pqBuses[i].Power.Imaginary);

            for (var i = 0; i < pvBuses.Count; ++i)
                SetPVBus(calculator, i, pvBuses[i].ID, pvBuses[i].RealPower, pvBuses[i].VoltageMagnitude);

            Calculate(calculator);

            var voltages = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var real = GetVoltageReal(calculator, i);
                var imaginary = GetVoltageImaginary(calculator, i);
                voltages[i] = new Complex(real, imaginary);
            }

            DeleteLoadFlowCalculator(calculator);

            return voltages;
        }

        #region dll imports
        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateLoadFlowCalculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteLoadFlowCalculator(int calculator);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAdmittance(int calculator, int row, int column, double real, double imaginary);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetConstantCurrent(int calculator, int node, double real, double imaginary);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Calculate(int calculator);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageReal(int calculator, int node);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageImaginary(int calculator, int node);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double SetConsoleOutput(int calculator, StringCallback function);
        #endregion
    }
}
