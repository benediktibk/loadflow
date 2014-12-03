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
            var stopCriterion = CreateStopCriterion(powersRealError, powersImaginaryError, residualImprovementFactor);
            var solver = new TFQMR();
            solver.Solve(changeMatrix, rightSide, voltageChanges, new Iterator<double>(stopCriterion), new ILU0Preconditioner());
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
            var realPowerByRealPart = new SubMatrix(changeMatrix, 0, 0, busToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var realPowerByImaginaryPart = new SubMatrix(changeMatrix, 0, pqBusToMatrixIndex.Count, busToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var imaginaryPowerByRealPart = new SubMatrix(changeMatrix, busToMatrixIndex.Count, 0, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var imaginaryPowerByImaginaryPart = new SubMatrix(changeMatrix, busToMatrixIndex.Count, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count);
            var realPowerByAngle = new SubMatrix(changeMatrix, 0, 2*pqBusToMatrixIndex.Count, busToMatrixIndex.Count,
                pvBusToMatrixIndex.Count);
            var imaginaryPowerByAngle = new SubMatrix(changeMatrix, busToMatrixIndex.Count, 2 * pqBusToMatrixIndex.Count, pqBusToMatrixIndex.Count,
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
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var voltage = voltages[busIndex];
                var current = constantCurrents[busIndex];
                changeMatrix[matrixIndex, matrixIndex + pqBusToMatrixIndex.Count * 2] +=
                    voltage.Magnitude * current.Magnitude * Math.Sin(voltage.Phase - current.Phase);
            }

            return changeMatrix;
        }

        private static void FillChangeMatrixRealPowerByRealPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = busToMatrixIndex[busRow];
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                if (!pqBusToMatrixIndex.ContainsKey(busRow)) 
                    return;

                var voltage = voltages[busColumn];
                changeMatrix[matrixRow, matrixRow] += admittance.Real*voltage.Real -
                                                      admittance.Imaginary*voltage.Imaginary;
                return;
            }

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixRow] += 2*admittance.Real*voltageRow.Real;
            else
            {
                if (pqBusToMatrixIndex.ContainsKey(busRow))
                    changeMatrix[matrixRow, matrixRow] += admittance.Real*voltageColumn.Real -
                                                      admittance.Imaginary*voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn] = voltageRow.Real*admittance.Real +
                                                        voltageRow.Imaginary*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixRealPowerByImaginaryPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = busToMatrixIndex[busRow];
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                if (!pqBusToMatrixIndex.ContainsKey(busRow))
                    return;

                var voltage = voltages[busColumn];
                changeMatrix[matrixRow, matrixRow] += admittance.Imaginary * voltage.Real +
                                                                     admittance.Real * voltage.Imaginary;
                return;
            }

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixColumn] += 2*admittance.Real*voltageRow.Imaginary;
            else
            {
                if (pqBusToMatrixIndex.ContainsKey(busRow))
                    changeMatrix[matrixRow, matrixRow] += admittance.Imaginary*voltageColumn.Real +
                                                                     admittance.Real*voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn] = voltageRow.Imaginary*admittance.Real -
                                                                       voltageRow.Real*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixImaginaryPowerByRealPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            int matrixRow;
            int matrixColumn;

            if (!pqBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                return;

            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                var voltage = voltages[busColumn];
                changeMatrix[matrixRow, matrixRow] -= admittance.Imaginary * voltage.Real +
                                                                     admittance.Real * voltage.Imaginary;
                return;
            }

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixColumn] -= 2*admittance.Imaginary*voltageRow.Real;
            else
            {
                changeMatrix[matrixRow, matrixRow] -= admittance.Imaginary*voltageColumn.Real +
                                                                     admittance.Real*voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn] = voltageRow.Imaginary*admittance.Real -
                                                                    voltageRow.Real*admittance.Imaginary;
            }
        }

        private static void FillChangeMatrixImaginaryPowerByImaginaryPart(SubMatrix changeMatrix, IList<Complex> voltages,
            IReadOnlyDictionary<int, int> busToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            int matrixRow;
            int matrixColumn;

            if (!busToMatrixIndex.TryGetValue(busRow, out matrixRow))
                return;

            if (!busToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
            {
                var voltage = voltages[busColumn];
                changeMatrix[matrixRow, matrixRow] += admittance.Real * voltage.Real -
                                                                                    admittance.Imaginary *
                                                                                    voltage.Imaginary;
                return;
            }

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];

            if (matrixRow == matrixColumn)
                changeMatrix[matrixRow, matrixColumn] -= 2*admittance.Imaginary*
                                                                                    voltageRow.Imaginary;
            else
            {
                changeMatrix[matrixRow, matrixRow] += admittance.Real * voltageColumn.Real -
                                                                                    admittance.Imaginary*
                                                                                    voltageColumn.Imaginary;
                changeMatrix[matrixRow, matrixColumn] = (-1)*(voltageRow.Real*admittance.Real +
                                                                                    voltageRow.Imaginary*
                                                                                    admittance.Imaginary);
            }
        }

        private static void FillChangeMatrixRealPowerByAngle(SubMatrix changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var matrixRow = rowBusToMatrixIndex[busRow];
            int matrixColumn;

            if (!columnBusToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                return;

            if (matrixRow == matrixColumn)
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            changeMatrix[matrixRow, matrixColumn] =
                admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);

            if (columnBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                changeMatrix[matrixRow, matrixRow] +=
                    admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        private static void FillChangeMatrixImaginaryPowerByAngle(SubMatrix changeMatrix, IList<Complex> voltages, IReadOnlyDictionary<int, int> rowBusToMatrixIndex, IReadOnlyDictionary<int, int> columnBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
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
            changeMatrix[matrixRow, matrixColumn] = 
                (-1) * admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Cos(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);

            if (columnBusToMatrixIndex.TryGetValue(busRow, out matrixRow))
                changeMatrix[matrixRow, matrixRow] +=
                    voltageRow.Magnitude*admittance.Magnitude*voltageColumn.Magnitude*
                    Math.Cos(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
        }

        private static ResidualStopCriterion<double> CreateStopCriterion(IEnumerable<double> powersRealError, ICollection<double> powersImaginaryError,
            double residualImprovementFactor)
        {
            var realPowerMaximumError = powersRealError.Select(Math.Abs).Max();
            var imaginaryPowerMaximumError = powersImaginaryError.Count > 0 ? powersImaginaryError.Select(Math.Abs).Max() : 0;
            var powerMaximumError = Math.Max(realPowerMaximumError, imaginaryPowerMaximumError);
            var stopCriterion = new ResidualStopCriterion<double>(powerMaximumError * residualImprovementFactor);
            return stopCriterion;
        }
    }
}
