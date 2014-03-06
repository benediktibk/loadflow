using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, 0.1, targetPrecision*1E4)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var unknownAngles = pqBuses.Count + pvBuses.Count;
            var unknownMagnitudes = pqBuses.Count;
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(pqBuses.Count + pvBuses.Count);;

            var changeMatrixRealPower = new DenseMatrix(unknownAngles, unknownAngles);
            CalculateChangeMatrixRealPowerByAngle(changeMatrixRealPower, admittances, voltages, constantCurrents, 0,
                0);
            var factorizationRealPower = changeMatrixRealPower.QR();
            var angleChange = factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));
            Vector<double> amplitudeChange = null;

            if (pqBuses.Count > 0)
            {
                var changeMatrixImaginaryPower = new DenseMatrix(unknownMagnitudes, unknownMagnitudes);
                CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrixImaginaryPower, admittances, voltages,
                    constantCurrents, 0, 0, pqBuses);
                var factorizationImaginaryPower = changeMatrixImaginaryPower.QR();
                amplitudeChange = factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));
            }

            var busIDToAmplitudeIndex = new Dictionary<int, int>();
            var busIndex = 0;

            for (var i = 0; i < pqBuses.Count + pvBuses.Count && busIndex < pqBuses.Count; ++i)
                if (i == pqBuses[busIndex].ID)
                {
                    busIDToAmplitudeIndex[pqBuses[busIndex].ID] = i;
                    ++busIndex;
                }

            foreach (var bus in pqBuses)
                improvedVoltages[bus.ID] = Complex.FromPolarCoordinates(voltages[bus.ID].Magnitude + amplitudeChange[busIDToAmplitudeIndex[bus.ID]], voltages[bus.ID].Phase + angleChange[bus.ID]);

            foreach (var bus in pvBuses)
                improvedVoltages[bus.ID] = Complex.FromPolarCoordinates(bus.VoltageMagnitude, voltages[bus.ID].Phase + angleChange[bus.ID]);

            return improvedVoltages;
        }
    }
}
