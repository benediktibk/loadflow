using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using SincalConnector;

namespace ConvergenceExperiment
{
    class Program
    {
        private const string _powerNet = "data/vorstadt_files/database.mdb";
        private const int _nodeId = 1280;

        private static void Main(string[] args)
        {
            const double targetPrecision = 1e-10;
            const int maximumIterations = 100;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"Newton-Raphson, iterative", new NewtonRaphsonMethod(targetPrecision, maximumIterations, true)},
                {"Newton-Raphson, LU", new NewtonRaphsonMethod(targetPrecision, maximumIterations, false)},
                {"FDLF, iterative", new FastDecoupledLoadFlowMethod(targetPrecision, maximumIterations, true)},
                {"FDLF, LU", new FastDecoupledLoadFlowMethod(targetPrecision, maximumIterations, false)},
                {"HELM, 64 Bit, iterative", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true)},
                {"HELM, 64 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false)},
                {"HELM mit Current Iteration, 64 Bit, iterative", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true), new CurrentIteration(targetPrecision, maximumIterations, true))},
                {"HELM mit Current Iteration, 64 Bit, LU", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false), new CurrentIteration(targetPrecision, maximumIterations, false))},
                {"HELM, 100 Bit, iterative", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, true)},
                {"HELM, 100 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, false)},
                {"HELM, 200 Bit, iterative", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, true)},
                {"HELM, 200 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, false)},
                {"Current Iteration, iterative", new CurrentIteration(targetPrecision, maximumIterations, true)},
                {"Current Iteration, LU", new CurrentIteration(targetPrecision, maximumIterations, false)}
            };
            var file = new StreamWriter("results.csv", false);
            file.WriteLine("Verfahren;Konvergenzgrenze [W]");

            foreach (var calculator in calculators)
            {
                Console.WriteLine("next calculator: " + calculator.Key);
                Console.WriteLine("searching for upper limit");
                var upperLimit = FindUnstableLoad(calculator.Value);
                Console.WriteLine("searching for convergence border");
                var convergenceBorder = FindConvergenceBorderInRange(0, upperLimit, calculator.Value);
                Console.WriteLine("convergence border for " + calculator.Key + ": " + convergenceBorder);
                file.WriteLine(calculator.Key + ";" + convergenceBorder);
                file.Flush();
            }

            file.Close();
            Console.ReadKey();
        }

        private static double FindUnstableLoad(INodeVoltageCalculator calculator)
        {
            var result = 1.0e4;

            while (true)
            {
                Console.WriteLine("testing " + result);
                var convergence = CheckConvergence(calculator, result);

                if (!convergence)
                    return result;

                result = result*10;
            }
        }

        private static double FindConvergenceBorderInRange(double lowerLimit, double upperLimit,
            INodeVoltageCalculator calculator)
        {
            const double relativePrecision = 1e-4;
            while ((upperLimit - lowerLimit)/upperLimit > relativePrecision)
            {
                var middle = (upperLimit + lowerLimit) / 2;
                Console.WriteLine("testing " + middle);
                var convergence = CheckConvergence(calculator, middle);

                if (convergence)
                    lowerLimit = middle;
                else
                    upperLimit = middle;
            }

            return lowerLimit;
        }

        private static bool CheckConvergence(INodeVoltageCalculator calculator, double additionalLoad)
        {
            var powerNet = new PowerNetDatabaseAdapter(_powerNet);
            powerNet.AddLoad(_nodeId, new Complex(additionalLoad, 0));
            bool convergence;

            try
            {
                double relativePowerError;
                convergence = powerNet.CalculateNodeVoltages(calculator, out relativePowerError);

            }
            catch (Exception)
            {
                convergence = false;
            }

            return convergence;
        }
    }
}
