using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, 0, targetPrecision*100)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);

            var loadCurrents = admittances * voltages;
            var totalCurrents = loadCurrents - constantCurrents;
            var changeMatrix = new DenseMatrix(pqBuses.Count * 2 + pvBuses.Count, pqBuses.Count * 2 + pvBuses.Count);

            CalculateChangeMatrixRealPowerByRealPart(changeMatrix, admittances, voltages, totalCurrents, 0, 0, allNodes, pqBuses);
            CalculateChangeMatrixRealPowerByImaginaryPart(changeMatrix, admittances, voltages, totalCurrents, 0, pqBuses.Count, allNodes, pqBuses);
            CalculateChangeMatrixImaginaryPowerByRealPart(changeMatrix,
                admittances, voltages, totalCurrents, allNodes.Count, 0, pqBuses, pqBuses);
            CalculateChangeMatrixImaginaryPowerByImaginaryPart(changeMatrix,
                admittances, voltages, totalCurrents, allNodes.Count, pqBuses.Count, pqBuses, pqBuses);
            CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0,
                pqBuses.Count * 2, allNodes, pvBuses);
            CalculateChangeMatrixImaginaryPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, allNodes.Count,
                pqBuses.Count * 2, pqBuses, pvBuses);

            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var factorization = changeMatrix.QR();
            var voltageChanges = factorization.Solve(rightSide);
            IList<double> voltageChangesReal;
            IList<double> voltageChangesImaginary;
            IList<double> voltageChangesAngle;
            DivideParts(voltageChanges, pqBuses.Count, pqBuses.Count, out voltageChangesReal, out voltageChangesImaginary, out voltageChangesAngle);
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(allNodes.Count);
            var mappingPQBusToIndex = CreateMappingBusIdToIndex(pqBuses, allNodes.Count);
            var mappingPVBusToIndex = CreateMappingBusIdToIndex(pvBuses, allNodes.Count);

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
    }
}
