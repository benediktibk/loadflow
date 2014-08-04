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

        #endregion

        #region setup

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new SymmetricPowerNet(50);
            _newtonRaphsonCalculator = new NewtonRaphsonMethod(0.000001, 1000);
            _helmCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.000001, 80, new PrecisionMulti(200), true);
        }

        #endregion

        #region verification with SINCAL

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoad_CorrectResults()
        {
            const double omega = 2*Math.PI*50;
            _powerNet.AddNode(1, 400);
            _powerNet.AddNode(2, 400);
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
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
            _powerNet.AddNode(1, 1000);
            _powerNet.AddNode(2, 400);
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI/180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.001*3000, 10, 0.01, 2.5, 0);

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
            _powerNet.AddNode(1, 1000);
            _powerNet.AddNode(2, 400);
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.001 * 3000, 0.00000001, 0.000000001, 2.5, 0);

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
            _powerNet.AddNode(1, 1000);
            _powerNet.AddNode(2, 400);
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTransformer(1, 2, 3000, 0.08, 0.0799999 * 3000, 0.0000000001, 0.00000000001, 2.5, 0);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            Assert.AreEqual(377.615, loadVoltage.Magnitude, 0.5);
        }

        #endregion
    }
}
