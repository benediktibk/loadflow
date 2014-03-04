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
        public abstract Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse);

        /// <summary>
        /// calculates the missing node voltages and powers
        /// </summary>
        /// <param name="admittances"></param>
        /// <param name="nominalVoltage"></param>
        /// <param name="nodes"></param>
        /// <param name="voltageCollapse">the relability of this param depends on the used method, e.g. for CurrentIteration it could also just mean that the solution does not converge</param>
        /// <returns></returns>
        public Node[] CalculateNodeVoltagesAndPowers(Matrix<Complex> admittances, double nominalVoltage, Node[] nodes, out bool voltageCollapse)
        {
            CheckDimensions(admittances, nodes);

            if (!IsAdmittanceMatrixValid(admittances))
                throw new ArgumentOutOfRangeException("admittances", "the admittance matrix is invalid");

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
            {
                voltageCollapse = false;
                allVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
            }
            else
            {
                var knownVoltages = ExtractKnownVoltages(nodes, indexOfSlackBuses);
                var pqBuses = ExtractPQBuses(nodes, indexOfPQBuses);
                var pvBuses = ExtractPVBuses(nodes, indexOfPVBuses, pqBuses.Count);
                Matrix<Complex> admittancesToUnknownVoltages;
                Vector<Complex> constantCurrentRightHandSide;
                ReduceAdmittancesByKnownVoltages(admittances, indexOfNodesWithUnknownVoltage, indexOfSlackBuses, knownVoltages, out admittancesToUnknownVoltages, out constantCurrentRightHandSide);

                var unknownVoltages = CalculateUnknownVoltages(admittancesToUnknownVoltages,
                    nominalVoltage, constantCurrentRightHandSide, pqBuses, pvBuses, out voltageCollapse);

                allVoltages = CombineKnownAndUnknownVoltages(indexOfSlackBuses, knownVoltages,
                    indexOfNodesWithUnknownVoltage, unknownVoltages);
            }

            var allPowers = CalculateAllPowers(admittances, allVoltages);
            return CombineVoltagesAndPowersToNodes(allPowers, allVoltages);
        }

        public static void ReduceAdmittancesByKnownVoltages(Matrix<Complex> admittances, List<int> indexOfNodesWithUnknownVoltage,
            List<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages, out Matrix<Complex> admittancesToUnknownVoltages,
            out Vector<Complex> constantCurrentRightHandSide)
        {
            var admittancesReduced = ExtractRowsOfUnknownVoltages(admittances,
                indexOfNodesWithUnknownVoltage);
            var admittancesToKnownVoltages = ExtractAdmittancesToKnownVoltages(admittancesReduced,
                indexOfNodesWithKnownVoltage);
            admittancesToUnknownVoltages = ExtractAdmittancesToUnknownVoltages(admittancesReduced,
                indexOfNodesWithUnknownVoltage);

            var constantCurrentsLeftHandSide = admittancesToKnownVoltages.Multiply(knownVoltages);
            constantCurrentRightHandSide = constantCurrentsLeftHandSide.Multiply(new Complex(-1, 0));
        }

        private static Node[] CombineVoltagesAndPowersToNodes(Vector<Complex> allPowers, Vector<Complex> allVoltages)
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

        public static Vector<Complex> CalculateAllPowers(Matrix<Complex> admittances, Vector<Complex> allVoltages)
        {
            var currents = admittances.Multiply(allVoltages);
            var allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        private static Vector<Complex> CombineKnownAndUnknownVoltages(IReadOnlyList<int> indexOfNodesWithKnownVoltage,
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

        public static Matrix<Complex> ExtractAdmittancesToUnknownVoltages(Matrix<Complex> admittancesReduced,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesToUnknownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfUnknownVoltages);

            for (int i = 0; i < countOfUnknownVoltages; ++i)
                admittancesToUnknownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithUnknownVoltage[i]));

            return admittancesToUnknownVoltages;
        }

        public static Matrix<Complex> ExtractAdmittancesToKnownVoltages(Matrix<Complex> admittancesReduced,
            IReadOnlyList<int> indexOfNodesWithKnownVoltage)
        {
            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var admittancesToKnownVoltages = new SparseMatrix(admittancesReduced.RowCount, countOfKnownVoltages);

            for (var i = 0; i < countOfKnownVoltages; ++i)
                admittancesToKnownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithKnownVoltage[i]));

            return admittancesToKnownVoltages;
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

        public static Matrix<Complex> ExtractRowsOfUnknownVoltages(Matrix<Complex> admittances,
            IReadOnlyList<int> indexOfNodesWithUnknownVoltage)
        {
            var nodeCount = admittances.ColumnCount;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var admittancesReduced = new SparseMatrix(countOfUnknownVoltages, nodeCount);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesReduced.SetRow(i, admittances.Row(indexOfNodesWithUnknownVoltage[i]));

            return admittancesReduced;
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

        private static void CheckDimensions(Matrix<Complex> admittances, IEnumerable<Node> nodes)
        {
            var rows = admittances.RowCount;
            var columns = admittances.ColumnCount;
            var nodeCount = nodes.Count();

            if (rows != columns)
                throw new ArgumentOutOfRangeException("admittances", "the admittance matrix must be quadratic");

            if (rows != nodeCount)
                throw new ArgumentOutOfRangeException("nodes",
                    "the count of nodes does not match the dimensions of the admittance matrix");

            if (!admittances.IsSymmetric)
                throw new ArgumentOutOfRangeException("admittances", "the admittance matrix must be symmetric");
        }

        private static bool IsAdmittanceMatrixValid(Matrix<Complex> admittances)
        {
            for (var i = 0; i < admittances.ColumnCount; ++i)
            {
                var column = admittances.Column(i);
                if (column.Sum().Magnitude > 0.00001)
                    return false;
            }

            return true;
        }
    }
}