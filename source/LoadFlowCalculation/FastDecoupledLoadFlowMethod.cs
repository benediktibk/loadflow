using System.Collections.Generic;
using System.Diagnostics;
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

        public override Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            Debug.Assert(pqBuses.Count + pvBuses.Count == admittances.RowCount);
            Debug.Assert(pvBuses.Count == pvBusVoltages.Count);

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

            var pqBusIdToAmplitudeIndex = CreateMappingPQBusIdToAmplitudeIndex(pqBuses, pvBuses);

            foreach (var bus in pqBuses)
                improvedVoltages[bus] = Complex.FromPolarCoordinates(voltages[bus].Magnitude + amplitudeChange[pqBusIdToAmplitudeIndex[bus]], voltages[bus].Phase + angleChange[bus]);

            for(var i = 0; i < pvBuses.Count; ++i)
                improvedVoltages[pvBuses[i]] = Complex.FromPolarCoordinates(pvBusVoltages[i], voltages[pvBuses[i]].Phase + angleChange[pvBuses[i]]);

            return improvedVoltages;
        }

        public static Dictionary<int, int> CreateMappingPQBusIdToAmplitudeIndex(IList<int> pqBuses, IList<int> pvBuses)
        {
            var busIDToAmplitudeIndex = new Dictionary<int, int>();
            var busIndex = 0;

            for (var i = 0; i < pqBuses.Count + pvBuses.Count && busIndex < pqBuses.Count; ++i)
                if (i == pqBuses[busIndex])
                {
                    busIDToAmplitudeIndex[pqBuses[busIndex]] = i;
                    ++busIndex;
                }
            return busIDToAmplitudeIndex;
        }
    }
}
