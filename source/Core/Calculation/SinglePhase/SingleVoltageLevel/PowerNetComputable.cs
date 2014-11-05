using System;
using System.Collections.Generic;
using System.IO;
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
            if (Nodes.Count != Admittances.NodeCount)
                throw new InvalidDataException("the node count does not match the size of the admittance matrix");

            List<NodeWithIndex> indexOfSlackBuses;
            List<NodeWithIndex> indexOfPqBuses;
            List<NodeWithIndex> indexOfPvBuses;
            SeperateNodesInBusTypes(Nodes, out indexOfSlackBuses,
                out indexOfPqBuses, out indexOfPvBuses);

            var countOfKnownVoltages = indexOfSlackBuses.Count;
            var countOfUnknownVoltages = indexOfPqBuses.Count + indexOfPvBuses.Count;

            var indexOfNodesWithUnknownVoltage = new List<int>(countOfUnknownVoltages);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPqBuses.Select(x => x.Index));
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPvBuses.Select(x => x.Index));

            if (countOfKnownVoltages == 0)
                throw new ArgumentException("there must be at least one slack bus");

            var allVoltages = countOfUnknownVoltages == 0 ?
                ExtractKnownVoltages(indexOfSlackBuses) :
                CalculateUnknownVoltages(Admittances, NominalVoltage, indexOfSlackBuses, indexOfPqBuses, indexOfPvBuses, indexOfNodesWithUnknownVoltage, countOfUnknownVoltages);

            var allPowers = DeterminePowers(Admittances, allVoltages, indexOfPqBuses, indexOfPvBuses);
            allVoltages = DetermineFixedVoltages(allVoltages, indexOfPvBuses, indexOfSlackBuses);
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
            Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var powers = CalculateAllPowers(admittances, voltages, constantCurrents);
            return 
                pqBuses.Sum(bus => Math.Abs(bus.Power.Real - powers[bus.Index].Real) + Math.Abs(bus.Power.Imaginary - powers[bus.Index].Imaginary)) + 
                pvBuses.Sum(bus => Math.Abs(bus.RealPower - powers[bus.Index].Real));
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

        public static Vector<Complex> CombineKnownAndUnknownVoltages(List<int> slackNodes,
            Vector<Complex> knownVoltages,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage, Vector<Complex> unknownVoltages)
        {
            var countOfKnownVoltages = slackNodes.Count;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var nodeCount = countOfKnownVoltages + countOfUnknownVoltages;
            var voltagesArray = new Complex[nodeCount];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                voltagesArray[slackNodes[i]] = knownVoltages.At(i);

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

        private static List<PqNodeWithIndex> ExtractPqNodesWithIndices(IReadOnlyCollection<NodeWithIndex> nodesWithIndices)
        {
            var result = new List<PqNodeWithIndex>(nodesWithIndices.Count);
            var newIndex = 0;

            foreach (var nodeWithIndex in nodesWithIndices)
            {
                nodeWithIndex.Node.AddTo(result, newIndex);
                ++newIndex;
            }

            return result;
        }

        private static List<PvNodeWithIndex> ExtractPvNodesWithIndices(IReadOnlyCollection<NodeWithIndex> nodeWithIndices, int countOfPqBuses)
        {
            var result = new List<PvNodeWithIndex>(nodeWithIndices.Count);
            var newIndex = countOfPqBuses;

            foreach (var nodeWithIndex in nodeWithIndices)
            {
                nodeWithIndex.Node.AddTo(result, newIndex);
                ++newIndex;
            }

            return result;
        }

        private static Vector<Complex> ExtractKnownVoltages(IReadOnlyCollection<NodeWithIndex> slackNodes)
        {
            var countOfKnownVoltages = slackNodes.Count;
            var knownVoltages = new DenseVector(countOfKnownVoltages);
            var index = 0;

            foreach (var node in slackNodes)
            {
                node.Node.SetVoltageIn(knownVoltages, index);
                ++index;
            }

            return knownVoltages;
        }

        private static void SeperateNodesInBusTypes(IReadOnlyList<INode> nodes,
            out List<NodeWithIndex> slackNodes, out List<NodeWithIndex> pqNodes, out List<NodeWithIndex> pvNodes)
        {
            slackNodes = new List<NodeWithIndex>();
            pqNodes = new List<NodeWithIndex>();
            pvNodes = new List<NodeWithIndex>();

            for (var i = 0; i < nodes.Count(); ++i)
                nodes[i].AddTo(slackNodes, pqNodes, pvNodes, i);
        }

        private static DenseVector DetermineFixedVoltages(Vector<Complex> allVoltages, IEnumerable<NodeWithIndex> pvNodes, IEnumerable<NodeWithIndex> slackNodes)
        {
            var allVoltagesFixed = DenseVector.OfVector(allVoltages);

            foreach (var node in pvNodes)
                node.SetVoltageMagnitudeIn(allVoltagesFixed);

            foreach (var node in slackNodes)
                node.SetVoltageIn(allVoltagesFixed);

            return allVoltagesFixed;
        }

        private static Vector<Complex> DeterminePowers(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> allVoltages, IEnumerable<NodeWithIndex> pqNodes, IEnumerable<NodeWithIndex> pvNodes)
        {
            var allPowers = CalculateAllPowers(admittances, allVoltages);

            foreach (var node in pqNodes)
                node.SetPowerIn(allPowers);

            foreach (var node in pvNodes)
                node.SetRealPowerIn(allPowers);

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

        private Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyCollection<NodeWithIndex> slackNodes, IReadOnlyCollection<NodeWithIndex> pqNodes, IReadOnlyCollection<NodeWithIndex> pvNodes, IReadOnlyList<int> indexOfNodesWithUnknownVoltage, int countOfUnknownVoltages)
        {
            var knownVoltages = ExtractKnownVoltages(slackNodes);
            var pqNodeWithIndices = ExtractPqNodesWithIndices(pqNodes);
            var pvNodeWithIndices = ExtractPvNodesWithIndices(pvNodes, pqNodeWithIndices.Count);
            Vector<Complex> constantCurrentRightHandSide;
            var indexOfNodesWithKnownVoltage = slackNodes.Select(x => x.Index).ToList();
            var admittancesToUnknownVoltages = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnknownVoltage,
                indexOfNodesWithKnownVoltage, knownVoltages, out constantCurrentRightHandSide);
            var totalAdmittanceRowSums = admittances.CalculateRowSums();
            var initialVoltages = CalculateNominalVoltages(nominalVoltage, countOfUnknownVoltages,
                indexOfNodesWithUnknownVoltage);

            var unknownVoltages = _nodeVoltageCalculator.CalculateUnknownVoltages(admittancesToUnknownVoltages,
                totalAdmittanceRowSums, nominalVoltage,
                initialVoltages, constantCurrentRightHandSide, pqNodeWithIndices, pvNodeWithIndices);

            var result = CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnknownVoltage, unknownVoltages);
            return result;
        }
    }
}