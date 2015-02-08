using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNetComputable : PowerNet, IPowerNetComputable
    {
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public PowerNetComputable(INodeVoltageCalculator nodeVoltageCalculator, IAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyList<Complex> constantCurrents)
            : base(admittances, nominalVoltage, constantCurrents)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public INodeVoltageCalculator NodeVoltageCalculator
        {
             get { return _nodeVoltageCalculator; }
        }

        public IList<NodeResult> CalculateNodeResults(out double relativePowerError)
        {
            List<NodeWithIndex> indexOfSlackBuses;
            List<NodeWithIndex> indexOfPqBuses;
            List<NodeWithIndex> indexOfPvBuses;
            SeperateNodesByTypes(out indexOfSlackBuses,
                out indexOfPqBuses, out indexOfPvBuses);

            var countOfKnownVoltages = indexOfSlackBuses.Count;
            var countOfUnknownVoltages = indexOfPqBuses.Count + indexOfPvBuses.Count;

            var indexOfNodesWithUnknownVoltage = new List<int>(countOfUnknownVoltages);
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPqBuses.Select(x => x.Index));
            indexOfNodesWithUnknownVoltage.AddRange(indexOfPvBuses.Select(x => x.Index));

            if (countOfKnownVoltages == 0)
            {
                relativePowerError = 0;
                return CreateEmptyNodeResults(countOfUnknownVoltages);
            }

            var allVoltages = countOfUnknownVoltages == 0 ?
                ExtractKnownVoltages(indexOfSlackBuses) :
                CalculateUnknownVoltages(indexOfSlackBuses, indexOfPqBuses, indexOfPvBuses, indexOfNodesWithUnknownVoltage, countOfUnknownVoltages);

            var allPowers = DeterminePowers(allVoltages, indexOfPqBuses, indexOfPvBuses);
            allVoltages = DetermineFixedVoltages(allVoltages, indexOfPvBuses, indexOfSlackBuses);
            relativePowerError = CalculateRelativePowerError(allPowers, allVoltages, indexOfNodesWithUnknownVoltage);
            var voltageCollapse = relativePowerError > _nodeVoltageCalculator.MaximumRelativePowerError || Double.IsNaN(relativePowerError) ||
                                  Double.IsInfinity(relativePowerError);
            var nodeResults = CombineVoltagesAndPowersToNodeResults(allPowers, allVoltages);
            return voltageCollapse ? null : nodeResults;
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

            return new DenseVector(voltagesArray);
        }

        private static IList<NodeResult> CreateEmptyNodeResults(int countOfUnknownVoltages)
        {
            var result = new List<NodeResult>(countOfUnknownVoltages);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                result.Add(new NodeResult(new Complex(), new Complex()));

            return result;
        }

        private static IList<NodeResult> CombineVoltagesAndPowersToNodeResults(IList<Complex> allPowers, IList<Complex> allVoltages)
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

        private static DenseVector DetermineFixedVoltages(Vector<Complex> allVoltages, IEnumerable<NodeWithIndex> pvNodes, IEnumerable<NodeWithIndex> slackNodes)
        {
            var allVoltagesFixed = DenseVector.OfVector(allVoltages);

            foreach (var node in pvNodes)
                node.SetVoltageMagnitudeIn(allVoltagesFixed);

            foreach (var node in slackNodes)
                node.SetVoltageIn(allVoltagesFixed);

            return allVoltagesFixed;
        }

        private void SeperateNodesByTypes(out List<NodeWithIndex> slackNodes, out List<NodeWithIndex> pqNodes, out List<NodeWithIndex> pvNodes)
        {
            slackNodes = new List<NodeWithIndex>();
            pqNodes = new List<NodeWithIndex>();
            pvNodes = new List<NodeWithIndex>();

            for (var i = 0; i < Nodes.Count(); ++i)
                Nodes[i].AddTo(slackNodes, pqNodes, pvNodes, i);
        }

        private Vector<Complex> CalculateNominalVoltages(int countOfUnknownVoltages, IReadOnlyCollection<int> indexOfNodesWithUnknownVoltage)
        {
            var nominalVoltages = new DenseVector(countOfUnknownVoltages);

            for (var i = 0; i < indexOfNodesWithUnknownVoltage.Count; ++i)
                nominalVoltages[i] = Complex.FromPolarCoordinates(NominalVoltage, 0);

            return nominalVoltages;
        }

        private Vector<Complex> DeterminePowers(Vector<Complex> allVoltages, IEnumerable<NodeWithIndex> pqNodes, IEnumerable<NodeWithIndex> pvNodes)
        {
            var allPowers = Admittances.CalculateAllPowers(allVoltages);

            foreach (var node in pqNodes)
                node.SetPowerIn(allPowers);

            foreach (var node in pvNodes)
                node.SetRealPowerIn(allPowers);

            return allPowers;
        }

        private double CalculateRelativePowerError(Vector<Complex> allPowers, Vector<Complex> allVoltages, IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var inputPowerSum = allPowers.Sum();
            var absolutePowerSum = allPowers.Sum(power => Math.Abs(power.Real) + Math.Abs(power.Imaginary));
            var lossPowerSum = Admittances.CalculatePowerLoss(allVoltages);
            var currentSourcePowerSum = CalculatePowerInputOfCurrentSources(allVoltages, indexOfNodesWithUnknownVoltage);
            var absolutPowerError = (lossPowerSum - inputPowerSum - currentSourcePowerSum).Magnitude;
            var relativePowerError = absolutePowerSum > 1e-10 ? absolutPowerError/absolutePowerSum : absolutPowerError;
            return relativePowerError;
        }

        private Complex CalculatePowerInputOfCurrentSources(IList<Complex> allVoltages, IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var result = new Complex();

            for (var i = 0; i < indexOfNodesWithUnknownVoltage.Count; ++i)
            {
                var voltage = allVoltages[indexOfNodesWithUnknownVoltage[i]];
                var current = ConstantCurrents[i];
                result += voltage*current;
            }

            return result;
        }

        private Vector<Complex> CalculateUnknownVoltages(IReadOnlyCollection<NodeWithIndex> slackNodes, IReadOnlyCollection<NodeWithIndex> pqNodes, IReadOnlyCollection<NodeWithIndex> pvNodes, IReadOnlyList<int> indexOfNodesWithUnknownVoltage, int countOfUnknownVoltages)
        {
            var knownVoltages = ExtractKnownVoltages(slackNodes);
            var pqNodeWithIndices = ExtractPqNodesWithIndices(pqNodes);
            var pvNodeWithIndices = ExtractPvNodesWithIndices(pvNodes, pqNodeWithIndices.Count);
            Vector<Complex> constantCurrentsBySlackNodes;
            var indexOfNodesWithKnownVoltage = slackNodes.Select(x => x.Index).ToList();
            var admittancesToUnknownVoltages = Admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnknownVoltage,
                indexOfNodesWithKnownVoltage, knownVoltages, out constantCurrentsBySlackNodes);
            var constantCurrents = ExtractConstantCurrents(indexOfNodesWithUnknownVoltage);
            var constantCurrentsTotal = constantCurrents + constantCurrentsBySlackNodes;
            var totalAdmittanceRowSums = Admittances.CalculateRowSums();
            var initialVoltages = CalculateNominalVoltages(countOfUnknownVoltages,
                indexOfNodesWithUnknownVoltage);

            var unknownVoltages = _nodeVoltageCalculator.CalculateUnknownVoltages(admittancesToUnknownVoltages,
                totalAdmittanceRowSums, NominalVoltage,
                initialVoltages, constantCurrentsTotal, pqNodeWithIndices, pvNodeWithIndices);

            var result = CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnknownVoltage, unknownVoltages);
            return result;
        }

        private Vector<Complex> ExtractConstantCurrents(IReadOnlyList<int> nodeIndices)
        {
            var result = new SparseVector(nodeIndices.Count);
            var i = 0;

            foreach (var nodeIndex in nodeIndices)
            {
                result[i] = ConstantCurrents[nodeIndex];
                i++;
            }

            return result;
        }
    }
}