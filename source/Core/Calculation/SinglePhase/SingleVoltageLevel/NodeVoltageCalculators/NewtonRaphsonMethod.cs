using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;

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

        public static Vector<double> CalculateVoltageChanges(IList<double> powersRealError, IList<double> powersImaginaryError, DenseMatrix changeMatrix, double residualImprovementFactor)
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

        public static DenseMatrix CalculateChangeMatrix(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
            IList<int> pqBuses, IList<int> pvBuses)
        {
            var loadCurrents = admittances.CalculateCurrents(voltages);
            var totalCurrents = loadCurrents - constantCurrents;
            var changeMatrix = new DenseMatrix(pqBuses.Count*2 + pvBuses.Count, pqBuses.Count*2 + pvBuses.Count);
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);
            CalculateChangeMatrixRealPowerByRealPart(changeMatrix, admittances, voltages, totalCurrents, 0, 0, allNodes, pqBuses);
            CalculateChangeMatrixRealPowerByImaginaryPart(changeMatrix, admittances, voltages, totalCurrents, 0, pqBuses.Count,
                allNodes, pqBuses);
            CalculateChangeMatrixImaginaryPowerByRealPart(changeMatrix,
                admittances, voltages, totalCurrents, allNodes.Count, 0, pqBuses, pqBuses);
            CalculateChangeMatrixImaginaryPowerByImaginaryPart(changeMatrix,
                admittances, voltages, totalCurrents, allNodes.Count, pqBuses.Count, pqBuses, pqBuses);
            CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0,
                pqBuses.Count*2, allNodes, pvBuses);
            CalculateChangeMatrixImaginaryPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, allNodes.Count,
                pqBuses.Count*2, pqBuses, pvBuses);
            return changeMatrix;
        }
    }
}
