using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Factorization;

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

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            Vector<Complex> voltages = DenseVector.OfVector(initialVoltages);
            var powers = CollectPowers(pqBuses, pvBuses);
            var totalAbsolutePowerSum = powers.Sum(x => Math.Abs(x.Real) + Math.Abs(x.Imaginary));
            var iterations = 0;
            bool accurateEnough;
            var factorization = admittances.CalculateFactorization();

            do
            {
                bool powerErrorTooBig;
                var rightHandSide = CalculateRightHandSide(constantCurrents, powers, voltages);

                var newVoltages = CalculateImprovedVoltagesAndPowers(admittances, constantCurrents, pvBuses, factorization,
                    rightHandSide, powers, out powerErrorTooBig);

                accurateEnough = CheckAccuracy(admittances, nominalVoltage, constantCurrents, pqBuses, pvBuses, newVoltages, voltages, totalAbsolutePowerSum, powerErrorTooBig);
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && !accurateEnough);

            return voltages;
        }

        public double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        private static DenseVector CalculateRightHandSide(IList<Complex> constantCurrents, IList<Complex> powers, IList<Complex> voltages)
        {
            var nodeCount = constantCurrents.Count;
            var rightHandSide = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                rightHandSide[i] = constantCurrents[i] + (powers[i] / voltages[i]).Conjugate();

            return rightHandSide;
        }

        private static DenseVector CollectPowers(IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var nodeCount = pqBuses.Count() + pvBuses.Count();
            var powers = new DenseVector(nodeCount);

            foreach (var bus in pqBuses)
                powers[bus.Index] = bus.Power;

            foreach (var bus in pvBuses)
                powers[bus.Index] = new Complex(bus.RealPower, 0);

            return powers;
        }

        private bool CheckAccuracy(IReadOnlyAdmittanceMatrix admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses,
            IList<PvNodeWithIndex> pvBuses, Vector<Complex> newVoltages, Vector<Complex> voltages, double totalAbsolutePowerSum, bool powerErrorTooBig)
        {
            var voltageDifference = newVoltages.Subtract(voltages);
            var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
            var voltageChange = maximumVoltageDifference.Magnitude/nominalVoltage;
            var absolutePowerError = PowerNetComputable.CalculatePowerError(admittances, newVoltages, constantCurrents,
                pqBuses, pvBuses);
            var relativePowerError = totalAbsolutePowerSum != 0
                ? absolutePowerError/totalAbsolutePowerSum
                : absolutePowerError;
            return voltageChange/nominalVoltage < _targetPrecision/10 && !powerErrorTooBig &&
                         relativePowerError < MaximumRelativePowerError;
        }

        private Vector<Complex> CalculateImprovedVoltagesAndPowers(IReadOnlyAdmittanceMatrix admittances, IList<Complex> constantCurrents, IEnumerable<PvNodeWithIndex> pvBuses,
            ISolver<Complex> factorization, Vector<Complex> rightHandSide, IList<Complex> powers, out bool powerErrorTooBig)
        {
            powerErrorTooBig = false;
            var newVoltages = factorization.Solve(rightHandSide);

            foreach (var bus in pvBuses)
            {
                var newVoltage = newVoltages[bus.Index];
                newVoltage = Complex.FromPolarCoordinates(bus.VoltageMagnitude, newVoltage.Phase);
                newVoltages[bus.Index] = newVoltage;
                var newPower = CalculatePower(bus.Index, admittances, constantCurrents, newVoltages);

                if (Math.Abs((newPower.Real - bus.RealPower)/bus.RealPower) > _targetPrecision)
                    powerErrorTooBig = true;

                powers[bus.Index] = new Complex(bus.RealPower, newPower.Imaginary);
            }

            return newVoltages;
        }

        private Complex CalculatePower(int i, IReadOnlyAdmittanceMatrix admittances, IList<Complex> constantCurrents,
            Vector<Complex> voltages)
        {
            var voltage = voltages[i];
            var constantCurrent = constantCurrents[i];
            var branchAdmittances = admittances.GetRow(i);
            var branchCurrent = (branchAdmittances.PointwiseMultiply(voltages)).Sum();
            var totalCurrents = branchCurrent - constantCurrent;
            return voltage * totalCurrents.Conjugate();
        }
    }
}