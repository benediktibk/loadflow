using System;
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

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNetComputable(50, new PowerNetFactory(new CurrentIteration(0.00001, 1000)), new NodeGraph());
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneFeedInAndOneLoad_CorrectResults()
        {
            _powerNet.AddNode(0, 100, "");
            _powerNet.AddNode(1, 100, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddTransmissionLine(0, 1, 10, 0, 0, 0, 1, true);

            var nodeResults = _powerNet.CalculateNodeVoltages();

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
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);

            var nodeResults = _powerNet.CalculateNodeVoltages();

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
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 5, 700e-6, 0, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoadWithNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 5, 700e-6, 0, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatio_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTwoWindingTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(397.065516574289, -4.58278572856559, nodeResults[1].Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTwoWindingTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(397.065516574289, -4.58278572856559, nodeResults[1].Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioSecondVersion_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1002, 0), 0, 1.1, 1);
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1002, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(395.07823654009, -8.51263248957149, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndSmallLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(-200, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(399.016841711796, -0.732873636155886, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNoLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 400, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTwoWindingTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5, new Angle(), "");
            _powerNet.AddLoad(1, new Complex(0, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1000, 0, nodeResults[0].Voltage, 0.01);
            ComplexAssert.AreEqual(399.518772194282, 0.133486530903931, nodeResults[1].Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransmissionLine_VoltagesAndPowersAreCorrect()
        {
            _powerNet.AddNode(0, 1000, "");
            _powerNet.AddNode(1, 1000, "");
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);
            _powerNet.AddLoad(1, new Complex(-1e5, 0));

            var nodeResults = _powerNet.CalculateNodeVoltages();

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
            _powerNet.AddFeedIn(0, new Complex(230, 0), 0, 0, 0);
            _powerNet.AddImpedanceLoad(1, new Complex(22, 0));
            _powerNet.AddTransmissionLine(0, 1, 1, 0, 0, 0, 1, false);

            var nodeResults = _powerNet.CalculateNodeVoltages();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(230, 0, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(220, 0, nodeResults[1].Voltage, 0.00001);
            ComplexAssert.AreEqual(2300, 0, nodeResults[0].Power, 0.00001);
            ComplexAssert.AreEqual(0, 0, nodeResults[1].Power, 0.00001);
        }
    }
}
