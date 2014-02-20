using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public abstract class LoadFlowCalculator
    {
        public abstract Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, Vector<Complex> knownPowers);

        public Node[] CalculateNodeVoltagesAndPowers(Matrix<Complex> admittances, double nominalVoltage, Node[] nodes)
        {
            CheckDimensions(admittances, nodes);

            List<int> indexOfNodesWithKnownVoltage;
            List<int> indexOfNodesWithUnknownVoltage;
            SeperateNodesInKnownAndUnknownVoltages(nodes, out indexOfNodesWithKnownVoltage,
                out indexOfNodesWithUnknownVoltage);

            int countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            int countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;

            if (countOfKnownVoltages == 0)
                throw new NotFullRankException();

            Vector<Complex> allVoltages;
            if (countOfUnknownVoltages == 0)
                allVoltages = ExtractKnownVoltages(nodes, indexOfNodesWithKnownVoltage);
            else
            {
                Vector<Complex> knownVoltages = ExtractKnownVoltages(nodes, indexOfNodesWithKnownVoltage);
                Vector<Complex> knownPowers = ExtractKnownPowers(nodes, indexOfNodesWithUnknownVoltage);
                Matrix<Complex> admittancesReduced = ExtractRowsOfUnknownVoltages(admittances,
                    indexOfNodesWithUnknownVoltage);
                Matrix<Complex> admittancesToKnownVoltages = ExtractAdmittancesToKnownVoltages(admittancesReduced,
                    indexOfNodesWithKnownVoltage);
                Matrix<Complex> admittancesToUnknownVoltages = ExtractAdmittancesToUnknownVoltages(admittancesReduced,
                    indexOfNodesWithUnknownVoltage);

                Vector<Complex> constantCurrentsLeftHandSide = admittancesToKnownVoltages.Multiply(knownVoltages);
                Vector<Complex> constantCurrentRightHandSide = constantCurrentsLeftHandSide.Multiply(new Complex(-1, 0));
                Vector<Complex> unknownVoltages = CalculateUnknownVoltages(admittancesToUnknownVoltages,
                    nominalVoltage, constantCurrentRightHandSide, knownPowers);

                allVoltages = CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                    indexOfNodesWithUnknownVoltage, unknownVoltages);
            }

            Vector<Complex> allPowers = CalculateAllPowers(admittances, allVoltages);
            return CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
        }

        private static Node[] CombineVoltagesAndPowersToNodes(Vector<Complex> allPowers, Vector<Complex> allVoltages)
        {
            if (allPowers.Count != allVoltages.Count)
                throw new ArgumentOutOfRangeException();

            int nodeCount = allPowers.Count;
            var result = new Node[nodeCount];

            for (int i = 0; i < nodeCount; ++i)
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

        private static Vector<Complex> CalculateAllPowers(Matrix<Complex> admittances, Vector<Complex> allVoltages)
        {
            Vector<Complex> currents = admittances.Multiply(allVoltages);
            Vector<Complex> allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        private static Vector<Complex> CombineKnownAndUnknownVoltages(IReadOnlyList<int> indexOfNodesWithKnownVoltage,
            Vector<Complex> knownVoltages,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage, Vector<Complex> unknownVoltages)
        {
            int countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            int countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            int nodeCount = countOfKnownVoltages + countOfUnknownVoltages;
            var voltagesArray = new Complex[nodeCount];

            for (int i = 0; i < countOfKnownVoltages; ++i)
                voltagesArray[indexOfNodesWithKnownVoltage[i]] = knownVoltages.At(i);

            for (int i = 0; i < countOfUnknownVoltages; ++i)
                voltagesArray[indexOfNodesWithUnknownVoltage[i]] = unknownVoltages.At(i);

            var allVoltages = new DenseVector(voltagesArray);
            return allVoltages;
        }

        private static Vector<Complex> ExtractKnownPowers(IList<Node> nodes,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            int countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var knownPowersArray = new Complex[countOfUnknownVoltages];

            for (int i = 0; i < countOfUnknownVoltages; ++i)
                knownPowersArray[i] = nodes[indexOfNodesWithUnknownVoltage[i]].Power;

            var knownPowers = new DenseVector(knownPowersArray);
            return knownPowers;
        }

        private static Matrix<Complex> ExtractAdmittancesToUnknownVoltages(Matrix<Complex> admittancesReduced,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            int countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesToUnknownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfUnknownVoltages);

            for (int i = 0; i < countOfUnknownVoltages; ++i)
                admittancesToUnknownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithUnknownVoltage[i]));

            return admittancesToUnknownVoltages;
        }

        private static Matrix<Complex> ExtractAdmittancesToKnownVoltages(Matrix<Complex> admittancesReduced,
            IReadOnlyList<int> indexOfNodesWithKnownVoltage)
        {
            int countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var admittancesToKnownVoltages = new SparseMatrix(admittancesReduced.RowCount, countOfKnownVoltages);

            for (int i = 0; i < countOfKnownVoltages; ++i)
                admittancesToKnownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithKnownVoltage[i]));

            return admittancesToKnownVoltages;
        }

        private static Vector<Complex> ExtractKnownVoltages(IList<Node> nodes,
            IReadOnlyList<int> indexOfNodesWithKnownVoltage)
        {
            int countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var knownVoltagesArray = new Complex[countOfKnownVoltages];

            for (int i = 0; i < countOfKnownVoltages; ++i)
                knownVoltagesArray[i] = nodes[indexOfNodesWithKnownVoltage[i]].Voltage;

            var knownVoltages = new DenseVector(knownVoltagesArray);
            return knownVoltages;
        }

        private static Matrix<Complex> ExtractRowsOfUnknownVoltages(Matrix<Complex> admittances,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            int nodeCount = admittances.ColumnCount;
            int countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesReduced = new SparseMatrix(countOfUnknownVoltages, nodeCount);

            for (int i = 0; i < countOfUnknownVoltages; ++i)
                admittancesReduced.SetRow(i, admittances.Row(indexOfNodesWithUnknownVoltage[i]));
            return admittancesReduced;
        }

        private static void SeperateNodesInKnownAndUnknownVoltages(IList<Node> nodes,
            out List<int> indexOfNodesWithKnownVoltage, out List<int> indexOfNodesWithUnknownVoltage)
        {
            indexOfNodesWithKnownVoltage = new List<int>();
            indexOfNodesWithUnknownVoltage = new List<int>();

            for (int i = 0; i < nodes.Count(); ++i)
            {
                Node node = nodes[i];

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
            int rows = admittances.RowCount;
            int columns = admittances.ColumnCount;
            int nodeCount = nodes.Count();

            if (rows != columns)
                throw new NotQuadraticException();

            if (rows != nodeCount)
                throw new ArgumentOutOfRangeException();

            if (!admittances.IsSymmetric)
                throw new NotSymmetricException();
        }
    }
}