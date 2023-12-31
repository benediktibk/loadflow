﻿using System;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace CalculationIntegrationTest.SinglePhase.MultipleVoltageLevel
{
    [TestClass]
    public class PowerNetComputableTest
    {
        private PowerNetComputable _powerNet;
        private double _relativePowerError;

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNetComputable(50, new PowerNetFactory(new CurrentIteration(0.00001, 1000, true)), new NodeGraph());
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneFeedInAndOneLoad_CorrectResults()
        {
            _powerNet.AddNode(0, 100, "");
            _powerNet.AddNode(1, 100, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), new Complex());
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddTransmissionLine(0, 1, 10, 0, 0, 0, 1, true);

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(100, 0, nodeResults[0].Voltage, 0.000001);
            ComplexAssert.AreEqual(98.989794855663561963945681494118, 0, nodeResults[1].Voltage, 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_GeneratorAndLoad_CorrectResults()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), new Complex());
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(1.05, 0, nodeResults[0].Voltage, 0.001);
            ComplexAssert.AreEqual(1.0198, -0.019, nodeResults[1].Voltage, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 5, 700e-6, 0, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_DirectConnectionFromFeedInToLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 5, 700e-6, 0, 0, 0, true);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(1000, 0, nodeResults[1].Voltage, 0.01);
            ComplexAssert.AreEqual(2693.9, 4118.5, nodeResults[0].Power, 0.01);
            ComplexAssert.AreEqual(-2693.9, -4118.5, nodeResults[1].Power, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_DirectConnectionFromFeedInToLoadAndShuntValues_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 0, 0, 1e-3, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(-200, -400));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(1000, 0, nodeResults[1].Voltage, 0.01);
            ComplexAssert.AreEqual(1200, 400, nodeResults[0].Power, 0.01);
            ComplexAssert.AreEqual(-200, -400, nodeResults[1].Power, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoadWithNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 5, 700e-6, 0, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatio_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(397.065516574289, -4.58278572856559, nodeResults[1].Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(397.065516574289, -4.58278572856559, nodeResults[1].Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioSecondVersion_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1002, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1002, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(395.07823654009, -8.51263248957149, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndSmallLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-200, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(399.016841711796, -0.732873636155886, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNoLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(0, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(399.518772194282, 0.133486530903931, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransmissionLine_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddLoad(1, new Complex(-1e5, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            var targetVoltageShouldBe = (0.5 + Math.Sqrt(0.15)) * 1e3;
            var voltageDrop = 1e3 - targetVoltageShouldBe;
            var powerLoss = voltageDrop * voltageDrop;
            var loadPowerShouldBe = 1e5 + powerLoss;
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(targetVoltageShouldBe, 0, nodeResults[1].Voltage, 0.01);
            ComplexAssert.AreEqual(loadPowerShouldBe, 0, nodeResults[0].Power, 0.1);
            ComplexAssert.AreEqual(-1e5, 0, nodeResults[1].Power, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneImpedanceLoad_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 230, "");
            _powerNet.AddNode(1, 230, "");
            _powerNet.AddFeedIn(0, new Complex(230, 0), new Complex());
            _powerNet.AddImpedanceLoad(1, new Complex(22, 0));
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(230, 0, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(220, 0, nodeResults[1].Voltage, 0.00001);
            ComplexAssert.AreEqual(2300, 0, nodeResults[0].Power, 0.00001);
            ComplexAssert.AreEqual(0, 0, nodeResults[1].Power, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneCurrentSourceAndOneTransmissionLine_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 100, "");
            _powerNet.AddNode(1, 100, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), new Complex());
            _powerNet.AddCurrentSource(1, new Complex(10, 0), new Complex(2, 0));
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(100, 0, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(110, 0, nodeResults[1].Voltage, 0.00001);
            ComplexAssert.AreEqual(-1000, 0, nodeResults[0].Power, 0.00001);
            ComplexAssert.AreEqual(0, 0, nodeResults[1].Power, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneCurrentSource_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 100, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), new Complex());
            _powerNet.AddCurrentSource(0, new Complex(10, 0), new Complex(2, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(100, 0, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(-1000, 0, nodeResults[0].Power, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneCurrentSourceSimplified_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddCurrentSource(0, new Complex(0.1, 0), new Complex(2, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(-0.1, 0, nodeResults[0].Power, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_ThreeDirectConnectedNodes_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddNode(3, 1, "");
            _powerNet.AddNode(4, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 0.5, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(2, 3, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(3, 4, 1, 0, 0, 0, 0.5, false);
            _powerNet.AddLoad(4, new Complex(-0.09, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.95, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.95, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.95, 0, nodeResults[3].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[4].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.1, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(0, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(0, 0, nodeResults[2].Power, 1e-5);
            ComplexAssert.AreEqual(0, 0, nodeResults[3].Power, 1e-5);
            ComplexAssert.AreEqual(-0.09, 0, nodeResults[4].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_ThreeDirectConnectedNodesWithLoads_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddNode(3, 1, "");
            _powerNet.AddNode(4, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(2, 3, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(3, 4, 1, 0, 0, 0, 1, false);
            _powerNet.AddLoad(4, new Complex(-0.018, 0));
            _powerNet.AddLoad(1, new Complex(-0.05, 0));
            _powerNet.AddLoad(2, new Complex(-0.005, 0));
            _powerNet.AddLoad(3, new Complex(-0.0002, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.92, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.92, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.92, 0, nodeResults[3].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[4].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.08, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(-0.05, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(-0.005, 0, nodeResults[2].Power, 1e-5);
            ComplexAssert.AreEqual(-0.0002, 0, nodeResults[3].Power, 1e-5);
            ComplexAssert.AreEqual(-0.018, 0, nodeResults[4].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TwoDirectConnectedNodesWithLoadAndFeedIn_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 1, false);
            _powerNet.AddLoad(1, new Complex(-0.1, 0));
            _powerNet.AddLoad(2, new Complex(-0.09, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.2, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(-0.1, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(-0.09, 0, nodeResults[2].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TwoDirectConnectedFeedIns_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddFeedIn(1, new Complex(1, 0), new Complex());
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 0, false);
            _powerNet.AddLoad(2, new Complex(-1, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(1, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(1, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.5, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(0.5, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(-1, 0, nodeResults[2].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TwoFeedInsAtTheSameNode_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddLoad(0, new Complex(-1, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(0, 0, nodeResults[0].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_PvNodeAndLoadDirectConnected_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 0, false);
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddGenerator(1, 0.9, 0.1);
            _powerNet.AddLoad(2, new Complex(-0.19, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.1, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(0.1, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(-0.19, 0, nodeResults[2].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TwoPvNodesAndLoadDirectConnected_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddNode(3, 1, "");
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddTransmissionLine(1, 2, 1, 0, 0, 0, 0, false);
            _powerNet.AddTransmissionLine(2, 3, 1, 0, 0, 0, 0, false);
            _powerNet.AddFeedIn(0, new Complex(1, 0), new Complex());
            _powerNet.AddGenerator(1, 0.9, 0.08);
            _powerNet.AddGenerator(2, 0.9, 0.02);
            _powerNet.AddLoad(3, new Complex(-0.19, 0));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[1].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[2].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.9, 0, nodeResults[3].Voltage, 1e-5);
            ComplexAssert.AreEqual(0.1, 0, nodeResults[0].Power, 1e-5);
            ComplexAssert.AreEqual(0.08, 0, nodeResults[1].Power, 1e-5);
            ComplexAssert.AreEqual(0.02, 0, nodeResults[2].Power, 1e-5);
            ComplexAssert.AreEqual(-0.19, 0, nodeResults[3].Power, 1e-5);
        }

        [TestMethod]
        public void CalculateNodeVoltages_TwoFeedInsAtDifferentVoltageLevelSegments_ThrowsNoException()
        {
            _powerNet.AddNode(0, 2, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddNode(2, 1, "");
            _powerNet.AddFeedIn(0, new Complex(2, 0), new Complex());
            _powerNet.AddFeedIn(1, new Complex(1, 0), new Complex());
            _powerNet.AddFeedIn(2, new Complex(1, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 1, 0.01, 0.005, 0.005, 0.01, 2, new Angle(2), "");

            var result = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(result);
        }
    }
}
