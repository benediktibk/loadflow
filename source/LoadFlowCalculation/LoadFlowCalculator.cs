using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public class LoadFlowCalculator
    {
        private readonly ILoadFlowCalculatorInternal _calculatorInternal;

        public LoadFlowCalculator(ILoadFlowCalculatorInternal calculatorInternal)
        {
            _calculatorInternal = calculatorInternal;
        }

        public Node[] CalculateNodeVoltages(Matrix admittances, double nominalVoltage, Node[] nodes)
        {
            var rows = admittances.RowCount;
            var columns = admittances.ColumnCount;
            var nodeCount = nodes.Count();

            if (rows != columns)
                throw new NotQuadraticException();

            if (rows != nodeCount)
                throw new ArgumentOutOfRangeException();

            var indexOfNodesWithKnownVoltage = new List<int>();
            var indexOfNodesWithUnknownVoltage = new List<int>();

            for (var i = 0; i < nodeCount; ++i)
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

            var countOfKnownVoltages = indexOfNodesWithKnownVoltage.Count;
            var countOfUnknownVoltages = indexOfNodesWithUnknownVoltage.Count;
            var knownVoltagesArray = new Complex[countOfKnownVoltages];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                knownVoltagesArray[i] = nodes[indexOfNodesWithKnownVoltage[i]].Voltage;

            var knownVoltages = new DenseVector(knownVoltagesArray);
            var admittancesToKnownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfKnownVoltages);
            var admittancesToUnknownVoltages = new SparseMatrix(countOfUnknownVoltages, countOfUnknownVoltages);
            var admittancesReduced = new SparseMatrix(countOfUnknownVoltages, nodeCount);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesReduced.SetRow(i, admittances.Row(indexOfNodesWithUnknownVoltage[i]));

            for (var i = 0; i < countOfKnownVoltages; ++i)
                admittancesToKnownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithKnownVoltage[i]));

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                admittancesToUnknownVoltages.SetColumn(i, admittancesReduced.Column(indexOfNodesWithUnknownVoltage[i]));

            var knownPowersArray = new Complex[countOfUnknownVoltages];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                knownPowersArray[i] = nodes[indexOfNodesWithUnknownVoltage[i]].Power;

            var knownPowers = new DenseVector(knownPowersArray);
            var unknownVoltages = _calculatorInternal.CalculateNodeVoltagesInternal(admittancesToKnownVoltages, admittancesToUnknownVoltages,
                nominalVoltage, knownVoltages, knownPowers);

            var voltagesArray = new Complex[nodeCount];

            for (var i = 0; i < countOfKnownVoltages; ++i)
                voltagesArray[indexOfNodesWithKnownVoltage[i]] = knownVoltages.At(i);

            for (var i = 0; i < countOfUnknownVoltages; ++i)
                voltagesArray[indexOfNodesWithUnknownVoltage[i]] = unknownVoltages.At(i);

            var allVoltages = new DenseVector(voltagesArray);
            var allPowers = (allVoltages.Conjugate()).PointwiseMultiply(admittances.Multiply(allVoltages));

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
    }
}
