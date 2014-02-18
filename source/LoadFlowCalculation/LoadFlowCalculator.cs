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
        abstract public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers);

        public Node[] CalculateNodeVoltagesAndPowers(Matrix<Complex> admittances, double nominalVoltage, Node[] nodes)
        {
            CheckDimensions(admittances, nodes);

            List<int> indexOfNodesWithKnownVoltage;
            List<int> indexOfNodesWithUnknownVoltage;
            SeperateNodesInKnownAndUnknownVoltages(nodes, out indexOfNodesWithKnownVoltage, out indexOfNodesWithUnknownVoltage);

            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;

            if (countOfKnownVoltages == 0)
                throw new NotFullRankException();

            Vector<Complex> allVoltages;
            if (countOfUnknownVoltages == 0)
                allVoltages = ExtractKnownVoltages(nodes, indexOfNodesWithKnownVoltage);
            else
            {
                var knownVoltages = ExtractKnownVoltages(nodes, indexOfNodesWithKnownVoltage);
                var knownPowers = ExtractKnownPowers(nodes, indexOfNodesWithUnknownVoltage);
                var admittancesReduced = ExtractRowsOfUnknownVoltages(admittances, indexOfNodesWithUnknownVoltage);
                var admittancesToKnownVoltages = ExtractAdmittancesToKnownVoltages(admittancesReduced,
                    indexOfNodesWithKnownVoltage);
                var admittancesToUnknownVoltages = ExtractAdmittancesToUnknownVoltages(admittancesReduced,
                    indexOfNodesWithUnknownVoltage);

                var constantCurrents = admittancesToKnownVoltages.Multiply(knownVoltages);
                var unknownVoltages = CalculateUnknownVoltages(admittancesToUnknownVoltages,
                    nominalVoltage, constantCurrents, knownPowers);

                allVoltages = CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                    indexOfNodesWithUnknownVoltage, unknownVoltages);
            }

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
                var node = new Node()
                {
                    Power = allPowers[i],
                    Voltage = allVoltages[i]
                };
                result[i] = node;
            }

            return result;
        }

        private static Vector<Complex> CalculateAllPowers(Matrix<Complex> admittances, Vector<Complex> allVoltages)
        {
            var currents = admittances.Multiply(allVoltages);
            var allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        private static Vector<Complex> CombineKnownAndUnknownVoltages(IReadOnlyList<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages,
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

        private static Vector<Complex> ExtractKnownPowers(IList<Node> nodes, IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var knownPowersArray = new Complex[countOfUnknownVoltages];

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                knownPowersArray[i] = nodes[indexOfNodesWithUnknownVoltage[i]].Power;

            var knownPowers = new DenseVector(knownPowersArray);
            return knownPowers;
        }

        private static Matrix<Complex> ExtractAdmittancesToUnknownVoltages(Matrix<Complex> admittancesReduced,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesToUnknownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfUnknownVoltages);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesToUnknownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithUnknownVoltage[i]));

            return admittancesToUnknownVoltages;
        }

        private static Matrix<Complex> ExtractAdmittancesToKnownVoltages(Matrix<Complex> admittancesReduced, IReadOnlyList<int> indexOfNodesWithKnownVoltage)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var admittancesToKnownVoltages = new SparseMatrix(admittancesReduced.RowCount, countOfKnownVoltages);

            for (var i = 0; i < countOfKnownVoltages; ++i)
                admittancesToKnownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithKnownVoltage[i]));

            return admittancesToKnownVoltages;
        }

        private static Vector<Complex> ExtractKnownVoltages(IList<Node> nodes, IReadOnlyList<int> indexOfNodesWithKnownVoltage)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var knownVoltagesArray = new Complex[countOfKnownVoltages];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                knownVoltagesArray[i] = nodes[indexOfNodesWithKnownVoltage[i]].Voltage;

            var knownVoltages = new DenseVector(knownVoltagesArray);
            return knownVoltages;
        }

        private static Matrix<Complex> ExtractRowsOfUnknownVoltages(Matrix<Complex> admittances, IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var nodeCount = admittances.ColumnCount;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesReduced = new SparseMatrix(countOfUnknownVoltages, nodeCount);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesReduced.SetRow(i, admittances.Row(indexOfNodesWithUnknownVoltage[i]));
            return admittancesReduced;
        }

        private static void SeperateNodesInKnownAndUnknownVoltages(IList<Node> nodes,
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

        private static void CheckDimensions(Matrix<Complex> admittances, IEnumerable<Node> nodes)
        {
            var rows = admittances.RowCount;
            var columns = admittances.ColumnCount;
            var nodeCount = nodes.Count();

            if (rows != columns)
                throw new NotQuadraticException();

            if (rows != nodeCount)
                throw new ArgumentOutOfRangeException();

            if (!admittances.IsSymmetric)
                throw new NotSymmetricException();
        }
    }
}
