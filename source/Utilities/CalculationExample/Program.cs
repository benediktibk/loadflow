﻿using System;
using System.Numerics;
using HolomorphicEmbeddedLoadFlowMethod = Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators.HolomorphicEmbeddedLoadFlowMethod;
using NodeGraph = Calculation.SinglePhase.MultipleVoltageLevels.NodeGraph;
using PowerNetFactory = Calculation.SinglePhase.SingleVoltageLevel.PowerNetFactory;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;
using SymmetricPowerNet = Calculation.ThreePhase.SymmetricPowerNet;

namespace CalculationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(1e-5, 50, 64);
            var nodeGraph = new NodeGraph();
            var singleVoltagePowerNetFactory = new PowerNetFactory(nodeVoltageCalculator);
            var singlePhasePowerNet = new PowerNetComputable(50, singleVoltagePowerNetFactory, nodeGraph);
            var symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);

            symmetricPowerNet.AddNode(1, 1000, "source");
            symmetricPowerNet.AddNode(2, 1000, "load");
            symmetricPowerNet.AddTransmissionLine(1, 2, 0.0002, 0.0009, 0, 0, 2000, false);
            symmetricPowerNet.AddFeedIn(1, new Complex(1050, 100), new Complex());
            symmetricPowerNet.AddLoad(2, new Complex(-200, -100));

            double relativePowerError;
            var nodeResults = symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);

            if (nodeResults == null)
                Console.WriteLine("was not able to caculate the power net");
            else
                foreach (var nodeResult in nodeResults)
                    Console.WriteLine("node with ID " + nodeResult.Key + " has the voltage " + nodeResult.Value.Voltage + " V");

            Console.ReadKey();
        }
    }
}