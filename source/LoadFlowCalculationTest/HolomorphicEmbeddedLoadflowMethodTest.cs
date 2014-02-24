using System.Linq;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class HolomorphicEmbeddedLoadflowMethodTest : LoadFlowCalculatorHighAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new HolomorphicEmbeddedLoadFlowMethod(0.0001, 50);
        }

        [TestMethod]
        public void CalculateVoltagePowerSeries_oneNodeProblem_correctCoefficients()
        {
            var admittances = DenseMatrix.OfArray(new [,]{{new Complex(1000, 0)}});
            var currents = new DenseVector(new[] {new Complex(1000, 0)});
            var powers = new DenseVector(new[] {new Complex(-1, 0)});
            var calculatorHelm = new HolomorphicEmbeddedLoadFlowMethod(0.0001, 4);

            var allPowerSeries = calculatorHelm.CalculateVoltagePowerSeries(admittances, currents, powers);

            Assert.AreEqual(1, allPowerSeries.Count());
            var powerSeries = allPowerSeries[0];
            Assert.AreEqual(3, powerSeries.Degree);
            ComplexAssert.AreEqual(new Complex(-1, 0), powerSeries[0], 0.0001);
            ComplexAssert.AreEqual(new Complex(2.001, 0), powerSeries[1], 0.0001);
            ComplexAssert.AreEqual(new Complex(2.001E-3, 0), powerSeries[2], 0.0000001);
            ComplexAssert.AreEqual(new Complex(4.006002E-3, 0), powerSeries[3], 0.0000001);
        }
    }
}
