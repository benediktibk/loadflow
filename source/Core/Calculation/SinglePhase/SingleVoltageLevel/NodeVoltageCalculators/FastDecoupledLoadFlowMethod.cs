using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            var unknownAngles = pqBuses.Count + pvBuses.Count;
            var unknownMagnitudes = pqBuses.Count;
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(pqBuses.Count + pvBuses.Count);
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);
            var pqBusIdToAmplitudeIndex = CreateMappingBusIdToIndex(pqBuses, allNodes.Count);

            var angleChange = CalculateAngleChange(admittances, voltages, constantCurrents, powersRealError, unknownAngles, allNodes);

            if (pqBuses.Count > 0)
            {
                var amplitudeChange = CalculateAmplitudeChange(admittances, voltages, constantCurrents, powersImaginaryError,
                    pqBuses, unknownMagnitudes);

                foreach (var bus in pqBuses)
                    improvedVoltages[bus] =
                        Complex.FromPolarCoordinates(
                            voltages[bus].Magnitude + amplitudeChange[pqBusIdToAmplitudeIndex[bus]],
                            voltages[bus].Phase + angleChange[bus]);
            }

            for(var i = 0; i < pvBuses.Count; ++i)
                improvedVoltages[pvBuses[i]] = Complex.FromPolarCoordinates(pvBusVoltages[i], voltages[pvBuses[i]].Phase + angleChange[pvBuses[i]]);

            return improvedVoltages;
        }

        private static Vector<double> CalculateAmplitudeChange(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
            IEnumerable<double> powersImaginaryError, IList<int> pqBuses, int unknownMagnitudes)
        {
            var changeMatrixImaginaryPower = new DenseMatrix(unknownMagnitudes, unknownMagnitudes);
            CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrixImaginaryPower, admittances, voltages,
                constantCurrents, 0, 0, pqBuses, pqBuses);
            var factorizationImaginaryPower = changeMatrixImaginaryPower.LU();
            return factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));
        }

        private static Vector<double> CalculateAngleChange(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents,
            IEnumerable<double> powersRealError, int unknownAngles, IList<int> allNodes)
        {
            var changeMatrixRealPower = new DenseMatrix(unknownAngles, unknownAngles);
            CalculateChangeMatrixRealPowerByAngle(changeMatrixRealPower, admittances, voltages, constantCurrents, 0,
                0, allNodes, allNodes);
            var factorizationRealPower = changeMatrixRealPower.LU();
            return factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));
        }
    }
}
