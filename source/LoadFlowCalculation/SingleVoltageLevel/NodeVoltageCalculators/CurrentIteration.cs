﻿using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators
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

        public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses,
            Vector<Complex> initialVoltages)
        {
            var powers = new DenseVector(admittances.RowCount);
            var voltages = initialVoltages;

            foreach (var bus in pqBuses)
                powers[bus.ID] = bus.Power;

            foreach (var bus in pvBuses)
                powers[bus.ID] = new Complex(bus.RealPower, 0);

            var iterations = 0;
            double voltageChange;
            bool powerErrorTooBig;

            do
            {
                powerErrorTooBig = false;
                var newVoltages = DenseVector.OfVector(voltages);

                foreach (var bus in pqBuses)
                {
                    var newVoltage = CalculateVoltage(bus.ID, admittances, constantCurrents, powers, newVoltages);
                    newVoltages[bus.ID] = newVoltage;
                }

                foreach (var bus in pvBuses)
                {
                    var newVoltage = CalculateVoltage(bus.ID, admittances, constantCurrents, powers, newVoltages);
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
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && (voltageChange/nominalVoltage > _targetPrecision/10 || powerErrorTooBig));

            return voltages;
        }

        public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var initialVoltageCalculator = new NodePotentialMethod();
            var initialVoltages = initialVoltageCalculator.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, constantCurrents, pqBuses, pvBuses);

            return CalculateUnknownVoltages(admittances, nominalVoltage, constantCurrents, pqBuses, pvBuses,
                initialVoltages);
        }

        public double GetMaximumPowerError()
        {
            return 0.1;
        }

        private Complex CalculateVoltage(int i, Matrix<Complex> admittances, IList<Complex> constantCurrents, IList<Complex> powers,
            IList<Complex> previousVoltages)
        {
            var constantCurrent = constantCurrents[i];
            var admittance = admittances[i, i];
            var previousVoltage = previousVoltages[i];
            var power = powers[i];

            var branchCurrentSum = new Complex();
            
            for (var k = 0; k < admittances.RowCount; ++k)
                if (k != i)
                {
                    var branchAdmittance = admittances[i, k];
                    var branchVoltage = previousVoltages[k];
                    var branchCurrent = branchAdmittance*branchVoltage;
                    branchCurrentSum += branchCurrent;
                }

            var totalCurrents = constantCurrent + (power/previousVoltage).Conjugate() - branchCurrentSum;
            return totalCurrents/admittance;
        }

        private Complex CalculatePower(int i, Matrix<Complex> admittances, IList<Complex> constantCurrents,
            Vector<Complex> voltages)
        {
            var voltage = voltages[i];
            var constantCurrent = constantCurrents[i];
            var branchAdmittances = admittances.Row(i);
            var branchCurrent = (branchAdmittances.PointwiseMultiply(voltages)).Sum();
            var totalCurrents = branchCurrent - constantCurrent;
            return voltage*totalCurrents.Conjugate();
        }
    }
}