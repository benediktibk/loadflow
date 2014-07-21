using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public class LoadFlowCalculator
    {
        private readonly double _maximumPowerError;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public LoadFlowCalculator(INodeVoltageCalculator nodeVoltageCalculator)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
            _maximumPowerError = nodeVoltageCalculator.GetMaximumPowerError();
        }

        /// <summary>
        /// calculates the missing node voltages and powers
        /// </summary>
        /// <param name="admittances"></param>
        /// <param name="nominalVoltage"></param>
        /// <param name="nodes"></param>
        /// <param name="voltageCollapse">the relability of this param depends on the used method, e.g. for CurrentIteration it could also just mean that the solution did not converge</param>
        /// <returns></returns>
        public IList<Node> CalculateNodeVoltagesAndPowers(AdmittanceMatrix admittances, double nominalVoltage, IList<Node> nodes, out bool voltageCollapse)
        {
            if (admittances.NodeCount != nodes.Count())
                throw new ArgumentOutOfRangeException("nodes",
                    "the count of nodes does not match the dimensions of the admittance matrix");

            List<int> indexOfSlackBuses;
            List<int> indexOfPQBuses;
            List<int> indexOfPVBuses;
            SeperateNodesInBusTypes(nodes, out indexOfSlackBuses,
                out indexOfPQBuses, out indexOfPVBuses);

            var countOfKnownVoltages = indexOfSlackBuses.Count;
            var countOfUnknownVoltages = indexOfPQBuses.Count + indexOfPVBuses.Count;

            var indexOfNodesWithUnknownVoltage = new List<int>(countOfUnknownVoltages);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPQBuses);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPVBuses);

            if (countOfKnownVoltages == 0)
                throw new ArgumentOutOfRangeException("nodes", "there must be at least one slack bus");

            Vector<Complex> allVoltages;
            if (countOfUnknownVoltages == 0)
                allVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
            else
            {
                var knownVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
                var pqBuses = ExtractPQBuses(nodes, indexOfPQBuses);
                var pvBuses = ExtractPVBuses(nodes, indexOfPVBuses, pqBuses.Count);
                Vector<Complex> constantCurrentRightHandSide;
                var admittancesToUnknownVoltages = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnknownVoltage, indexOfSlackBuses, knownVoltages, out constantCurrentRightHandSide);
                var totalAdmittanceRowSums = admittances.CalculateRowSums();

                var unknownVoltages = _nodeVoltageCalculator.CalculateUnknownVoltages(admittancesToUnknownVoltages, totalAdmittanceRowSums,
                    nominalVoltage, constantCurrentRightHandSide, pqBuses, pvBuses);

                allVoltages = CombineKnownAndUnknownVoltages(indexOfSlackBuses, knownVoltages,
                    indexOfNodesWithUnknownVoltage, unknownVoltages);
            }

            var allPowers = CalculateAllPowers(admittances, allVoltages);
            voltageCollapse = false;
            var inputPowerSum = new Complex();

            foreach (var index in indexOfPQBuses)
            {
                var power = nodes[index].Power;
                inputPowerSum += power;
                allPowers[index] = power;
            }

            foreach (var index in indexOfPVBuses)
            {
                var power = new Complex(nodes[index].RealPower, allPowers[index].Imaginary);
                inputPowerSum += power;
                allPowers[index] = power;
                allVoltages[index] = Complex.FromPolarCoordinates(nodes[index].VoltageMagnitude,
                    allVoltages[index].Phase);
            }

            foreach (var index in indexOfSlackBuses)
            {
                var power = allPowers[index];
                inputPowerSum += power;
                allVoltages[index] = nodes[index].Voltage;
            }

            var absolutePowerSum = allPowers.Sum(power => Math.Abs((double) power.Real) + Math.Abs((double) power.Imaginary));
            var lossPowerSum = CalculatePowerLoss(admittances, allVoltages);
            var relativePowerError = (lossPowerSum - inputPowerSum).Magnitude / absolutePowerSum;

            if (relativePowerError > _maximumPowerError || Double.IsNaN(relativePowerError) || Double.IsInfinity(relativePowerError))
                voltageCollapse = true;

            return CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
        }

        public static Complex CalculatePowerLoss(AdmittanceMatrix admittances, Vector<Complex> allVoltages)
        {
            var powerLoss = new Complex();

            for (var i = 0; i < admittances.NodeCount; ++i)
                for (var j = i + 1; j < admittances.NodeCount; ++j)
                {
                    var admittance = admittances[i, j];
                    var voltageDifference = allVoltages[i] - allVoltages[j];
                    var branchCurrent = admittance*voltageDifference;
                    var branchPowerLoss = voltageDifference*branchCurrent.Conjugate();
                    powerLoss += branchPowerLoss;
                }

            return powerLoss*(-1);
        }

        private static Node[] CombineVoltagesAndPowersToNodes(IList<Complex> allPowers, IList<Complex> allVoltages)
        {
            if (allPowers.Count != allVoltages.Count)
                throw new ArgumentOutOfRangeException();

            var nodeCount = allPowers.Count;
            var result = new Node[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
            {
                var node = new Node
                {
                    Power = allPowers[i],
                    Voltage = allVoltages[i]
                };
                result[i] = node;
            }

            return result;
        }

        public static double CalculatePowerError(AdmittanceMatrix admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var powers = CalculateAllPowers(admittances, voltages, constantCurrents);
            double sum = 0;

            foreach (var bus in pqBuses)
            {
                var id = bus.ID;
                var power = bus.Power;
                sum += Math.Abs(power.Real - powers[id].Real);
                sum += Math.Abs(power.Imaginary - powers[id].Imaginary);
            }

            foreach (var bus in pvBuses)
            {
                var id = bus.ID;
                var power = bus.RealPower;
                sum += Math.Abs(power - powers[id].Real);
            }

            return sum;
        }

        public static Vector<Complex> CalculateAllPowers(AdmittanceMatrix admittances, Vector<Complex> allVoltages)
        {
            var currents = admittances.CalculateCurrents(allVoltages);
            var allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        public static Vector<Complex> CalculateAllPowers(AdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents)
        {
            var currents = admittances.CalculateCurrents(voltages) - constantCurrents;
            var powers = voltages.PointwiseMultiply(currents.Conjugate());
            return powers;
        }

        public static Vector<Complex> CombineKnownAndUnknownVoltages(IReadOnlyList<int> indexOfNodesWithKnownVoltage,
            Vector<Complex> knownVoltages,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage, Vector<Complex> unknownVoltages)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var nodeCount = countOfKnownVoltages + countOfUnknownVoltages;
            var voltagesArray = new Complex[nodeCount];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                voltagesArray[indexOfNodesWithKnownVoltage[i]] = knownVoltages.At(i);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                voltagesArray[indexOfNodesWithUnknownVoltage[i]] = unknownVoltages.At(i);

            var allVoltages = new DenseVector(voltagesArray);
            return allVoltages;
        }

        private static List<PQBus> ExtractPQBuses(IList<Node> nodes,
            IEnumerable<int> indexes)
        {
            var result = new List<PQBus>(nodes.Count);
            var newIndex = 0;

            foreach (var index in indexes)
            {
                if (!nodes[index].IsPQBus)
                    throw new ArgumentOutOfRangeException("indexes", "selected node is not a PQ-bus");

                result.Add(new PQBus(newIndex, nodes[index].Power));
                ++newIndex;
            }

            return result;
        }

        private static List<PVBus> ExtractPVBuses(IList<Node> nodes,
            IEnumerable<int> indexes, int countOfPQBuses)
        {
            var result = new List<PVBus>(nodes.Count);
            var newIndex = countOfPQBuses;

            foreach (var index in indexes)
            {
                if (!nodes[index].IsPVBus)
                    throw new ArgumentOutOfRangeException("indexes", "selected node is not a PV-bus");

                result.Add(new PVBus(newIndex, nodes[index].RealPower, nodes[index].VoltageMagnitude));
                ++newIndex;
            }

            return result;
        }

        private static Vector<Complex> ExtractKnownVoltages(IList<Node> nodes,
            IReadOnlyList<int> indexes)
        {
            var countOfKnownVoltages = indexes.Count;
            var knownVoltages = new DenseVector(countOfKnownVoltages);

            for (var i = 0; i < countOfKnownVoltages; ++i)
                knownVoltages[i] = nodes[indexes[i]].Voltage;

            return knownVoltages;
        }

        private static void SeperateNodesInBusTypes(IList<Node> nodes,
            out List<int> indexOfSlackBuses, out List<int> indexOfPQBuses, out List<int> indexOfPVBuses)
        {
            indexOfSlackBuses = new List<int>();
            indexOfPQBuses = new List<int>();
            indexOfPVBuses = new List<int>();

            for (var i = 0; i < nodes.Count(); ++i)
            {
                var node = nodes[i];

                if (node.IsSlackBus)
                    indexOfSlackBuses.Add(i);
                else if (node.IsPQBus)
                    indexOfPQBuses.Add(i);
                else if (node.IsPVBus)
                    indexOfPVBuses.Add(i);
                else
                    throw new ArgumentOutOfRangeException("nodes",
                        "invalid bus type (neither PV, PQ or slack bus)");
            }
        }
    }
}