using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using SincalConnector;

namespace ConvergenceExperiment
{
    class Program
    {
        private static void Main(string[] args)
        {
            const int maximumIterations = 100;
            const double targetPrecision = 1e-10;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"HELM, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false)},
                {"Stromiteration, iterativ", new CurrentIteration(targetPrecision, maximumIterations, true)},
                {"HELM (LU) mit Stromiteration (iterativ)", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false), new CurrentIteration(targetPrecision, maximumIterations, true))}
            };
            var file = new StreamWriter("results.csv", false);
            file.WriteLine("method;relative power error;elapsed time [s]");
            var powerNet = new PowerNetDatabaseAdapter(
                "C:\\Users\\benediktibk\\Desktop\\modifiziert\\einphasig, ohne Regelstufen, ohne geregelte Generatoren, ohne PV, ohne Generatoren\\10_2015_files\\database.mdb", 0.1);
            var stopWatch = new Stopwatch();

            foreach (var calculator in calculators)
            {
                Console.WriteLine("next calculator: " + calculator.Key);

                stopWatch.Restart();
                var relativePowerError = CalculateRelativePowerError(calculator.Value, powerNet);
                stopWatch.Stop();
                Console.WriteLine("relative power error: " + relativePowerError);
                file.WriteLine(calculator.Key + ";" + relativePowerError + ";" + stopWatch.Elapsed.TotalSeconds);
                file.Flush();
            }

            file.Close();
        }

        private static double CalculateRelativePowerError(INodeVoltageCalculator calculator, PowerNetDatabaseAdapter powerNet)
        {
            double result;
             powerNet.CalculateNodeVoltages(calculator, out result);
            return result;
        }
    }
}
