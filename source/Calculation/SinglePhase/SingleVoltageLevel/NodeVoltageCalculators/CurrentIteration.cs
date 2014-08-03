using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class CurrentIteration : INodeVoltageCalculator
    {
        private readonly int _maximumIterations;
        private readonly double _targetPrecision;

        public CurrentIteration(double targetPrecision, int maximumIterations)
        {
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
        }

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses,
            Vector<Complex> initialVoltages)
        {
            var nodeCount = admittances.NodeCount;
            var powers = new DenseVector(nodeCount);
            var voltages = initialVoltages;
            var totalAbsolutePowerSum = 0.0;

            foreach (var bus in pqBuses)
            {
                powers[bus.ID] = bus.Power;
                totalAbsolutePowerSum += Math.Abs(bus.Power.Real) + Math.Abs(bus.Power.Imaginary);
            }

            foreach (var bus in pvBuses)
            {
                powers[bus.ID] = new Complex(bus.RealPower, 0);
                totalAbsolutePowerSum += Math.Abs(bus.RealPower);
            }

            var iterations = 0;
            double voltageChange;
            bool powerErrorTooBig;
            bool accurateEnough;
            var factorization = admittances.CalculateFactorization();

            do
            {
                powerErrorTooBig = false;
                var rightHandSide = new DenseVector(nodeCount);

                for (var i = 0; i < nodeCount; ++i)
                    rightHandSide[i] = constantCurrents[i] + (powers[i]/voltages[i]).Conjugate();

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

                var voltageDifference = newVoltages.Subtract(voltages);
                var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
                voltageChange = maximumVoltageDifference.Magnitude / nominalVoltage;
                var absolutePowerError = LoadFlowCalculator.CalculatePowerError(admittances, newVoltages, constantCurrents,
                    pqBuses, pvBuses);
                var relativePowerError = totalAbsolutePowerSum != 0
                    ? absolutePowerError/totalAbsolutePowerSum
                    : absolutePowerError;
                accurateEnough = voltageChange/nominalVoltage < _targetPrecision/10 && !powerErrorTooBig &&
                                 relativePowerError < GetMaximumPowerError();
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && !accurateEnough);

            return voltages;
        }

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, IReadOnlyList<Complex> nominalVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var initialVoltages = DenseVector.OfEnumerable(nominalVoltages);

            return CalculateUnknownVoltages(admittances, nominalVoltage, constantCurrents, pqBuses, pvBuses,
                initialVoltages);
        }

        public double GetMaximumPowerError()
        {
            return 0.1;
        }

        private Complex CalculatePower(int i, AdmittanceMatrix admittances, IList<Complex> constantCurrents,
            Vector<Complex> voltages)
        {
            var voltage = voltages[i];
            var constantCurrent = constantCurrents[i];
            var branchAdmittances = admittances.GetRow(i);
            var branchCurrent = (branchAdmittances.PointwiseMultiply(voltages)).Sum();
            var totalCurrents = branchCurrent - constantCurrent;
            return voltage*totalCurrents.Conjugate();
        }
    }
}