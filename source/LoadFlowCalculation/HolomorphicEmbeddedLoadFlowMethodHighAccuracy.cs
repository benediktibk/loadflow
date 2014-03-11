using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethodHighAccuracy : LoadFlowCalculator
    {
        private readonly double _targetPrecision;
        private readonly int _numberOfCoefficients;

        private delegate void StringCallback(string text);

        private StringCallback _stringCallback;

        public HolomorphicEmbeddedLoadFlowMethodHighAccuracy(double targetPrecision, int numberOfCoefficients)
            : base(targetPrecision * 10000)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            _numberOfCoefficients = numberOfCoefficients;
            _targetPrecision = targetPrecision;
            _stringCallback = Console.WriteLine;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses,
            IList<PVBus> pvBuses)
        {
            SetConsoleOutput(_stringCallback);

            var nodeCount = admittances.RowCount;
            var calculator = CreateLoadFlowCalculator(_targetPrecision * nominalVoltage / 10, _numberOfCoefficients, nodeCount,
                pqBuses.Count, pvBuses.Count);

            if (calculator < 0)
                throw new IndexOutOfRangeException("the handle to the calculator must be not-negative");

            for (var row = 0; row < admittances.RowCount; ++row)
                for (var column = 0; column < admittances.ColumnCount; ++column)
                {
                    var admittance = admittances[row, column];

                    if (admittance == new Complex()) 
                        continue;

                    SetAdmittanceReal(calculator, row, column, admittance.Real);
                    SetAdmittanceImaginary(calculator, row, column, admittance.Imaginary);
                }

            for (var i = 0; i < nodeCount; ++i)
            {
                var current = constantCurrents[i];
                SetConstantCurrentReal(calculator, i, current.Real);
                SetConstantCurrentImaginary(calculator, i, current.Imaginary);
            }

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var bus = pqBuses[i];
                SetPQBusPowerReal(calculator, i, bus.ID, bus.Power.Real);
                SetPQBusPowerImaginary(calculator, i, bus.ID, bus.Power.Imaginary);
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                var bus = pvBuses[i];
                SetPVBusPowerReal(calculator, i, bus.ID, bus.RealPower);
                SetPVBusVoltageMagnitude(calculator, i, bus.ID, bus.VoltageMagnitude);
            }

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
        private static extern void SetAdmittanceReal(int calculator, int row, int column, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetAdmittanceImaginary(int calculator, int row, int column, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPQBusPowerReal(int calculator, int busId, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPQBusPowerImaginary(int calculator, int busId, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPVBusPowerReal(int calculator, int busId, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetPVBusVoltageMagnitude(int calculator, int busId, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetConstantCurrentReal(int calculator, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetConstantCurrentImaginary(int calculator, int node, double value);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Calculate(int calculator);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageReal(int calculator, int node);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetVoltageImaginary(int calculator, int node);

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double SetConsoleOutput(StringCallback function);
        #endregion
    }
}
