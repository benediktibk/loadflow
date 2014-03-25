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
            Console.WriteLine("result voltage: " + voltage);
            Console.ReadKey();
        }

        static Complex CalculateNextCoefficient(IList<Complex> coefficients, Complex slackVoltage, Complex admittance, double voltageMagnitudeSquared)
        {
            var lastCoefficient = coefficients.Last();
            var lastSquaredCoefficient = CalculateNextSquaredCoefficient(coefficients);
            return ((2/admittance + new Complex(voltageMagnitudeSquared, 0))*lastCoefficient -
                    slackVoltage*lastSquaredCoefficient)/voltageMagnitudeSquared;
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
