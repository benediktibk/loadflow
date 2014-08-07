using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class CurrentIteration : INodeVoltageCalculator
    {
        #region variables

        private readonly int _maximumIterations;
        private readonly double _targetPrecision;

        #endregion

        #region constructor

        public CurrentIteration(double targetPrecision, int maximumIterations)
        {
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
        }

        #endregion

        #region public functions

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var nodeCount = admittances.NodeCount;
            Vector<Complex> voltages = DenseVector.OfVector(initialVoltages);
            var powers = CollectPowers(pqBuses, pvBuses, nodeCount);
            var totalAbsolutePowerSum = powers.Sum(x => Math.Abs(x.Real) + Math.Abs(x.Imaginary));
            var iterations = 0;
            bool accurateEnough;
            var factorization = admittances.CalculateFactorization();

            do
            {
                bool powerErrorTooBig;
                var rightHandSide = CalculateRightHandSide(constantCurrents, nodeCount, powers, voltages);

                var newVoltages = CalculateImprovedVoltagesAndPowers(admittances, constantCurrents, pvBuses, factorization,
                    rightHandSide, powers, out powerErrorTooBig);

                accurateEnough = CheckAccuracy(admittances, nominalVoltage, constantCurrents, pqBuses, pvBuses, newVoltages, voltages, totalAbsolutePowerSum, powerErrorTooBig);
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && !accurateEnough);

            return voltages;
        }

        public double GetMaximumPowerError()
        {
            return 0.1;
        }

        #endregion

        #region private functions

        private bool CheckAccuracy(AdmittanceMatrix admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses,
            IList<PVBus> pvBuses, Vector<Complex> newVoltages, Vector<Complex> voltages, double totalAbsolutePowerSum, bool powerErrorTooBig)
        {
            var voltageDifference = newVoltages.Subtract(voltages);
            var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
            var voltageChange = maximumVoltageDifference.Magnitude/nominalVoltage;
            var absolutePowerError = LoadFlowCalculator.CalculatePowerError(admittances, newVoltages, constantCurrents,
                pqBuses, pvBuses);
            var relativePowerError = totalAbsolutePowerSum != 0
                ? absolutePowerError/totalAbsolutePowerSum
                : absolutePowerError;
            var result = voltageChange/nominalVoltage < _targetPrecision/10 && !powerErrorTooBig &&
                         relativePowerError < GetMaximumPowerError();
            return result;
        }

        private Vector<Complex> CalculateImprovedVoltagesAndPowers(AdmittanceMatrix admittances, IList<Complex> constantCurrents, IEnumerable<PVBus> pvBuses,
            ISolver<Complex> factorization, Vector<Complex> rightHandSide, IList<Complex> powers, out bool powerErrorTooBig)
        {
            powerErrorTooBig = false;
            var newVoltages = factorization.Solve(rightHandSide);

            foreach (var bus in pvBuses)
            {
                var newVoltage = newVoltages[bus.ID];
                newVoltage = Complex.FromPolarCoordinates(bus.VoltageMagnitude, newVoltage.Phase);
                newVoltages[bus.ID] = newVoltage;
                var newPower = CalculatePower(bus.ID, admittances, constantCurrents, newVoltages);

                if (Math.Abs((newPower.Real - bus.RealPower)/bus.RealPower) > _targetPrecision)
                    powerErrorTooBig = true;

                powers[bus.ID] = new Complex(bus.RealPower, newPower.Imaginary);
            }
            return newVoltages;
        }

        private Complex CalculatePower(int i, AdmittanceMatrix admittances, IList<Complex> constantCurrents,
            Vector<Complex> voltages)
        {
            var voltage = voltages[i];
            var constantCurrent = constantCurrents[i];
            var branchAdmittances = admittances.GetRow(i);
            var branchCurrent = (branchAdmittances.PointwiseMultiply(voltages)).Sum();
            var totalCurrents = branchCurrent - constantCurrent;
            return voltage * totalCurrents.Conjugate();
        }

        #endregion

        #region private static functions

        private static DenseVector CalculateRightHandSide(IList<Complex> constantCurrents, int nodeCount, IList<Complex> powers,
            IList<Complex> voltages)
        {
            var rightHandSide = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                rightHandSide[i] = constantCurrents[i] + (powers[i]/voltages[i]).Conjugate();

            return rightHandSide;
        }

        private static DenseVector CollectPowers(IEnumerable<PQBus> pqBuses, IEnumerable<PVBus> pvBuses, int nodeCount)
        {
            var powers = new DenseVector(nodeCount);

            foreach (var bus in pqBuses)
                powers[bus.ID] = bus.Power;

            foreach (var bus in pvBuses)
                powers[bus.ID] = new Complex(bus.RealPower, 0);

            return powers;
        }

        #endregion

    }
}