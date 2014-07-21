using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using Node = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.Node;
using PowerNet = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.PowerNet;
using AdmittanceMatrix = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var j = -20; j <= 20; ++j)
            {
                var powerNet = new PowerNet(50);
                powerNet.AddNode("sourceNode", 1000);
                powerNet.AddNode("targetNode", 400);
                powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1002, 0), 0);
                powerNet.AddTransformer("sourceNode", "targetNode", "transformer", new Complex(9.410964912, 0),
                    new Complex(0.399, 0), new Complex(), 2.0, Math.Pow(10, j));
                powerNet.AddLoad("targetNode", "load", new Complex(-2000, 0));

                var nodes = new List<IReadOnlyNode>(powerNet.GetAllNecessaryNodes());
                IReadOnlyNode groundNode = null;

                if (powerNet.CheckIfGroundNodeIsNecessary())
                {
                    groundNode = new Node("#ground", 0);
                    nodes.Add(groundNode);
                }

                var nodeIndexes = new Dictionary<IReadOnlyNode, int>();

                for (var i = 0; i < nodes.Count; ++i)
                    nodeIndexes.Add(nodes[i], i);

                var admittances = new AdmittanceMatrix(nodes.Count, nodeIndexes);
                powerNet.FillInAdmittances(admittances, 1);
                var matrix = admittances.GetValues();
                var matrixReduced = new DenseMatrix(matrix.RowCount - 1, matrix.ColumnCount - 1);
                for (var row = 0; row < matrix.RowCount - 1; ++row)
                    for (var column = 0; column < matrix.ColumnCount - 1; ++column)
                        matrixReduced[row, column] = matrix[row + 1, column + 1];

                var separation = matrixReduced.LU();
                var norm = matrixReduced.L2Norm();
                var inverse = separation.Inverse();

                try
                {
                    var inverseNorm = inverse.L2Norm();
                    var condition = (norm*inverseNorm).Magnitude;
                    Console.WriteLine(j + " -> " + condition);
                }
                catch (NonConvergenceException exception)
                {
                    Console.WriteLine(j + " -> not converging");
                }
            }

            Console.ReadKey();
        }
    }
}
