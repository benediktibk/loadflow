using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            const double targetPrecision = 0.00001;
            const int iterations = 1000;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"node potential method", new NodePotentialMethod()},
                {"curent iteration", new CurrentIteration(targetPrecision, iterations)},
                {"newton raphson", new NewtonRaphsonMethod(targetPrecision, iterations)},
                {"FDLF", new FastDecoupledLoadFlowMethod(targetPrecision, iterations)},
                {
                    "HELM 64 Bit",
                    new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, new PrecisionLongDouble(), false)
                },
                {
                    "HELM with current iteration",
                    new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision,
                        new CurrentIteration(targetPrecision, iterations))
                },
                {
                    "HELM with newton raphson",
                    new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision,
                        new NewtonRaphsonMethod(targetPrecision, iterations))
                },
                {
                    "HELM 200 Bit",
                    new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 80, new PrecisionMulti(200), false)
                },
                {
                    "HELM 1000 Bit",
                    new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 80, new PrecisionMulti(5000), false)
                }
            };

            foreach (var calculator in calculators)
            {
                var border = DetermineBorderOfVoltageCollapse(calculator.Value, targetPrecision);
                Console.WriteLine(calculator.Key + ";" + border);
            }

            Console.ReadKey();
        }

        private static double DetermineBorderOfVoltageCollapse(INodeVoltageCalculator nodeVoltageCalculator, double targetPrecision)
        {
            var lower = 0.0;
            var upper = 0.25001;

            while (upper - lower > 0.0001)
                ReduceInterval(ref lower, ref upper, nodeVoltageCalculator,targetPrecision);

            return (upper + lower)/2;
        }

        private static void ReduceInterval(ref double lower, ref double upper, INodeVoltageCalculator nodeVoltageCalculator, double targetPrecision)
        {
            var middle = (upper + lower)/2;

            if (PreciseEnough(nodeVoltageCalculator, middle, targetPrecision))
                lower = middle;
            else
                upper = middle;
        }

        private static bool PreciseEnough(INodeVoltageCalculator nodeVoltageCalculator, double load, double targetPrecision)
        {
            var powerNet = CreatePowerNet(load);
            var calculator = new LoadFlowCalculator(nodeVoltageCalculator);
            var voltageCollapse = powerNet.CalculateMissingInformation(calculator);

            if (load > 0.25)
                return voltageCollapse;

            if (voltageCollapse)
                return false;

            var voltageShouldBe = 0.5 + Math.Sqrt(0.25 - load);
            var voltageIs = powerNet.GetNodes()[1].Voltage;
            var error = (voltageShouldBe - voltageIs).Magnitude / voltageShouldBe;
            return error < targetPrecision;
        }

        private static PowerNet CreatePowerNet(double load)
        {
            var powerNet = new PowerNet(2, 1);
            powerNet.Admittances.AddConnection(0, 1, new Complex(1, 0));
            var feedInNode = new Node() {Voltage = new Complex(1, 0)};
            var loadNode = new Node() {Power = new Complex((-1)*load, 0)};
            powerNet.SetNode(0, feedInNode);
            powerNet.SetNode(1, loadNode);
            return powerNet;
        }
    }
}
