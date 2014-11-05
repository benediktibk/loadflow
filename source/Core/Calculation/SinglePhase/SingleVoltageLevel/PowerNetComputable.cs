using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNetComputable : PowerNet, IPowerNetComputable
    {
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public PowerNetComputable(INodeVoltageCalculator nodeVoltageCalculator, IAdmittanceMatrix admittances, double nominalVoltage) : base(admittances, nominalVoltage)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public INodeVoltageCalculator NodeVoltageCalculator
        {
             get { return _nodeVoltageCalculator; }
        }

        public IList<NodeResult> CalculateNodeResults()
        {
            var nodes = Nodes;
            List<int> indexOfSlackBuses;
            List<int> indexOfPqBuses;
            List<int> indexOfPvBuses;
            SeperateNodesInBusTypes(nodes, out indexOfSlackBuses,
                out indexOfPqBuses, out indexOfPvBuses);

            var countOfKnownVoltages = indexOfSlackBuses.Count;
            var countOfUnknownVoltages = indexOfPqBuses.Count + indexOfPvBuses.Count;

            var indexOfNodesWithUnknownVoltage = new List<int>(countOfUnknownVoltages);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPqBuses);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPvBuses);

            if (countOfKnownVoltages == 0)
                throw new ArgumentException("there must be at least one slack bus");

            var allVoltages = countOfUnknownVoltages == 0 ?
                ExtractKnownVoltages(nodes, indexOfSlackBuses) :
                CalculateUnknownVoltages(Admittances, NominalVoltage, nodes, indexOfSlackBuses, indexOfPqBuses, indexOfPvBuses, indexOfNodesWithUnknownVoltage, countOfUnknownVoltages);

            var allPowers = DeterminePowers(Admittances, nodes, allVoltages, indexOfPqBuses, indexOfPvBuses);
            allVoltages = DetermineFixedVoltages(nodes, allVoltages, indexOfPvBuses, indexOfSlackBuses);
            var voltageCollapse = CheckForVoltageCollapse(Admittances, allPowers, allVoltages);
            var nodeResults = CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
            return voltageCollapse ? null : nodeResults;
        }

        public static Complex CalculatePowerLoss(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> allVoltages)
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

        public static double CalculatePowerError(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, IList<PqBus> pqBuses, IList<PvBus> pvBuses)
        {
            var powers = CalculateAllPowers(admittances, voltages, constantCurrents);
            double sum = 0;

            foreach (var bus in pqBuses)
            {
                var id = bus.Id;
                var power = bus.Power;
                sum += Math.Abs(power.Real - powers[id].Real);
                sum += Math.Abs(power.Imaginary - powers[id].Imaginary);
            }

            foreach (var bus in pvBuses)
            {
                var id = bus.Id;
                var power = bus.RealPower;
                sum += Math.Abs(power - powers[id].Real);
            }

            return sum;
        }

        public static Vector<Complex> CalculateAllPowers(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> allVoltages)
        {
            var currents = admittances.CalculateCurrents(allVoltages);
            var allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        public static Vector<Complex> CalculateAllPowers(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents)
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

        private static IList<NodeResult> CombineVoltagesAndPowersToNodes(IList<Complex> allPowers, IList<Complex> allVoltages)
        {
            var nodeCount = allPowers.Count;
            var result = new NodeResult[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                result[i] = new NodeResult(allVoltages[i], allPowers[i]);

            return result;
        }

        private static List<PqBus> ExtractPqBuses(IReadOnlyList<IReadOnlyNode> nodes,
            IEnumerable<int> indexes)
        {
            var result = new List<PqBus>(nodes.Count);
            var newIndex = 0;

            foreach (var index in indexes)
            {
                nodes[index].AddTo(result, newIndex);
                ++newIndex;
            }

            return result;
        }

        private static List<PvBus> ExtractPvBuses(IReadOnlyList<IReadOnlyNode> nodes,
            IEnumerable<int> indexes, int countOfPqBuses)
        {
            var result = new List<PvBus>(nodes.Count);
            var newIndex = countOfPqBuses;

            foreach (var index in indexes)
            {
                nodes[index].AddTo(result, newIndex);
                ++newIndex;
            }

            return result;
        }

        private static Vector<Complex> ExtractKnownVoltages(IReadOnlyList<IReadOnlyNode> nodes,
            IReadOnlyList<int> indexes)
        {
            var countOfKnownVoltages = indexes.Count;
            var knownVoltages = new DenseVector(countOfKnownVoltages);

            for (var i = 0; i < countOfKnownVoltages; ++i)
                nodes[indexes[i]].SetValueIn(knownVoltages, i);

            return knownVoltages;
        }

        private static void SeperateNodesInBusTypes(IReadOnlyList<IReadOnlyNode> nodes,
            out List<int> indexOfSlackBuses, out List<int> indexOfPQBuses, out List<int> indexOfPVBuses)
        {
            indexOfSlackBuses = new List<int>();
            indexOfPQBuses = new List<int>();
            indexOfPVBuses = new List<int>();

            for (var i = 0; i < nodes.Count(); ++i)
                nodes[i].AddTo(indexOfSlackBuses, indexOfPQBuses, indexOfPVBuses, i);
        }

        private static DenseVector DetermineFixedVoltages(IReadOnlyList<IReadOnlyNode> nodes, Vector<Complex> allVoltages, IEnumerable<int> indexOfPVBuses,
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

        private static Vector<Complex> DeterminePowers(IReadOnlyAdmittanceMatrix admittances, IReadOnlyList<IReadOnlyNode> nodes, Vector<Complex> allVoltages,
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

        private static Vector<Complex> CalculateNominalVoltages(double nominalVoltage, int countOfUnknownVoltages, IReadOnlyCollection<int> indexOfNodesWithUnknownVoltage)
        {
            var nominalVoltages = new DenseVector(countOfUnknownVoltages);

            for (var i = 0; i < indexOfNodesWithUnknownVoltage.Count; ++i)
                nominalVoltages[i] = Complex.FromPolarCoordinates(nominalVoltage, 0);

            return nominalVoltages;
        }

        private bool CheckForVoltageCollapse(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> allPowers, Vector<Complex> allVoltages)
        {
            var inputPowerSum = allPowers.Sum();
            var absolutePowerSum = allPowers.Sum(power => Math.Abs(power.Real) + Math.Abs(power.Imaginary));
            var lossPowerSum = CalculatePowerLoss(admittances, allVoltages);
            var absolutPowerError = (lossPowerSum - inputPowerSum).Magnitude;
            var relativePowerError = absolutePowerSum > 1e-10 ? absolutPowerError / absolutePowerSum : absolutPowerError;

            var result = relativePowerError > _nodeVoltageCalculator.MaximumRelativePowerError || Double.IsNaN(relativePowerError) ||
                         Double.IsInfinity(relativePowerError);
            return result;
        }

        private Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyList<IReadOnlyNode> nodes,
            IReadOnlyList<int> indexOfSlackBuses, IEnumerable<int> indexOfPqBuses, IEnumerable<int> indexOfPvBuses, IReadOnlyList<int> indexOfNodesWithUnknownVoltage,
            int countOfUnknownVoltages)
        {
            var knownVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
            var pqBuses = ExtractPqBuses(nodes, indexOfPqBuses);
            var pvBuses = ExtractPvBuses(nodes, indexOfPvBuses, pqBuses.Count);
            Vector<Complex> constantCurrentRightHandSide;
            var admittancesToUnknownVoltages = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnknownVoltage,
                indexOfSlackBuses, knownVoltages, out constantCurrentRightHandSide);
            var totalAdmittanceRowSums = admittances.CalculateRowSums();
            var initialVoltages = CalculateNominalVoltages(nominalVoltage, countOfUnknownVoltages,
                indexOfNodesWithUnknownVoltage);

            var unknownVoltages = _nodeVoltageCalculator.CalculateUnknownVoltages(admittancesToUnknownVoltages,
                totalAdmittanceRowSums, nominalVoltage,
                initialVoltages, constantCurrentRightHandSide, pqBuses, pvBuses);

            var result = CombineKnownAndUnknownVoltages(indexOfSlackBuses, knownVoltages,
                indexOfNodesWithUnknownVoltage, unknownVoltages);
            return result;
        }
    }
}