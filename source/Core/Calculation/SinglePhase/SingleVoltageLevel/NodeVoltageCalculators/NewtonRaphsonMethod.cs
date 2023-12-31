﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        private SparseMatrixStorage _changeMatrixStorage;

        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations, bool iterativeSolver)
            : base(targetPrecision, maximumIterations)
        {
            IterativeSolver = iterativeSolver;
            _changeMatrixStorage = null;
        }

        public bool IterativeSolver { get; private set; }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<double> pvBusVoltages, double residualImprovementFactor, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex, IReadOnlyDictionary<int, int> busToMatrixIndex)
        {
            var changeMatrix = CalculateChangeMatrix(admittances, voltages, constantCurrents, pqBusToMatrixIndex, pvBusToMatrixIndex, busToMatrixIndex);
            var voltageChanges = CalculateVoltageChanges(powersRealError, powersImaginaryError, changeMatrix, residualImprovementFactor, IterativeSolver);
            return CalculateImprovedVoltagesFromVoltageChanges(voltages, pqBusToMatrixIndex, pvBusToMatrixIndex, pvBusVoltages, voltageChanges);
        }

        public override void InitializeMatrixStorage(int pqBusCount, int pvBusCount)
        {
            _changeMatrixStorage = new SparseMatrixStorage(pqBusCount * 2 + pvBusCount);
        }

        public static DenseVector CalculateImprovedVoltagesFromVoltageChanges(IList<Complex> voltages, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex,
            IList<double> pvBusVoltages, IList<double> voltageChanges)
        {
            IList<double> voltageChangesReal;
            IList<double> voltageChangesImaginary;
            IList<double> voltageChangesAngle;
            DivideParts(voltageChanges, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count, out voltageChangesReal, out voltageChangesImaginary,
                out voltageChangesAngle);
            var improvedVoltages = new DenseVector(pqBusToMatrixIndex.Count + pvBusToMatrixIndex.Count);

            foreach (var bus in pqBusToMatrixIndex)
                improvedVoltages[bus.Key] = voltages[bus.Key] + new Complex(voltageChangesReal[bus.Value], voltageChangesImaginary[bus.Value]);

            foreach (var bus in pvBusToMatrixIndex)
                improvedVoltages[bus.Key] = new Complex(pvBusVoltages[bus.Value], voltages[bus.Key].Phase + voltageChangesAngle[bus.Value]);

            return improvedVoltages;
        }

        public static Vector<double> CalculateVoltageChanges(IList<double> powersRealError, IList<double> powersImaginaryError, Matrix<double> changeMatrix, double residualImprovementFactor, bool iterativeSolver)
        {
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            Vector<double> voltageChanges = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(rightSide.Count);
            var stopCriterion = CreateStopCriterion(powersRealError, powersImaginaryError, residualImprovementFactor);

            if (iterativeSolver)
            {
                var solver = new TFQMR();
                solver.Solve(changeMatrix, rightSide, voltageChanges, new Iterator<double>(stopCriterion),
                    new ILU0Preconditioner());
            }
            else
            {
                var factorization = changeMatrix.LU();
                voltageChanges = factorization.Solve(rightSide);
            }

            return voltageChanges;
        }

        public Matrix<double> CalculateChangeMatrix(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
            IReadOnlyDictionary<int, int> pqBusToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex, IReadOnlyDictionary<int, int> busToMatrixIndex)
        {
            _changeMatrixStorage.Reset();
            var realPowerByRealPart = new SubMatrix(_changeMatrixStorage, 0, 0, busToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var realPowerByImaginaryPart = new SubMatrix(_changeMatrixStorage, 0, pqBusToMatrixIndex.Count, busToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var imaginaryPowerByRealPart = new SubMatrix(_changeMatrixStorage, busToMatrixIndex.Count, 0, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var imaginaryPowerByImaginaryPart = new SubMatrix(_changeMatrixStorage, busToMatrixIndex.Count, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var realPowerByAngle = new SubMatrix(_changeMatrixStorage, 0, 2 * pqBusToMatrixIndex.Count, busToMatrixIndex.Count,
                pvBusToMatrixIndex.Count);
            var imaginaryPowerByAngle = new SubMatrix(_changeMatrixStorage, busToMatrixIndex.Count, 2 * pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count,
                pvBusToMatrixIndex.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                var admittance = entry.Item3;
                FillChangeMatrixRealPowerByRealPart(realPowerByRealPart, voltages, busToMatrixIndex, pqBusToMatrixIndex, busRow,
                    busColumn, admittance);
                FillChangeMatrixRealPowerByImaginaryPart(realPowerByImaginaryPart, voltages, busToMatrixIndex, pqBusToMatrixIndex, busRow,
                    busColumn, admittance);
                FillChangeMatrixImaginaryPowerByRealPart(imaginaryPowerByRealPart, voltages, pqBusToMatrixIndex, busRow, busColumn,
                    admittance);
                FillChangeMatrixImaginaryPowerByImaginaryPart(imaginaryPowerByImaginaryPart, voltages, pqBusToMatrixIndex, busRow, busColumn,
                    admittance);
                FillChangeMatrixRealPowerByAngle(realPowerByAngle, voltages, busToMatrixIndex, pvBusToMatrixIndex, busRow, busColumn, admittance);
                FillChangeMatrixImaginaryPowerByAngle(imaginaryPowerByAngle, voltages, pqBusToMatrixIndex, pvBusToMatrixIndex, busRow, busColumn,
                    admittance);
            }

            foreach (var bus in pqBusToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var current = constantCurrents[busIndex];
                realPowerByRealPart[matrixIndex, matrixIndex] -= current.Real;
                realPowerByImaginaryPart[matrixIndex, matrixIndex] -= current.Imaginary;
                imaginaryPowerByRealPart[matrixIndex, matrixIndex] += current.Imaginary;
                imaginaryPowerByImaginaryPart[matrixIndex, matrixIndex] -= current.Real;
            }

            foreach (var bus in pvBusToMatrixIndex)
            {
                var busIndex = bus.Key;
                var row = busToMatrixIndex[busIndex];
                var column = bus.Value;
                var voltage = voltages[busIndex];
                var current = constantCurrents[busIndex];
                realPowerByAngle[row, column] +=
                    voltage.Magnitude * current.Magnitude * Math.Sin(voltage.Phase - current.Phase);
            }

            return _changeMatrixStorage.ToMatrix();
        }

        private static void FillChangeMatrixRealPowerByRealPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            var matrixRow = busToMatrixIndex[busRow];
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                if (pqBusToMatrixIndex.ContainsKey(busRow)) 
                    changeMatrix[matrixRow, matrixRow] += admittance.Real * voltageColumn.Real -
                                                        admittance.Imaginary * voltageColumn.Imaginary;
                return;
            }

            if (busRow == busColumn)
            {
                changeMatrix[matrixRow, matrixRow] += 2 * admittance.Real * voltageRow.Real;
                return;
            }

            if (pqBusToMatrixIndex.ContainsKey(busRow))
                changeMatrix[matrixRow, matrixRow] += admittance.Real*voltageColumn.Real -
                                                    admittance.Imaginary*voltageColumn.Imaginary;
            changeMatrix[matrixRow, matrixColumn] = voltageRow.Real*admittance.Real +
                                                    voltageRow.Imaginary*admittance.Imaginary;
            
        }

        private static void FillChangeMatrixRealPowerByImaginaryPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = busToMatrixIndex[busRow];
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                if (pqBusToMatrixIndex.ContainsKey(busRow))
                    changeMatrix[matrixRow, matrixRow] += admittance.Imaginary * voltageColumn.Real +
                                                        admittance.Real * voltageColumn.Imaginary;
                return;
            }


            if (busRow == busColumn)
            {
                changeMatrix[matrixRow, matrixColumn] += 2*admittance.Real*voltageRow.Imaginary;
                return;
            }

            if (pqBusToMatrixIndex.ContainsKey(busRow))
                changeMatrix[matrixRow, matrixRow] += admittance.Imaginary*voltageColumn.Real +
                                                      admittance.Real*voltageColumn.Imaginary;
            changeMatrix[matrixRow, matrixColumn] = voltageRow.Imaginary*admittance.Real -
                                                    voltageRow.Real*admittance.Imaginary;
        }

        private static void FillChangeMatrixImaginaryPowerByRealPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            int matrixRow;
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                return;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                changeMatrix[matrixRow, matrixRow] -= admittance.Imaginary * voltageColumn.Real +
                                                                     admittance.Real * voltageColumn.Imaginary;
                return;
            }

            if (busRow == busColumn)
            {
                changeMatrix[matrixRow, matrixColumn] -= 2*admittance.Imaginary*voltageRow.Real;
                return;
            }

            changeMatrix[matrixRow, matrixRow] -= admittance.Imaginary*voltageColumn.Real +
                                                  admittance.Real*voltageColumn.Imaginary;
            changeMatrix[matrixRow, matrixColumn] = voltageRow.Imaginary*admittance.Real -
                                                    voltageRow.Real*admittance.Imaginary;
        }

        private static void FillChangeMatrixImaginaryPowerByImaginaryPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            int matrixRow;
            int matrixColumn;

            if (!busToMatrixIndex.TryGetValue(busRow, out matrixRow))
                return;

            if (!busToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                changeMatrix[matrixRow, matrixRow] += admittance.Real * voltageColumn.Real -
                                                    admittance.Imaginary * voltageColumn.Imaginary;
                return;
            }

            if (busRow == busColumn)
            {
                changeMatrix[matrixRow, matrixColumn] -= 2*admittance.Imaginary*
                                                         voltageRow.Imaginary;
                return;
            }

            changeMatrix[matrixRow, matrixRow] += admittance.Real*voltageColumn.Real -
                                                  admittance.Imaginary*
                                                  voltageColumn.Imaginary;
            changeMatrix[matrixRow, matrixColumn] = (-1)*(voltageRow.Real*admittance.Real +
                                                          voltageRow.Imaginary*
                                                          admittance.Imaginary);
        }

        private static void FillChangeMatrixRealPowerByAngle(SubMatrix changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = busToMatrixIndex[busRow];
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            int matrixColumn;

            if (busRow == busColumn)
                return;

            if (pvBusToMatrixIndex.TryGetValue(busRow, out matrixColumn))
                changeMatrix[matrixRow, matrixColumn] +=
                    admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude *
                    Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);

            if (pvBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                changeMatrix[matrixRow, matrixColumn] =
                    admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
        }

        private static void FillChangeMatrixImaginaryPowerByAngle(SubMatrix changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            int matrixRow;
            int matrixColumn;

            if (!rowBusToMatrixIndex.TryGetValue(busRow, out matrixRow) ||
                !columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            if (busRow == busColumn)
                return;

            changeMatrix[matrixRow, matrixColumn] = 
                (-1) * admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Cos(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        private static IIterationStopCriterion<double> CreateStopCriterion(ICollection<double> powersRealError, ICollection<double> powersImaginaryError,
            double residualImprovementFactor)
        {
            var realPowerMaximumError = powersRealError.Select(Math.Abs).Max();
            var imaginaryPowerMaximumError = powersImaginaryError.Count > 0 ? powersImaginaryError.Select(Math.Abs).Max() : 0;
            var powerMaximumError = Math.Max(realPowerMaximumError, imaginaryPowerMaximumError);
            var residualStopCriterion = new ResidualStopCriterion<double>(powerMaximumError * residualImprovementFactor);
            var iterationStopCriterion =
                new IterationCountStopCriterion<double>(Math.Max(20, powersRealError.Count + powersImaginaryError.Count));
            var stopCriterion =
                new DelegateStopCriterion<double>(
                    (i, vector, arg3, arg4) =>
                        residualStopCriterion.DetermineStatus(i, vector, arg3, arg4) == IterationStatus.Continue &&
                        iterationStopCriterion.DetermineStatus(i, vector, arg3, arg4) == IterationStatus.Continue
                            ? IterationStatus.Continue
                            : IterationStatus.StoppedWithoutConvergence);
            return stopCriterion;
        }
    }
}
