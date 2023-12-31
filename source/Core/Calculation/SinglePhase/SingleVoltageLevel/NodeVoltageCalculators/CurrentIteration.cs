﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Solvers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class CurrentIteration : NodeVoltageCalculator
    {
        private ISolver<Complex> _factorization;

        public CurrentIteration(double targetPrecision, int maximumIterations, bool iterativeSolver)
        {
            MaximumIterations = maximumIterations;
            TargetPrecision = targetPrecision;
            IterativeSolver = iterativeSolver;
        }

        public override double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        public int MaximumIterations { get; private set; }
        public double TargetPrecision { get; private set; }
        public bool IterativeSolver { get; private set; }

        public override Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            ResetProgress();
            Vector<Complex> voltages = DenseVector.OfVector(initialVoltages);
            var powers = CollectPowers(pqBuses, pvBuses);
            var totalAbsolutePowerSum = powers.Sum(x => Math.Abs(x.Real) + Math.Abs(x.Imaginary));
            var iterations = 0;
            bool accurateEnough;

            if (!IterativeSolver)
                _factorization = admittances.CalculateFactorization();

            do
            {
                bool powerErrorTooBig;
                var rightHandSide = CalculateRightHandSide(constantCurrents, powers, voltages);

                var newVoltages = CalculateImprovedVoltagesAndPowers(admittances, constantCurrents, pvBuses,
                    rightHandSide, powers, voltages, out powerErrorTooBig);

                var voltageChange = CalculateVoltageChange(newVoltages, voltages);
                var absolutePowerError = admittances.CalculatePowerError(newVoltages, constantCurrents, pqBuses, pvBuses);
                var relativePowerError = totalAbsolutePowerSum != 0 ? absolutePowerError/totalAbsolutePowerSum : absolutePowerError;
                accurateEnough = 10*voltageChange/nominalVoltage < TargetPrecision && !powerErrorTooBig && relativePowerError < MaximumRelativePowerError;
                voltages = newVoltages;
                ++iterations;
                Progress = (double) iterations/MaximumIterations;
                RelativePowerError = relativePowerError;
            } while (iterations <= MaximumIterations && !accurateEnough);

            return voltages;
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

        private static double CalculateVoltageChange(Vector<Complex> newVoltages, Vector<Complex> voltages)
        {
            var voltageDifference = newVoltages.Subtract(voltages);
            var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
            var voltageChange = maximumVoltageDifference.Magnitude;
            return voltageChange;
        }

        private Vector<Complex> CalculateImprovedVoltagesAndPowers(IReadOnlyAdmittanceMatrix admittances, IList<Complex> constantCurrents, IEnumerable<PvNodeWithIndex> pvBuses, Vector<Complex> rightHandSide, IList<Complex> powers, Vector<Complex> oldVoltages, out bool powerErrorTooBig)
        {
            Vector<Complex> newVoltages;
            powerErrorTooBig = false;

            if (IterativeSolver)
            {
                newVoltages = new DenseVector(oldVoltages.Count);
                oldVoltages.CopyTo(newVoltages);
                admittances.CalculateVoltages(newVoltages, rightHandSide, new BiCgStab(), new Iterator<Complex>());
            }
            else
                newVoltages = _factorization.Solve(rightHandSide);

            foreach (var bus in pvBuses)
            {
                var newVoltage = newVoltages[bus.Index];
                newVoltage = Complex.FromPolarCoordinates(bus.VoltageMagnitude, newVoltage.Phase);
                newVoltages[bus.Index] = newVoltage;
                var newPower = CalculatePower(bus.Index, admittances, constantCurrents, newVoltages);

                if (Math.Abs((newPower.Real - bus.RealPower) / bus.RealPower) > TargetPrecision)
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