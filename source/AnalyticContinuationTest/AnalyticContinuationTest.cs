using System;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public abstract class AnalyticContinuationTest
    {
        protected PowerSeries<double> _powerSeriesExponential;
        protected PowerSeries<double> _powerSeriesSine;
        protected PowerSeries<Complex> _powerSeriesVoltage;
        protected IAnalyticContinuation<double> _continuationExponential;
        protected IAnalyticContinuation<double> _continuationSine;
        protected IAnalyticContinuation<Complex> _continuationVoltage;

        protected abstract IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries);
        protected abstract IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries);

        [TestInitialize]
        public void SetUp()
        {
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            _powerSeriesSine = PowerSeriesDouble.CreateSine(10, new CalculatorDouble());
            _powerSeriesVoltage = CreateHelmVoltageSeriesComplex(50);
            _continuationExponential = CreateAnalyticContinuation(_powerSeriesExponential);
            _continuationSine = CreateAnalyticContinuation(_powerSeriesSine);
            _continuationVoltage = CreateAnalyticContinuation(_powerSeriesVoltage);

        }

        [TestMethod]
        public void Evaluate_ExponentialAt2_CorrectResult()
        {
            const double x = 2.0;

            var result = _continuationExponential.Evaluate(x);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(Math.Exp(x), result, 0.01);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt15_CorrectResult()
        {
            const double x = 1.5;

            var result = _continuationExponential.Evaluate(x);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(Math.Exp(x), result, 0.001);
        }

        [TestMethod]
        public void Evaluate_SineAt1_CorrectResult()
        {
            const double x = 1;

            var result = _continuationSine.Evaluate(x);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(Math.Sin(x), result, 0.001);
        }

        [TestMethod]
        public void EvaluateAt1_Empty_SameAsEvaluateCalledWith1()
        {
            Assert.AreEqual(_continuationExponential.Evaluate(1), _continuationExponential.EvaluateAt1());
        }

        [TestMethod]
        public void Evaluate_ZeroOfLaguerrePolynom_CorrectResult()
        {
            var series = CreateLaguerreSeries(8);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(0.5857864375, result, 0.0001);
        }

        [TestMethod]
        public void Evaluate_EulerSeries_MoreAccurateThanDirectPowerSeries()
        {
            var series = CreateEulerSeries(8);
            var continuation = CreateAnalyticContinuation(series);
            var directValue = series.Evaluate(1);
            var continuatedValue = continuation.Evaluate(1);
            const double correctValue = Math.PI*Math.PI/6;
            var directError = Math.Abs(directValue - correctValue);
            var continuatedError = Math.Abs(continuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(continuatedValue));
            Assert.IsFalse(Double.IsNaN(directValue));
            Assert.IsFalse(Double.IsInfinity(continuatedValue));
            Assert.IsFalse(Double.IsInfinity(directValue));
            Assert.AreEqual(correctValue, directValue, 0.2);
            Assert.AreEqual(correctValue, continuatedValue, 0.2);
            Assert.IsTrue(directError > continuatedError);
        }

        [TestMethod]
        public void Evaluate_Ln2Series_CorrectResult()
        {
            var series = CreateLn2Series(8);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(Math.Log(2), result, 0.0001);
        }

        public static PowerSeriesDouble CreateLaguerreSeries(int n)
        {
            var sums = new double[n];
            sums[0] = 0;

            for (var i = 1; i < n; ++i)
                sums[i] = (sums[i - 1]*sums[i - 1] + 2)/4;

            var powerSeries = new PowerSeriesDouble(n);

            for (var i = 1; i < n; ++i)
                powerSeries[i] = sums[i] - sums[i - 1];

            return powerSeries;
        }

        public static PowerSeriesDouble CreateEulerSeries(int n)
        {
            var powerSeries = new PowerSeriesDouble(n);

            for (var i = 1; i < n; ++i)
                powerSeries[i] = 1.0/(i*i);

            return powerSeries;
        }

        public static PowerSeriesComplex CreateHelmVoltageSeriesComplex(int n)
        {
            var coefficients = CreateHelmVoltageSeriesCoefficients();

            var series = new PowerSeriesComplex(n);

            for (var i = 0; i < n; ++i)
                series[i] = coefficients[i];

            return series;
        }

        public static PowerSeriesDecimalComplex CreateHelmVoltageSeriesDecimalComplex(int n)
        {
            var coefficients = CreateHelmVoltageSeriesCoefficients();

            var series = new PowerSeriesDecimalComplex(n);

            for (var i = 0; i < n; ++i)
                series[i] = new DecimalComplex(coefficients[i]);

            return series;
        }

        private static Complex[] CreateHelmVoltageSeriesCoefficients()
        {
            var coefficients = new[]
            {
                new Complex(-1, 1.6694600439147E-16),
                new Complex(1.67974139124394, 0.504426030619388),
                new Complex(-0.114101479265145, 0.639980192400492),
                new Complex(1.0070656018567, 1.23390998718222),
                new Complex(4.47716579955149, 2.07378931269298),
                new Complex(13.1100639245265, 2.76508536326347),
                new Complex(33.9923988814818, 1.81009331066554),
                new Complex(88.4221555238277, -5.37794009669852),
                new Complex(242.798088344867, -32.2960729246381),
                new Complex(701.18479193071, -118.495662895541),
                new Complex(2089.9438132595, -379.147926816592),
                new Complex(6368.51500636798, -1146.94332811231),
                new Complex(19815.2177126681, -3381.00896328972),
                new Complex(62905.1727643245, -9864.07044354301),
                new Complex(203126.098913054, -28764.6180671557),
                new Complex(664515.281105212, -84376.8776120749),
                new Complex(2195349.85010498, -249858.806746953),
                new Complex(7308960.04118312, -748045.744209336),
                new Complex(24489453.6805216, -2264954.67552451),
                new Complex(82499017.8530824, -6933659.79338325),
                new Complex(279205898.07835, -21447535.2715492),
                new Complex(948717638.734695, -66983956.3152059),
                new Complex(3235030793.65458, -211046748.313325),
                new Complex(11065942751.3487, -670258582.001958),
                new Complex(37961031871.9188, -2144036456.76953),
                new Complex(130562729957.63, -6903283349.15794),
                new Complex(450131528696.145, -22359251226.9753),
                new Complex(1555314579444.13, -72813750664.6764),
                new Complex(5384994280710.81, -238302468125.983),
                new Complex(18680056512819.2, -783485488703.063),
                new Complex(64914465552115.1, -2586838250621.72),
                new Complex(225956671082360, -8574508951907.78),
                new Complex(787742084261600, -28525357536315.9),
                new Complex(2.75028126567498E+15, -95219783202426.4),
                new Complex(9.6153919663255E+15, -318860257043707),
                new Complex(3.36604212373324E+16, -1.07093379585647E+15),
                new Complex(1.17978663006263E+17, -3.6068967938863E+15),
                new Complex(4.13990136645768E+17, -1.21798085952344E+16),
                new Complex(1.45429534050547E+18, -4.12302371501503E+16),
                new Complex(5.11407215883769E+18, -1.39893742001937E+17),
                new Complex(1.80015568082651E+19, -4.75697876930109E+17),
                new Complex(6.34251144154713E+19, -1.62092314507791E+18),
                new Complex(2.2366649570658E+20, -5.53405823233453E+18),
                new Complex(7.89425124043963E+20, -1.89291248126283E+19),
                new Complex(2.78852396383137E+21, -6.48606874590464E+19),
                new Complex(9.85769688141809E+21, -2.22617054453466E+20),
                new Complex(3.48738120817103E+22, -7.65289170633181E+20),
                new Complex(1.23461757507178E+23, -2.63481304284825E+21),
                new Complex(4.37382701455794E+23, -9.08448369839981E+21),
                new Complex(1.55051048068301E+24, -3.13652011053964E+22),
            };
            return coefficients;
        }

        public static PowerSeriesDouble CreateLn2Series(int n)
        {
            var series = new PowerSeriesDouble(n);

            for (var i = 0; i < n; ++i)
            {
                if (i%2 == 0)
                    series[i] = 1.0/(i + 1);
                else
                    series[i] = -1.0/(i + 1);
            }

            return series;
        }
    }
}
