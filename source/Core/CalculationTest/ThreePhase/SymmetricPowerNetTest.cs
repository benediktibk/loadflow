﻿using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using MathNet.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

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
            _helmCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00000001, 80, new PrecisionMulti(200));
            _currentIterationCalculator = new CurrentIteration(0.0000001, 1000);
        }

        #endregion

        #region verification with SINCAL

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoad_CorrectResults()
        {
            const double omega = 2*Math.PI*50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddTransmissionLine(1, 2, 0.1, 0.4/omega, 100.0/(400*400), 1e-10, 1, true);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(392.363, -2.852*Math.PI/180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndTransmissionEquationModel_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 10000.0 / (400 * 400), 1e-7, 1, true);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(391.693, -3.212 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistance_CorrectResults()
        {
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, 0));
            _powerNet.AddTransmissionLine(1, 2, 0.1, 0, 0, 0, 1, false);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(394.936, 0 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistanceAndGroundLosses_CorrectResults()
        {
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, 0));
            _powerNet.AddTransmissionLine(1, 2, 0.05, 0, 50000.0 / (400 * 400), 0, 1, false);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(394.383, 0 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndOnlyResistanceAndShuntCapacity_CorrectResults()
        {
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, 20000));
            _powerNet.AddTransmissionLine(1, 2, 0.05, 0, 0, 1e-5, 1, false);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(397.476, -0.365 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoadAndSimplifiedModel_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 10000.0 / (400 * 400), 1e-7, 1, false);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(390.845, -3.561 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_Transformer_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI/180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.001*3000, 10, 0.01, 2.5, new Angle(), "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2*Math.PI/180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.007, -1.072 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLosses_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.001 * 3000, 0.00000001, 0.000000001, 2.5, new Angle(), "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(399.161, -1.063 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_TransformerWithNearlyNoIronLossesAndNoInductance_CorrectResults()
        {
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180), 0, 1.1, 1);
            _powerNet.AddLoad(2, new Complex(-2000, 0));
            _powerNet.AddTwoWindingTransformer(1, 2, 3000, 0.08, 0.0799999 * 3000, 0.0000000001, 0.00000000001, 2.5, new Angle(), "");

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(1000, 2 * Math.PI / 180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            Assert.AreEqual(377.615, loadVoltage.Magnitude, 0.5);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_Generator_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddNode(2, 400, "");
            _powerNet.AddNode(3, 400, "");
            _powerNet.AddFeedIn(1, Complex.FromPolarCoordinates(400, 0), 0, 1.1, 1);
            _powerNet.AddGenerator(3, 400, 15000);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddTransmissionLine(1, 2, 0.1, 0.4 / omega, 100.0 / (400 * 400), 1e-10, 1, true);
            _powerNet.AddTransmissionLine(2, 3, 0.1, 0.4 / omega, 100.0 / (400 * 400), 1e-10, 1, true);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            var feedInVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var generatorVoltage = _powerNet.GetNodeVoltage(3);
            var feedInVoltageShouldBe = new Complex(400, 0);
            var generatorVoltageShouldBe = Complex.FromPolarCoordinates(400, 1.487*Math.PI/180);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(396.306, -0.670*Math.PI/180);
            ComplexAssert.AreEqual(feedInVoltageShouldBe, feedInVoltage, 0.01);
            ComplexAssert.AreEqual(generatorVoltageShouldBe, generatorVoltage, 0.1);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndLoad_CorrectResults()
        {
            const double omega = 2 * Math.PI * 50;
            var threePhaseFactor = Math.Sqrt(3);
            var Z = new Complex(1, 1);
            var U1 = new Complex(1, 0);
            var U2 = new Complex(0.95, 0.1);
            var U3 = new Complex(0.93, 0.08);
            var I1 = (U1 - U2)/Z;
            var I2 = (U2 - U3)/Z;
            var load = (-1)*U3*I2.Conjugate();
            var generatorLoad = U2*(I2 - I1).Conjugate();
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddNode(3, threePhaseFactor, "load");
            _powerNet.AddFeedIn(1, U1 * threePhaseFactor, 0, 1, 1);
            _powerNet.AddGenerator(2, U2.Magnitude * threePhaseFactor, generatorLoad.Real * 3);
            _powerNet.AddLoad(3, load * 3);
            _powerNet.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(2, 3, 1, 1 / omega, 0, 0, 1, true);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(U1, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(U2, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(U3, _powerNet.GetNodeVoltage(3) / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_MinimalExampleWithLoad_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "load");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1);
            _powerNet.AddLoad(2, new Complex(-0.1425, 0));
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 1, true);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.000001);
            ComplexAssert.AreEqual(0.95, 0, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_MinimalExampleWithGenerator_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1);
            _powerNet.AddGenerator(2, Math.Sqrt(0.95*0.95 + 0.1*0.1) * threePhaseFactor, 0.09375);
            _powerNet.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            _powerNet.CalculateNodeVoltages(_currentIterationCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.00001);
            ComplexAssert.AreEqual(0.95, 0.1, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.00001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndNewtonRaphson_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1);
            _powerNet.AddGenerator(2, Math.Sqrt(0.95 * 0.95 + 0.1 * 0.1) * threePhaseFactor, 0.09375);
            _powerNet.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            _powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.001);
            ComplexAssert.AreEqual(0.95, 0.1, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_MinimalExampleWithGeneratorAndHELM_CorrectResults()
        {
            var threePhaseFactor = Math.Sqrt(3);
            const double omega = 2 * Math.PI * 50;
            _powerNet.AddNode(1, threePhaseFactor, "feed in");
            _powerNet.AddNode(2, threePhaseFactor, "generator");
            _powerNet.AddFeedIn(1, new Complex(threePhaseFactor, 0), 0, 1, 1);
            _powerNet.AddGenerator(2, Math.Sqrt(0.95 * 0.95 + 0.1 * 0.1) * threePhaseFactor, 0.09375);
            _powerNet.AddTransmissionLine(1, 2, 1, 1 / omega, 0, 0, 1, true);

            _powerNet.CalculateNodeVoltages(_helmCalculator);

            ComplexAssert.AreEqual(1, 0, _powerNet.GetNodeVoltage(1) / threePhaseFactor, 0.00001);
            ComplexAssert.AreEqual(0.95, 0.1, _powerNet.GetNodeVoltage(2) / threePhaseFactor, 0.00001);
        }

        #endregion

        #region other tests

        [TestMethod]
        public void CalculateNodeVoltages_NetWithOneTransmissionLine_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1000 * Math.Sqrt(3), "");
            _powerNet.AddNode(1, 1000 * Math.Sqrt(3), "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0) * Math.Sqrt(3), 0, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddLoad(1, new Complex(-3e5, 0));

            var voltageCollapse = !_powerNet.CalculateNodeVoltages(_newtonRaphsonCalculator);

            Assert.IsFalse(voltageCollapse);
            var targetVoltageShouldBe = (0.5 + Math.Sqrt(0.15)) * 1e3 * Math.Sqrt(3);
            var voltageDrop = 1e3 - targetVoltageShouldBe / Math.Sqrt(3);
            var powerLoss = voltageDrop * voltageDrop * 3;
            var loadPowerShouldBe = 3e5 + powerLoss;
            ComplexAssert.AreEqual(1000 * Math.Sqrt(3), 0, _powerNet.GetNodeVoltage(0), 0.01);
            ComplexAssert.AreEqual(targetVoltageShouldBe, 0, _powerNet.GetNodeVoltage(1), 0.01);
            ComplexAssert.AreEqual(loadPowerShouldBe, 0, _powerNet.GetNodePower(0), 0.01);
            ComplexAssert.AreEqual(-3e5, 0, _powerNet.GetNodePower(1), 0.01);
        }

        #endregion
    }
}