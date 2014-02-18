using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    abstract public class LoadFlowCalculator
    {
        abstract public Vector CalculateNodeVoltagesInternal(Matrix admittancesToKnownVoltages,
            Matrix admittancesToUnknownVoltages, double nominalVoltage, Vector knownVoltages, Vector knownPowers);

        public Node[] CalculateNodeVoltages(Matrix admittances, double nominalVoltage, Node[] nodes)
        {
            CheckDimensions(admittances, nodes);

            List<int> indexOfNodesWithKnownVoltage;
            List<int> indexOfNodesWithUnknownVoltage;
            SeperateNodesInKnownAndUnknownVoltages(nodes, out indexOfNodesWithKnownVoltage, out indexOfNodesWithUnknownVoltage);

            //var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            //var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;

            var knownVoltages = ExtractKnownVoltages(nodes, indexOfNodesWithKnownVoltage);
            var knownPowers = ExtractKnownPowers(nodes, indexOfNodesWithUnknownVoltage);
            var admittancesReduced = ExtractRowsOfUnknownVoltages(admittances, indexOfNodesWithUnknownVoltage);
            var admittancesToKnownVoltages = ExtractAdmittancesToKnownVoltages(admittancesReduced, indexOfNodesWithKnownVoltage);
            var admittancesToUnknownVoltages = ExtractAdmittancesToUnknownVoltages(admittancesReduced, indexOfNodesWithUnknownVoltage);

            var unknownVoltages = CalculateNodeVoltagesInternal(admittancesToKnownVoltages, admittancesToUnknownVoltages,
                nominalVoltage, knownVoltages, knownPowers);

            var allVoltages = CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages, indexOfNodesWithUnknownVoltage, unknownVoltages);
            var allPowers = CalculateAllPowers(admittances, allVoltages);
            return CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
        }

        private static Node[] CombineVoltagesAndPowersToNodes(Vector<Complex> allPowers, Vector<Complex> allVoltages)
        {
            if (allPowers.Count != allVoltages.Count)
                throw new ArgumentOutOfRangeException();

            var nodeCount = allPowers.Count;
            var result = new Node[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
            {
                var node = new Node();
                node.Power = allPowers[i];
                node.Voltage = allVoltages[i];
                result[i] = node;
            }

            return result;
        }

        private static Vector<Complex> CalculateAllPowers(Matrix admittances, DenseVector allVoltages)
        {
            var allPowers = (allVoltages.Conjugate()).PointwiseMultiply(admittances.Multiply(allVoltages));
            return allPowers;
        }

        private static DenseVector CombineKnownAndUnknownVoltages(List<int> indexOfNodesWithKnownVoltage, Vector knownVoltages,
            List<int> indexOfNodesWithUnknownVoltage, Vector unknownVoltages)
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

        private static DenseVector ExtractKnownPowers(Node[] nodes, List<int> indexOfNodesWithUnknownVoltage)
        {
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var knownPowersArray = new Complex[countOfUnknownVoltages];

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                knownPowersArray[i] = nodes[indexOfNodesWithUnknownVoltage[i]].Power;

            var knownPowers = new DenseVector(knownPowersArray);
            return knownPowers;
        }

        private static SparseMatrix ExtractAdmittancesToUnknownVoltages(SparseMatrix admittancesReduced,
            List<int> indexOfNodesWithUnknownVoltage)
        {
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesToUnknownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfUnknownVoltages);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesToUnknownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithUnknownVoltage[i]));

            return admittancesToUnknownVoltages;
        }

        private static SparseMatrix ExtractAdmittancesToKnownVoltages(SparseMatrix admittancesReduced, List<int> indexOfNodesWithKnownVoltage)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var admittancesToKnownVoltages = new SparseMatrix(admittancesReduced.RowCount, countOfKnownVoltages);

            for (var i = 0; i < countOfKnownVoltages; ++i)
                admittancesToKnownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithKnownVoltage[i]));

            return admittancesToKnownVoltages;
        }

        private static DenseVector ExtractKnownVoltages(Node[] nodes, List<int> indexOfNodesWithKnownVoltage)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var knownVoltagesArray = new Complex[countOfKnownVoltages];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                knownVoltagesArray[i] = nodes[indexOfNodesWithKnownVoltage[i]].Voltage;

            var knownVoltages = new DenseVector(knownVoltagesArray);
            return knownVoltages;
        }

        private static SparseMatrix ExtractRowsOfUnknownVoltages(Matrix admittances, List<int> indexOfNodesWithUnknownVoltage)
        {
            var nodeCount = admittances.ColumnCount;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesReduced = new SparseMatrix(countOfUnknownVoltages, nodeCount);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesReduced.SetRow(i, admittances.Row(indexOfNodesWithUnknownVoltage[i]));
            return admittancesReduced;
        }

        private static void SeperateNodesInKnownAndUnknownVoltages(Node[] nodes,
            out List<int> indexOfNodesWithKnownVoltage, out List<int> indexOfNodesWithUnknownVoltage)
        {
            indexOfNodesWithKnownVoltage = new List<int>();
            indexOfNodesWithUnknownVoltage = new List<int>();

            for (var i = 0; i < nodes.Count(); ++i)
            {
                var node = nodes[i];

                if (node.VoltageIsKnown)
                    indexOfNodesWithKnownVoltage.Add(i);
                else
                    indexOfNodesWithUnknownVoltage.Add(i);

                if (!node.VoltageIsKnown && !node.PowerIsKnown)
                    throw new UnderDeterminedProblemException();

                if (node.VoltageIsKnown && node.PowerIsKnown)
                    throw new OverDeterminedProblemException();
            }
        }

        private static void CheckDimensions(Matrix admittances, Node[] nodes)
        {
            var rows = admittances.RowCount;
            var columns = admittances.ColumnCount;
            var nodeCount = nodes.Count();

            if (rows != columns)
                throw new NotQuadraticException();

            if (rows != nodeCount)
                throw new ArgumentOutOfRangeException();
        }
    }
}
