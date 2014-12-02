using System;
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
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages, double residualImprovementFactor)
        {
            var changeMatrix = CalculateChangeMatrix(admittances, voltages, constantCurrents, pqBuses, pvBuses);
            var voltageChanges = CalculateVoltageChanges(powersRealError, powersImaginaryError, changeMatrix, residualImprovementFactor);
            return CalculateImprovedVoltagesFromVoltageChanges(voltages, pqBuses, pvBuses, pvBusVoltages, voltageChanges);
        }

        public static DenseVector CalculateImprovedVoltagesFromVoltageChanges(IList<Complex> voltages, IList<int> pqBuses, IList<int> pvBuses,
            IList<double> pvBusVoltages, IList<double> voltageChanges)
        {
            IList<double> voltageChangesReal;
            IList<double> voltageChangesImaginary;
            IList<double> voltageChangesAngle;
            DivideParts(voltageChanges, pqBuses.Count, pqBuses.Count, out voltageChangesReal, out voltageChangesImaginary,
                out voltageChangesAngle);
            var nodeCount = pqBuses.Count + pvBuses.Count;
            var improvedVoltages = new DenseVector(nodeCount);
            var mappingPqBusToIndex = CreateMappingBusToMatrixIndex(pqBuses);
            var mappingPvBusToIndex = CreateMappingBusToMatrixIndex(pvBuses);

            foreach (var bus in pqBuses)
            {
                var index = mappingPqBusToIndex[bus];
                improvedVoltages[bus] = voltages[bus] + new Complex(voltageChangesReal[index], voltageChangesImaginary[index]);
            }

            foreach (var bus in pvBuses)
            {
                var index = mappingPvBusToIndex[bus];
                improvedVoltages[bus] = new Complex(pvBusVoltages[index], voltages[bus].Phase + voltageChangesAngle[index]);
            }

            return improvedVoltages;
        }

        public static Vector<double> CalculateVoltageChanges(IList<double> powersRealError, IList<double> powersImaginaryError, Matrix<double> changeMatrix, double residualImprovementFactor)
        {
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var voltageChanges = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(rightSide.Count);
            var realPowerMaximumError = powersRealError.Select(Math.Abs).Max();
            var imaginaryPowerMaximumError = powersImaginaryError.Count > 0 ? powersImaginaryError.Select(Math.Abs).Max() : 0;
            var powerMaximumError = Math.Max(realPowerMaximumError, imaginaryPowerMaximumError);
            var stopCriterion = new Iterator<double>(new ResidualStopCriterion<double>(powerMaximumError * residualImprovementFactor));
            var preconditioner = new ILU0Preconditioner();
            var solver = new TFQMR();
            solver.Solve(changeMatrix, rightSide, voltageChanges, stopCriterion, preconditioner);
            return voltageChanges;
        }

        public static Matrix<double> CalculateChangeMatrix(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
            IList<int> pqBuses, IList<int> pvBuses)
        {
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);
            var busToMatrixIndex = CreateMappingBusToMatrixIndex(allNodes);
            var pqBusToMatrixIndex = CreateMappingBusToMatrixIndex(pqBuses);
            var pvBusToMatrixIndex = CreateMappingBusToMatrixIndex(pvBuses);
            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(pqBusToMatrixIndex.Count * 2 + pvBusToMatrixIndex.Count, pqBusToMatrixIndex.Count * 2 + pvBusToMatrixIndex.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                var admittance = entry.Item3;
                FillChangeMatrixRealPowerByRealPart(changeMatrix, voltages, busToMatrixIndex, pqBusToMatrixIndex, busRow,
                    busColumn, admittance);
                FillChangeMatrixRealPowerByImaginaryPart(changeMatrix, voltages, busToMatrixIndex, pqBusToMatrixIndex, busRow,
                    busColumn, admittance, pqBusToMatrixIndex.Count);
                FillChangeMatrixImaginaryPowerByRealPart(changeMatrix, voltages, pqBusToMatrixIndex, busRow, busColumn,
                    admittance, busToMatrixIndex.Count);
                FillChangeMatrixImaginaryPowerByImaginaryPart(changeMatrix, voltages, pqBusToMatrixIndex, busRow, busColumn,
                    admittance, busToMatrixIndex.Count, pqBusToMatrixIndex.Count);
                FillChangeMatrixRealPowerByAngle(changeMatrix, voltages, busToMatrixIndex, pvBusToMatrixIndex, busRow, busColumn, admittance, 2 * pqBusToMatrixIndex.Count);
                FillChangeMatrixImaginaryPowerByAngle(changeMatrix, voltages, pqBusToMatrixIndex, pvBusToMatrixIndex, busRow, busColumn,
                    admittance, busToMatrixIndex.Count, 2*pqBusToMatrixIndex.Count);
            }

            foreach (var bus in pqBusToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var current = constantCurrents[busIndex];
                changeMatrix[matrixIndex, matrixIndex] -= current.Real;
                changeMatrix[matrixIndex, matrixIndex + pqBusToMatrixIndex.Count] -= current.Imaginary;
                changeMatrix[matrixIndex + busToMatrixIndex.Count, matrixIndex] -= current.Imaginary;
                changeMatrix[matrixIndex + busToMatrixIndex.Count, matrixIndex + pqBusToMatrixIndex.Count] += current.Real;
            }

            foreach (var bus in pvBusToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var voltage = voltages[busIndex];
                var current = constantCurrents[busIndex];
                changeMatrix[matrixIndex, matrixIndex + pqBusToMatrixIndex.Count * 2] +=
                    voltage.Magnitude * current.Magnitude * Math.Sin(voltage.Phase - current.Phase);
            }

            return changeMatrix;
        }

        private static void FillChangeMatrixRealPowerByRealPart(Matrix<double> changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = rowBusToMatrixIndex[busRow];
            int matrixColumn;

            if (!columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixRow] += 2*admittance.Real*voltageRow.Real;
            else
            {
                changeMatrix[matrixRow, matrixRow] += admittance.Real*voltageColumn.Real -
                                                      admittance.Imaginary*voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn] = voltageRow.Real*admittance.Real +
                                                        voltageRow.Imaginary*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixRealPowerByImaginaryPart(Matrix<double> changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance, int columnOffset)
        {
            var matrixRow = rowBusToMatrixIndex[busRow];
            int matrixColumn;

            if (!columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixColumn + columnOffset] += 2*admittance.Real*voltageRow.Imaginary;
            else
            {
                changeMatrix[matrixRow, matrixRow + columnOffset] += admittance.Imaginary*voltageColumn.Real +
                                                                     admittance.Real*voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn + columnOffset] = voltageRow.Imaginary*admittance.Real -
                                                                       voltageRow.Real*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixImaginaryPowerByRealPart(Matrix<double> changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, int busRow, int busColumn, Complex admittance, int rowOffset)
        {
            int matrixRow;
            int matrixColumn;

            if (!busToMatrixIndex.TryGetValue(busRow, out matrixRow) || !busToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow + rowOffset, matrixColumn] -= 2*admittance.Imaginary*voltageRow.Real;
            else
            {
                changeMatrix[matrixRow + rowOffset, matrixRow] -= admittance.Imaginary*voltageColumn.Real +
                                                                     admittance.Real*voltageColumn.Imaginary;
                changeMatrix[matrixRow + rowOffset, matrixColumn] = voltageRow.Imaginary*admittance.Real -
                                                                    voltageRow.Real*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixImaginaryPowerByImaginaryPart(Matrix<double> changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, int busRow, int busColumn, Complex admittance, int rowOffset, int columnOffset)
        {
            int matrixRow;
            int matrixColumn;

            if (!busToMatrixIndex.TryGetValue(busRow, out matrixRow) || !busToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow + rowOffset, matrixColumn + columnOffset] -= 2*admittance.Imaginary*
                                                                                    voltageRow.Imaginary;
            else
            {
                changeMatrix[matrixRow + rowOffset, matrixRow + columnOffset] += admittance.Real * voltageColumn.Real -
                                                                                    admittance.Imaginary*
                                                                                    voltageColumn.Imaginary;
                changeMatrix[matrixRow + rowOffset, matrixColumn + columnOffset] = (-1)*
                                                                                   (voltageRow.Real*admittance.Real +
                                                                                    voltageRow.Imaginary*
                                                                                    admittance.Imaginary);
            }
        }

        private static void FillChangeMatrixRealPowerByAngle(Matrix<double> changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance, int columnOffset)
        {
            var matrixRow = rowBusToMatrixIndex[busRow];
            int matrixColumn;

            if (!columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            if (matrixRow == matrixColumn)
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            changeMatrix[matrixRow, matrixColumn + columnOffset] =
                admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);

            if (columnBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                changeMatrix[matrixRow, matrixRow + columnOffset] +=
                    admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        private static void FillChangeMatrixImaginaryPowerByAngle(Matrix<double> changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance, int rowOffset, int columnOffset)
        {
            int matrixRow;
            int matrixColumn;

            if (!rowBusToMatrixIndex.TryGetValue(busRow, out matrixRow) ||
                !columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            if (matrixRow == matrixColumn)
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            changeMatrix[matrixRow + rowOffset, matrixColumn + columnOffset] = 
                (-1) * admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Cos(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);

            if (columnBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                changeMatrix[matrixRow + rowOffset, matrixRow + columnOffset] +=
                    voltageRow.Magnitude*admittance.Magnitude*voltageColumn.Magnitude*
                    Math.Cos(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
        }
    }
}
