using System;
using System.IO;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            var powerNet = new PowerNet(2, 1);
            powerNet.Admittances.AddConnection(0, 1, new Complex(1, 0));
            var feedInNode = new Node() { Voltage = new Complex(1, 0) };
            var loadNode = new Node() { Power = new Complex(-0.23, 0) };
            powerNet.SetNode(0, feedInNode);
            powerNet.SetNode(1, loadNode);
            const int coefficientCount = 15;
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.0000001, coefficientCount,
                new PrecisionLongDouble());
            var calculator = new LoadFlowCalculator(nodeVoltageCalculator);

            powerNet.CalculateMissingInformation(calculator);

            var loadVoltage = powerNet.NodeVoltages[1].Real;
            Console.WriteLine("load voltage: " + loadVoltage);
            Console.WriteLine("error of load voltage: " + Math.Abs(1 - loadVoltage/0.641421356)*100 + "%");

            using (var file = new StreamWriter(@"C:\Users\Public\coefficients.csv", false))
            {
                for (var i = 0; i < coefficientCount; ++i)
                {
                    var coefficient = nodeVoltageCalculator.GetCoefficients(i)[0].Real;
                    file.WriteLine(coefficient + ";");
                }
            }

            Console.ReadKey();
        }
    }
}
