﻿using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

namespace CalculationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(1e-5, 50, 64, true);
            var symmetricPowerNet = SymmetricPowerNet.Create(nodeVoltageCalculator, 50);

            symmetricPowerNet.AddNode(1, 1000, "source");
            symmetricPowerNet.AddNode(2, 1000, "load");
            symmetricPowerNet.AddTransmissionLine(1, 2, 0.0002, 0.0009, 0, 0, 2000, false);
            symmetricPowerNet.AddFeedIn(1, new Complex(1050, 100), new Complex());
            symmetricPowerNet.AddLoad(2, new Complex(-200, -100));

            double relativePowerError;
            var nodeResults = symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);

            if (nodeResults == null)
                Console.WriteLine("was not able to calcuate the power net");
            else
                foreach (var nodeResult in nodeResults)
                    Console.WriteLine("node with ID " + nodeResult.Key + " has the voltage " + nodeResult.Value.Voltage + " V");

            Console.ReadKey();
        }
    }
}
