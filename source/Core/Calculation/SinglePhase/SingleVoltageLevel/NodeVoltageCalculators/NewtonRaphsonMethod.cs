using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(AdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            Debug.Assert(pvBuses.Count == pvBusVoltages.Count);
            var changeMatrix = CalculateChangeMatrix(admittances, voltages, constantCurrents, pqBuses, pvBuses);
            var voltageChanges = CalculateVoltageChanges(powersRealError, powersImaginaryError, changeMatrix);
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
            Debug.Assert(pqBuses.Count == voltageChangesReal.Count);
            Debug.Assert(pqBuses.Count == voltageChangesImaginary.Count);
            Debug.Assert(pvBuses.Count == voltageChangesAngle.Count);
            var nodeCount = pqBuses.Count + pvBuses.Count;
            var improvedVoltages = new DenseVector(nodeCount);
            var mappingPQBusToIndex = CreateMappingBusIdToIndex(pqBuses, nodeCount);
            var mappingPVBusToIndex = CreateMappingBusIdToIndex(pvBuses, nodeCount);

            foreach (var bus in pqBuses)
            {
                var index = mappingPQBusToIndex[bus];
                improvedVoltages[bus] = voltages[bus] + new Complex(voltageChangesReal[index], voltageChangesImaginary[index]);
            }

            foreach (var bus in pvBuses)
            {
                var index = mappingPVBusToIndex[bus];
                improvedVoltages[bus] = new Complex(pvBusVoltages[index], voltages[bus].Phase + voltageChangesAngle[index]);
            }

            return improvedVoltages;
        }

        public static Vector<double> CalculateVoltageChanges(IList<double> powersRealError, IList<double> powersImaginaryError,
            DenseMatrix changeMatrix)
        {
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var factorization = changeMatrix.LU();
            var voltageChanges = factorization.Solve(rightSide);
            return voltageChanges;
        }

        public static DenseMatrix CalculateChangeMatrix(AdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
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
