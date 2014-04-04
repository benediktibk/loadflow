using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Factorization;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Experiment
{
    class Program
    {
        private static IList<Complex> _coefficientsVoltageOne;
        private static IList<Complex> _coefficientsVoltageOneSquared;
        private static IList<Complex> _coefficientsVoltageOneWeighted;
        private static IList<Complex> _coefficientsVoltageTwo;
        private static IList<Complex> _coefficientsVoltageTwoInverse;
        private static IList<Complex> _coefficientsVoltageTwoConjugated;
        private static Matrix<Complex> _admittanceMatrixReduced;
        private static Complex _admittanceOneZero;
        private static Complex _admittanceTwoZero;
        private static Complex _slackVoltage;
        private static double _powerOne;
        private static Complex _powerTwo;
        private static double _voltageMagnitudeOne;
        private static QR _factorization;

        static void Main(string[] args)
        {
            const int coefficientCount = 60;
            _coefficientsVoltageOne = new List<Complex>(coefficientCount);
            _coefficientsVoltageOneSquared = new List<Complex>(coefficientCount);
            _coefficientsVoltageOneWeighted = new List<Complex>(coefficientCount);
            _coefficientsVoltageTwo = new List<Complex>(coefficientCount);
            _coefficientsVoltageTwoInverse = new List<Complex>(coefficientCount);
            _coefficientsVoltageTwoConjugated = new List<Complex>(coefficientCount);
            _admittanceMatrixReduced =
                DenseMatrix.OfArray(new[,]
                {{new Complex(15, 1500), new Complex(-10, -1000)}, {new Complex(-10, -1000), new Complex(30, 700)}});
            _factorization = _admittanceMatrixReduced.QR();
            _admittanceOneZero = new Complex(-5, -500);
            _admittanceTwoZero = new Complex(-20, 300);
            _slackVoltage = new Complex(1, 0.12);
            _powerOne = -46.86;
            _powerTwo = new Complex(30.23, -58.52);
            _voltageMagnitudeOne = Math.Sqrt(0.9*0.9 + 0.1*0.1);

            for (var i = 0; i < coefficientCount; ++i)
                CalculateNextCoefficients();

            var powerSeriesVoltageOne = new PowerSeriesComplex(coefficientCount);
            var powerSeriesVoltageTwo = new PowerSeriesComplex(coefficientCount);

            for (var i = 0; i < coefficientCount; ++i)
            {
                powerSeriesVoltageOne[i] = _coefficientsVoltageOne[i];
                powerSeriesVoltageTwo[i] = _coefficientsVoltageTwo[i];
            }

            var continuationVoltageOne = new EpsilonAlgorithm<Complex>(powerSeriesVoltageOne);
            var continuationVoltageTwo = new EpsilonAlgorithm<Complex>(powerSeriesVoltageTwo);
            var voltageOne = continuationVoltageOne.EvaluateAt1();
            var correctOne = Math.Abs(0.9 - voltageOne.Real) < 0.001 && Math.Abs(0.1 - voltageOne.Imaginary) < 0.001;
            var voltageTwo = continuationVoltageTwo.EvaluateAt1();
            var correctTwo = Math.Abs(0.95 - voltageTwo.Real) < 0.001 && Math.Abs(0.05 - voltageTwo.Imaginary) < 0.001;
            Console.WriteLine("result voltage one: " + voltageOne);
            Console.WriteLine(correctOne ? "the result is correct" : "the result is incorrect");
            Console.WriteLine("result voltage two: " + voltageTwo);
            Console.WriteLine(correctTwo ? "the result is correct" : "the result is incorrect");
            Console.ReadKey();
        }

        static void CalculateNextCoefficients()
        {
            if (_coefficientsVoltageOne.Count == 0)
                CalculateFirstCoefficients();
            else
                CalculateNextCoefficientsInternal();

            CalculateNextDerivedCoefficients();
        }

        private static void CalculateNextDerivedCoefficients()
        {
            CalculateNextInverseCoefficient();
            CalculateNextSquaredCoefficient();
            CalculateNextConjugatedCoefficient();
            CalculateNextWeightedCoefficient();
        }

        private static void CalculateNextWeightedCoefficient()
        {
            _coefficientsVoltageOneWeighted.Add(CalculateConvolution(_coefficientsVoltageOneSquared, _coefficientsVoltageTwoConjugated));
        }

        private static void CalculateNextConjugatedCoefficient()
        {
            _coefficientsVoltageTwoConjugated.Add(_coefficientsVoltageTwo.Last().Conjugate());
        }

        private static void CalculateNextSquaredCoefficient()
        {
            _coefficientsVoltageOneSquared.Add(CalculateConvolution(_coefficientsVoltageOne, _coefficientsVoltageOne));
        }

        private static Complex CalculateConvolution(IList<Complex> one, IList<Complex> two)
        {
            var n = one.Count - 1;
            var result = new Complex();

            for (var i = 0; i <= n; ++i)
                result += one[i]*two[n - i];

            return result;
        }

        private static void CalculateNextInverseCoefficient()
        {
	        var n = _coefficientsVoltageTwo.Count - 1;

            if (n == 0)
            {
                _coefficientsVoltageTwoInverse.Add(1/_coefficientsVoltageTwo[0]);
                return;
            }
            
            var result = new Complex();
	        for (var i = 0; i < n; ++i)
		        result -= _coefficientsVoltageTwo[n - i]*_coefficientsVoltageTwoInverse[i];

            _coefficientsVoltageTwoInverse.Add(result/_coefficientsVoltageTwo[0]);
        }

        private static void CalculateNextCoefficientsInternal()
        {
            var rightHandSide = new DenseVector(2);
            var voltageMagnitudeSquare = _voltageMagnitudeOne*_voltageMagnitudeOne;
            rightHandSide[0] = (2*_powerOne/voltageMagnitudeSquare - _admittanceMatrixReduced[0, 0].Conjugate())*
                               _coefficientsVoltageOne.Last() -
                               _admittanceOneZero.Conjugate()*_slackVoltage.Conjugate()/voltageMagnitudeSquare*
                               _coefficientsVoltageOneSquared.Last() -
                               _admittanceMatrixReduced[0, 1].Conjugate()/voltageMagnitudeSquare*
                               _coefficientsVoltageOneWeighted.Last();
            rightHandSide[1] = (_powerTwo*_coefficientsVoltageTwoInverse.Last()).Conjugate();
            var coefficients = _factorization.Solve(rightHandSide);
            _coefficientsVoltageOne.Add(coefficients[0]);
            _coefficientsVoltageTwo.Add(coefficients[1]);
        }

        private static void CalculateFirstCoefficients()
        {
            var rightHandSide = new DenseVector(new[] {(-1)*_admittanceOneZero*_slackVoltage, (-1)*_admittanceTwoZero*_slackVoltage});
            var coefficients = _factorization.Solve(rightHandSide);
            _coefficientsVoltageOne.Add(coefficients[0]);
            _coefficientsVoltageTwo.Add(coefficients[1]);
        }
    }
}
