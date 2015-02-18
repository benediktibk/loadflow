using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            const int maximumIterations = 100;
            const double targetPrecision = 1e-10;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"HELM, 64 Bit, iterativ", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true)},
                {"HELM mit Stromiteration, 64 Bit, iterativ", new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true), new CurrentIteration(targetPrecision, maximumIterations, true))},
                {"Stromiteration, iterativ", new CurrentIteration(targetPrecision, maximumIterations, true)}
            };
            var file = new StreamWriter("results.csv", false);
            file.WriteLine("method;relative power error;elapsed time");
            var powerNet = new PowerNetDatabaseAdapter(
                "C:\\Users\\benediktibk\\Desktop\\modifiziert\\einphasig, ohne Regelstufen, ohne geregelte Generatoren, ohne PV, ohne Generatoren\\10_2015_files\\database.mdb", 0.5);
            var stopWatch = new Stopwatch();

            foreach (var calculator in calculators)
            {
                Console.WriteLine("next calculator: " + calculator.Key);

                stopWatch.Restart();
                var relativePowerError = CalculateRelativePowerError(calculator.Value, powerNet);
                Console.WriteLine("relative power error: " + relativePowerError);
                stopWatch.Stop();
                file.WriteLine(calculator.Key + ";" + relativePowerError + ";" + stopWatch.Elapsed.TotalSeconds);
                file.Flush();
            }

            file.Close();
        }

        private static double CalculateRelativePowerError(INodeVoltageCalculator calculator, PowerNetDatabaseAdapter powerNet)
        {
            double result;

            var success = powerNet.CalculateNodeVoltages(calculator, out result);

            if (!success)
                result = 1e10;

            return result;
        }
    }
}
