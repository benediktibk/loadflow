using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class LoadFlowCalculator
    {
        #region variables

        private readonly double _maximumPowerError;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        #endregion

        #region constructor

        public LoadFlowCalculator(INodeVoltageCalculator nodeVoltageCalculator)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
            _maximumPowerError = nodeVoltageCalculator.GetMaximumPowerError();
        }

        #endregion

        #region public functions

        /// <summary>
        /// calculates the missing node voltages and powers
        /// </summary>
        /// <param id="admittances"></param>
        /// <param id="nominalVoltage"></param>
        /// <param id="nodes"></param>
        /// <param id="voltageCollapse">the relability of this param depends on the used method, e.g. for CurrentIteration it could also just mean that the solution did not converge</param>
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

            var allVoltages = countOfUnknownVoltages == 0 ? 
                ExtractKnownVoltages(nodes, indexOfSlackBuses) : 
                CalculateUnknownVoltages(admittances, nominalVoltage, nodes, indexOfSlackBuses, indexOfPQBuses, indexOfPVBuses, indexOfNodesWithUnknownVoltage, countOfUnknownVoltages);

            var allPowers = DeterminePowers(admittances, nodes, allVoltages, indexOfPQBuses, indexOfPVBuses);
            allVoltages = DetermineFixedVoltages(nodes, allVoltages, indexOfPVBuses, indexOfSlackBuses);
            voltageCollapse = CheckForVoltageCollapse(admittances, allPowers, allVoltages);

            return CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
        }

        #endregion

        #region public static functions

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

        #endregion

        #region private static functions

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

        private static DenseVector DetermineFixedVoltages(IList<Node> nodes, Vector<Complex> allVoltages, IEnumerable<int> indexOfPVBuses,
            IEnumerable<int> indexOfSlackBuses)
        {
            var allVoltagesFixed = DenseVector.OfVector(allVoltages);

            foreach (var index in indexOfPVBuses)
            {
                allVoltagesFixed[index] = Complex.FromPolarCoordinates(nodes[index].VoltageMagnitude,
                    allVoltagesFixed[index].Phase);
            }

            foreach (var index in indexOfSlackBuses)
            {
                allVoltagesFixed[index] = nodes[index].Voltage;
            }
            return allVoltagesFixed;
        }

        private static Vector<Complex> DeterminePowers(AdmittanceMatrix admittances, IList<Node> nodes, Vector<Complex> allVoltages,
            IEnumerable<int> indexOfPQBuses, IEnumerable<int> indexOfPVBuses)
        {
            var allPowers = CalculateAllPowers(admittances, allVoltages);
            foreach (var index in indexOfPQBuses)
            {
                var power = nodes[index].Power;
                allPowers[index] = power;
            }

            foreach (var index in indexOfPVBuses)
            {
                var power = new Complex(nodes[index].RealPower, allPowers[index].Imaginary);
                allPowers[index] = power;
            }
            return allPowers;
        }

        private static IReadOnlyList<Complex> CalculateNominalVoltages(double nominalVoltage, IList<Node> nodes, int countOfUnknownVoltages,
            IEnumerable<int> indexOfNodesWithUnknownVoltage)
        {
            var nominalVoltages = new List<Complex>(countOfUnknownVoltages);

            foreach (var index in indexOfNodesWithUnknownVoltage)
            {
                var node = nodes[index];
                nominalVoltages.Add(Complex.FromPolarCoordinates(nominalVoltage, node.NominalPhaseShift));
            }

            return nominalVoltages;
        }

        #endregion

        #region private functions

        private bool CheckForVoltageCollapse(AdmittanceMatrix admittances, Vector<Complex> allPowers, Vector<Complex> allVoltages)
        {
            var inputPowerSum = allPowers.Sum();
            var absolutePowerSum = allPowers.Sum(power => Math.Abs(power.Real) + Math.Abs(power.Imaginary));
            var lossPowerSum = CalculatePowerLoss(admittances, allVoltages);
            var absolutPowerError = (lossPowerSum - inputPowerSum).Magnitude;
            var relativePowerError = absolutePowerSum > 1e-10 ? absolutPowerError / absolutePowerSum : absolutPowerError;

            var result = relativePowerError > _maximumPowerError || Double.IsNaN(relativePowerError) ||
                         Double.IsInfinity(relativePowerError);
            return result;
        }

        private Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, double nominalVoltage, IList<Node> nodes,
            IReadOnlyList<int> indexOfSlackBuses, IEnumerable<int> indexOfPQBuses, IEnumerable<int> indexOfPVBuses, IReadOnlyList<int> indexOfNodesWithUnknownVoltage,
            int countOfUnknownVoltages)
        {
            var knownVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
            var pqBuses = ExtractPQBuses(nodes, indexOfPQBuses);
            var pvBuses = ExtractPVBuses(nodes, indexOfPVBuses, pqBuses.Count);
            Vector<Complex> constantCurrentRightHandSide;
            var admittancesToUnknownVoltages = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnknownVoltage,
                indexOfSlackBuses, knownVoltages, out constantCurrentRightHandSide);
            var totalAdmittanceRowSums = admittances.CalculateRowSums();
            var nominalVoltages = CalculateNominalVoltages(nominalVoltage, nodes, countOfUnknownVoltages,
                indexOfNodesWithUnknownVoltage);

            var unknownVoltages = _nodeVoltageCalculator.CalculateUnknownVoltages(admittancesToUnknownVoltages,
                totalAdmittanceRowSums, nominalVoltage,
                nominalVoltages, constantCurrentRightHandSide, pqBuses, pvBuses);

            var result = CombineKnownAndUnknownVoltages(indexOfSlackBuses, knownVoltages,
                indexOfNodesWithUnknownVoltage, unknownVoltages);
            return result;
        }

        #endregion
    }
}