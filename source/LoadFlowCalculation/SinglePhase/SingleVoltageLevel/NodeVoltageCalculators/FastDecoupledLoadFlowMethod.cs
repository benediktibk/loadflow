using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            Debug.Assert(pqBuses.Count + pvBuses.Count == admittances.RowCount);
            Debug.Assert(pvBuses.Count == pvBusVoltages.Count);

            var unknownAngles = pqBuses.Count + pvBuses.Count;
            var unknownMagnitudes = pqBuses.Count;
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(pqBuses.Count + pvBuses.Count);
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);

            var changeMatrixRealPower = new DenseMatrix(unknownAngles, unknownAngles);
            CalculateChangeMatrixRealPowerByAngle(changeMatrixRealPower, admittances, voltages, constantCurrents, 0,
                0, allNodes, allNodes);
            var factorizationRealPower = changeMatrixRealPower.QR();
            var angleChange = factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));
            Vector<double> amplitudeChange = null;

            if (pqBuses.Count > 0)
            {
                var changeMatrixImaginaryPower = new DenseMatrix(unknownMagnitudes, unknownMagnitudes);
                CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrixImaginaryPower, admittances, voltages,
                    constantCurrents, 0, 0, pqBuses, pqBuses);
                var factorizationImaginaryPower = changeMatrixImaginaryPower.QR();
                amplitudeChange = factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));
            }

            var pqBusIdToAmplitudeIndex = CreateMappingBusIdToIndex(pqBuses, allNodes.Count);

            foreach (var bus in pqBuses)
                improvedVoltages[bus] = Complex.FromPolarCoordinates(voltages[bus].Magnitude + amplitudeChange[pqBusIdToAmplitudeIndex[bus]], voltages[bus].Phase + angleChange[bus]);

            for(var i = 0; i < pvBuses.Count; ++i)
                improvedVoltages[pvBuses[i]] = Complex.FromPolarCoordinates(pvBusVoltages[i], voltages[pvBuses[i]].Phase + angleChange[pvBuses[i]]);

            return improvedVoltages;
        }
    }
}
