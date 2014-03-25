using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            var slackVoltage = new Complex(1.05, 0);
            const double voltageMagnitude = 1.02;
            const double voltageMagnitudeSquared = voltageMagnitude*voltageMagnitude;
            var admittance = new Complex(0, 50);
            const int coefficientCount = 20;
            var coefficients = new List<Complex>(coefficientCount);
            coefficients.Add(slackVoltage);

            while (coefficients.Count < coefficientCount)
            {
                var nextCoefficient = CalculateNextCoefficient(coefficients, slackVoltage, admittance,
                    voltageMagnitudeSquared);
                coefficients.Add(nextCoefficient);
            }

            var powerSeries = new PowerSeriesComplex(coefficientCount);

            for (var i = 0; i < coefficientCount; ++i)
                powerSeries[i] = coefficients[i];

            var continuation = new EpsilonAlgorithm<Complex>(powerSeries);
            var voltage = continuation.EvaluateAt1();
            var correct = Math.Abs(1.0198 - voltage.Real) < 0.001 && Math.Abs(-0.019 - voltage.Imaginary) < 0.001;
            Console.WriteLine("result voltage: " + voltage);
            Console.WriteLine(correct ? "the result is correct" : "the result is incorrect");
            Console.ReadKey();
        }

        static Complex CalculateNextCoefficient(IList<Complex> coefficients, Complex slackVoltage, Complex admittance, double voltageMagnitudeSquared)
        {
            var lastCoefficient = coefficients.Last();
            var lastSquaredCoefficient = CalculateNextSquaredCoefficient(coefficients);
            var constantCurrent = admittance * slackVoltage;
            var lastCombinedCoefficient = (-1) * admittance * lastCoefficient * voltageMagnitudeSquared;
            var rightHandSide = (2 * 1 * lastCoefficient - lastCombinedCoefficient + lastSquaredCoefficient * Complex.Conjugate(constantCurrent)) / voltageMagnitudeSquared;
            return rightHandSide/admittance;
        }

        static Complex CalculateNextSquaredCoefficient(IList<Complex> coefficients)
        {
            var n = coefficients.Count - 1;
            var coefficient = new Complex();

            for (var j = 0; j <= n; ++j)
                coefficient += coefficients[j] * coefficients[n - j];

            return coefficient;
        }
    }
}
