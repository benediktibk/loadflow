using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
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
            var powers = new DenseVector(admittances.NodeCount);
            var voltages = initialVoltages;
            IList<PQBus> normalPqBusses;
            IList<PQBus> currentControlledPqBusses;
            IList<int> rowsForCurrentControlledPqBusses;
            ExtractCurrentControlledPqBusses(pqBuses, admittances, out normalPqBusses, out currentControlledPqBusses,
                out rowsForCurrentControlledPqBusses);


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

                foreach (var bus in normalPqBusses)
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

                for (var i = 0; i < currentControlledPqBusses.Count; ++i)
                {
                    var rowIndex = rowsForCurrentControlledPqBusses[i];
                    var bus = currentControlledPqBusses[i];
                    var busId = bus.ID;

                    var newVoltage = CalculateCurrentControlledVoltage(busId, admittances, constantCurrents, powers,
                        newVoltages, rowIndex);
                    newVoltages[busId] = newVoltage;
                }

                var voltageDifference = newVoltages.Subtract(voltages);
                var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
                voltageChange = maximumVoltageDifference.Magnitude / nominalVoltage;
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && (voltageChange/nominalVoltage > _targetPrecision/10 || powerErrorTooBig));

            return voltages;
        }

        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
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

        private void ExtractCurrentControlledPqBusses(IEnumerable<PQBus> allBusses, AdmittanceMatrix admittances, out IList<PQBus> normalBusses,
            out IList<PQBus> currentControlledBusses, out IList<int> rowsForCurrentControlledBusses)
        {
            normalBusses = new List<PQBus>();
            currentControlledBusses = new List<PQBus>();
            rowsForCurrentControlledBusses = new List<int>();
            var currentControlledBusIDs = new HashSet<int>();

            foreach (var bus in allBusses)
            {
                var mainAdmittance = admittances[bus.ID, bus.ID];

                if (mainAdmittance.Magnitude != 0)
                    normalBusses.Add(bus);
                else
                {
                    currentControlledBusses.Add(bus);
                    var maximumAdmittance = 0.0;
                    var maximumRow = -1;

                    for (var row = 0; row < admittances.NodeCount; ++row)
                    {
                        var currentAdmittance = admittances[row, bus.ID];
                        var magnitude = currentAdmittance.MagnitudeSquared();

                        if (magnitude < maximumAdmittance || currentControlledBusIDs.Contains(row)) 
                            continue;

                        maximumRow = row;
                        maximumAdmittance = magnitude;
                    }

                    if (maximumRow < 0)
                        throw new InvalidDataException("admittance matrix seems to be singular");

                    rowsForCurrentControlledBusses.Add(maximumRow);
                    currentControlledBusIDs.Add(bus.ID);
                }
            }
        }

        private Complex CalculateVoltage(int i, AdmittanceMatrix admittances, IList<Complex> constantCurrents, IList<Complex> powers,
            IList<Complex> previousVoltages)
        {
            var constantCurrent = constantCurrents[i];
            var admittance = admittances[i, i];
            var previousVoltage = previousVoltages[i];
            var power = powers[i];

            if (admittance.Magnitude < 0.00001)
                throw new InvalidDataException("admittance on main diagonal is zero");

            var branchCurrentSum = new Complex();
            
            for (var k = 0; k < admittances.NodeCount; ++k)
                if (k != i)
                {
                    var branchAdmittance = admittances[i, k];
                    var branchVoltage = previousVoltages[k];
                    var branchCurrent = branchAdmittance*branchVoltage;
                    branchCurrentSum += branchCurrent;
                }

            var totalCurrents = constantCurrent + (power/previousVoltage).Conjugate() - branchCurrentSum;
            return totalCurrents / admittance;
        }

        private Complex CalculateCurrentControlledVoltage(int i, AdmittanceMatrix admittances,
            IList<Complex> constantCurrents, IList<Complex> powers,
            IList<Complex> previousVoltages, int rowIndex)
        {
            var constantCurrent = constantCurrents[rowIndex];
            var previousVoltage = previousVoltages[rowIndex];
            var mainAdmittance = admittances[rowIndex, i];
            var power = powers[rowIndex];
            var row = admittances.GetRow(rowIndex);
            var branchCurrentSum = new Complex(0, 0);

            Debug.Assert(mainAdmittance.MagnitudeSquared() > 0);

            foreach (var value in row.GetIndexedEnumerator())
            {
                var columnIndex = value.Item1;

                if (columnIndex == i)
                    continue;

                var admittance = value.Item2;
                branchCurrentSum += admittance*previousVoltages[columnIndex];
            }

            var totalCurrents = constantCurrent + (power / previousVoltage).Conjugate() - branchCurrentSum;
            return totalCurrents / mainAdmittance;
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