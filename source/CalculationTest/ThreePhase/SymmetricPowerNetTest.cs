using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace CalculationTest.ThreePhase
{
    [TestClass]
    public class SymmetricPowerNetTest
    {
        #region variables

        private SymmetricPowerNet _powerNet;
        private INodeVoltageCalculator _newtonRaphsonCalculator;
        private INodeVoltageCalculator _helmCalculator;
        private INodeVoltageCalculator _currentIterationCalculator;

        #endregion

        #region setup

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new SymmetricPowerNet(50);
            _newtonRaphsonCalculator = new NewtonRaphsonMethod(0.0000001, 1000);
            _helmCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00000001, 80, new PrecisionMulti(200), true);
            _currentIterationCalculator = new CurrentIteration(0.0000001, 1000);
        }

        #endregion

        #region verification with SINCAL

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoad_CorrectResults()
        {
            const double omega = 2*Math.PI*50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddLine(1, 2, 0.1, 0.4/omega, 100/(400*400), 1e-10, 1);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(392.367, -2.850*Math.PI/180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_Transformer_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI/180), 0, 1.1, 1, "");
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.001*3000, 10, 0.01, 2.5, "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2*Math.PI/180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.007, -1.072 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLosses_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1, "");
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.001 * 3000, 0.00000001, 0.000000001, 2.5, "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.161, -1.063 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLossesAndNoInductance_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1, "");
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.0799999 * 3000, 0.0000000001, 0.00000000001, 2.5, "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            Assert.AreEqual(377.615, loadVoltage.Magnitude, 0.5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_Generator_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddNode(3, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(400, 0), 0, 1.1, 1, "");
            _powerNet.AddGenerator(3, 400, 15000);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddLine(1, 2, 0.1, 0.4 / omega, 100 / (400 * 400), 1e-10, 1);
            _powerNet.AddLine(2, 3, 0.1, 0.4 / omega, 100 / (400 * 400), 1e-10, 1);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var feedInVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var generatorVoltage = _powerNet.GetNodeVoltage(3);
            var feedInVoltageShouldBe = new Complex(400, 0);
            var generatorVoltageShouldBe = Complex.FromPolarCoordinates(400, 1.498*Math.PI/180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(396.310, -0.662*Math.PI/180);
            ComplexAssert.AreEqual(feedInVoltageShouldBe, feedInVoltage, 0.1);
            ComplexAssert.AreEqual(generatorVoltageShouldBe, generatorVoltage, 0.1);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CompletePowerNet_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddNode(3, 400, "");
            _powerNet.AddNode(4, 400, "");
            _powerNet.AddNode(5, 400, "");
            _powerNet.AddNode(6, 400, "");
            _powerNet.AddNode(7, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2*Math.PI/180), 0, 1.2, 1e6, "");
            _powerNet.AddTransformer(2, 4, 3000, 0.08, 0.001 * 3000, 10, 0.01, 2.5, "");
            _powerNet.AddGenerator(7, 400, 15000);
            _powerNet.AddLoad(3, new Complex(-5000, -1000));
            _powerNet.AddLoad(5, new Complex(-10000, 200));
            _powerNet.AddLoad(6, new Complex(-5000, -100));
            _powerNet.AddLine(1, 2, 0.1, 0.4 / omega, 100 / (1000 * 1000), 1e-10, 10);
            _powerNet.AddLine(2, 3, 0.1, 0.4 / omega, 100 / (1000 * 1000), 1e-10, 1);
            _powerNet.AddLine(4, 5, 0.1, 0.4 / omega, 100 / (400 * 400), 1e-10, 0.1);
            _powerNet.AddLine(5, 6, 0.1, 0.4 / omega, 100 / (400 * 400), 1e-10, 0.1);
            _powerNet.AddLine(6, 7, 0.1, 0.4 / omega, 100 / (400 * 400), 1e-10, 1);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            ComplexAssert.AreEqual(Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), _powerNet.GetNodeVoltage(1), 0.01);
            ComplexAssert.AreEqual(Complex.FromPolarCoordinates(989.668, 0.701 * Math.PI / 180), _powerNet.GetNodeVoltage(2), 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndLoad_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddNode(3, threePhaseFactor, "load");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1, "");
            _powerNet.AddGenerator(2, 0.95 * threePhaseFactor, 0.0855);
            _powerNet.AddLoad(3, new Complex(-0.0558, 0));
            _powerNet.AddLine(1, 2, 1, 0, 0, 0, 1);
            _powerNet.AddLine(2, 3, 1, 0, 0, 0, 1);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.95, 0, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.93, 0, _powerNet.GetNodeVoltage(3) / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithLoad_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "load");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1, "");
            _powerNet.AddLoad(2, new Complex(-0.1425, 0));
            _powerNet.AddLine(1, 2, 1, 0, 0, 0, 1);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.95, 0, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MinimalExampleWithGenerator_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1, "");
            _powerNet.AddGenerator(2, Math.Sqrt(0.95*0.95 + 0.1*0.1) * threePhaseFactor, -0.09375);
            _powerNet.AddLine(1, 2, 1, 1 / omega, 0, 0, 1);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.95, 0.1, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.000001);
        }

        #endregion
    }
}
