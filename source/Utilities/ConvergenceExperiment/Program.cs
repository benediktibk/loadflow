using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using SincalConnector;
using PowerNetComputable = Calculation.SinglePhase.SingleVoltageLevel.PowerNetComputable;

namespace ConvergenceExperiment
{
    class Program
    {
        private static void Main(string[] args)
        {
            ConsoleKeyInfo key;
            do
            {
                Console.WriteLine("press 1 for the big net and 2 for the small one");
                key = Console.ReadKey();
            } while (key.KeyChar != '1' && key.KeyChar != '2');

            if (key.KeyChar == '1')
                BigNet();
            else
                SmallNet();

            Console.ReadKey();
        }

        private static void BigNet()
        {
            const int maximumIterations = 100;
            const double targetPrecision = 1e-10;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"HELM, 64 Bit, iterativ", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true)},
                {"HELM, 64 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false)},
                {"HELM mit Stromiteration, 64 Bit, LU", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false), new CurrentIteration(targetPrecision, maximumIterations, false))},
                {"HELM, 100 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, false)},
                {"HELM, 200 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, false)},
                {"HELM, 1000 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 200, 1000, false)},
                {"HELM, 10000 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 300, 10000, false)},
                {"Stromiteration, iterativ", new CurrentIteration(targetPrecision, maximumIterations, true)},
                {"Stromiteration, LU", new CurrentIteration(targetPrecision, maximumIterations, false)},
                {"Newton-Raphson, iterativ", new NewtonRaphsonMethod(targetPrecision, maximumIterations, true)},
                {"Newton-Raphson, LU", new NewtonRaphsonMethod(targetPrecision, maximumIterations, false)},
            };
            var file = new StreamWriter("results_big.csv", false);
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
        }

        private static void SmallNet()
        {
            const int maximumIterations = 100;
            const double targetPrecision = 1e-20;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"HELM, 64 Bit, iterativ", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true)},
                {"HELM, 64 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false)},
                {"HELM mit Stromiteration, 64 Bit, LU", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false), new CurrentIteration(targetPrecision, maximumIterations, false))},
                {"HELM, 100 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, false)},
                {"HELM, 200 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, false)},
                {"Stromiteration, iterativ", new CurrentIteration(targetPrecision, maximumIterations, true)},
                {"Stromiteration, LU", new CurrentIteration(targetPrecision, maximumIterations, false)},
                {"Newton-Raphson, iterativ", new NewtonRaphsonMethod(targetPrecision, maximumIterations, true)},
                {"Newton-Raphson, LU", new NewtonRaphsonMethod(targetPrecision, maximumIterations, false)},
                {"HELM, 1000 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 200, 1000, false)},
                {"HELM, 10000 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 300, 10000, false)}
            };

            var file = new StreamWriter("results_small.csv", false);
            file.WriteLine("Verfahren;Konvergenzgrenze [W]");

            foreach (var calculator in calculators)
            {
                Console.WriteLine("next calculator: " + calculator.Key);
                var convergenceBorder = FindConvergenceBorderSmallNet(calculator.Value);
                Console.WriteLine("convergence border for " + calculator.Key + ": " + convergenceBorder);
                file.WriteLine(calculator.Key + ";" + convergenceBorder);
                file.Flush();
            }

            file.Close();
        }

        private static double FindUnstableLoad(INodeVoltageCalculator calculator)
        {
            var result = 1e7;

            while (true)
            {
                Console.WriteLine("testing " + result);
                var convergence = CheckConvergence(calculator, result);

                if (!convergence)
                    return result;

                result = result*10;
            }
        }

        private static double FindConvergenceBorderSmallNet(INodeVoltageCalculator calculator)
        {
            var lowerLimit = 0.0;
            var upperLimit = 1.0;

            while (upperLimit - lowerLimit > 1e-10)
            {
                var middle = (upperLimit + lowerLimit) / 2;
                Console.WriteLine("testing " + middle);
                var convergence = CheckConvergenceSmallNet(calculator, middle);

                if (convergence)
                    lowerLimit = middle;
                else
                    upperLimit = middle;
            }

            return lowerLimit;
        }

        private static double FindConvergenceBorderInRange(double lowerLimit, double upperLimit,
            INodeVoltageCalculator calculator)
        {
            const double relativePrecision = 1e-7;
            while ((upperLimit - lowerLimit)/upperLimit > relativePrecision && (upperLimit - lowerLimit) > 10)
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

        private delegate bool CalculateNodeVoltagesAsync(INodeVoltageCalculator calculator, out double relativePowerError);

        private static bool CheckConvergence(INodeVoltageCalculator calculator, double additionalLoad)
        {
            var powerNet = new PowerNetDatabaseAdapter("data/vorstadt_files/database.mdb");
            powerNet.AddLoad(1280, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1111, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1110, new Complex(additionalLoad, 0));
            powerNet.AddLoad(740, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1016, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1044, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1046, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1207, new Complex(additionalLoad, 0));
            powerNet.AddLoad(1215, new Complex(additionalLoad, 0));
            bool convergence;
            calculator.ResetProgress();

            try
            {
                double relativePowerError;
                var asyncCaller =
                    new CalculateNodeVoltagesAsync(powerNet.CalculateNodeVoltages);
                var result = asyncCaller.BeginInvoke(calculator, out relativePowerError, null, null);
                var previousProgress = 0.0;

                while (!result.IsCompleted)
                {
                    var progress = calculator.Progress;

                    if (progress > previousProgress && progress < 1)
                    {
                        Console.WriteLine(progress*100 + "% done");
                        previousProgress = progress;
                    }
                    else
                        Thread.Sleep(100);
                }

                Console.WriteLine(100 + "% done");
                convergence = asyncCaller.EndInvoke(out relativePowerError, result);
            }
            catch (Exception)
            {
                convergence = false;
            }

            return convergence;
        }

        private static bool CheckConvergenceSmallNet(INodeVoltageCalculator calculator, double load)
        {
            var admittances = new AdmittanceMatrix(2);
            admittances.AddConnection(0, 1, new Complex(1, 1));
            var powerNet = new PowerNetComputable(calculator, admittances, 1);
            powerNet.AddNode(new SlackNode(new Complex(1, 0)));
            powerNet.AddNode(new PqNode(new Complex((-1)*load, (-1)*load/2)));
            bool convergence;

            try
            {
                double relativePowerError;
                powerNet.CalculateNodeResults(out relativePowerError);
                convergence = relativePowerError < 1e-10;
            }
            catch (Exception)
            {
                convergence = false;
            }

            return convergence;
        }
    }
}
