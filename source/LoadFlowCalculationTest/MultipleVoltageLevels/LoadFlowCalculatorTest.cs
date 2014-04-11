using System;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        private LoadFlowCalculator _calculator;
        private PowerNet _powerNet;
        private Mock<IReadOnlyPowerNet> _powerNetMock;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new LoadFlowCalculator(2, new CurrentIteration(0.00001, 1000));
            _powerNet = new PowerNet(50);
            _powerNetMock = new Mock<IReadOnlyPowerNet>();
            _powerNetMock.Setup(x => x.CheckIfFloatingNodesExists()).Returns(false);
            _powerNetMock.Setup(x => x.CheckIfNominalVoltagesDoNotMatch()).Returns(false);
            _powerNetMock.Setup(x => x.CheckIfNodeIsOverdetermined()).Returns(false);
        }

        [TestMethod]
        public void Constructor_2AsScaleBaseForPower_ScaleBaseForPowerIs2()
        {
            Assert.AreEqual(2, _calculator.ScaleBasePower, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneFeedInAndOneLoad_CorrectResults()
        {
            _powerNet.AddNode("feedInNode", 100);
            _powerNet.AddNode("loadNode", 100);
            _powerNet.AddFeedIn("feedInNode", "feedIn", new Complex(100, 0));
            _powerNet.AddLoad("loadNode", "load", new Complex(-10, 0));
            _powerNet.AddLine("connection", "feedInNode", "loadNode", 10, 0, 0, 0);

            var nodeVoltages = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey("feedInNode"));
            Assert.IsTrue(nodeVoltages.ContainsKey("loadNode"));
            ComplexAssert.AreEqual(100, 0, nodeVoltages["feedInNode"], 0.0001);
            ComplexAssert.AreEqual(98.989794855663561963945681494118, 0, nodeVoltages["loadNode"], 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void CalculateNodeVoltages_FloatingNode_ThrowsException()
        {
            _powerNetMock.Setup(x => x.CheckIfFloatingNodesExists()).Returns(true);

            _calculator.CalculateNodeVoltages(_powerNetMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltages_NominalVoltageMismatch_ThrowsException()
        {
            _powerNetMock.Setup(x => x.CheckIfNominalVoltagesDoNotMatch()).Returns(true);

            _calculator.CalculateNodeVoltages(_powerNetMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltages_OneNodeIsOverDetermined_ThrowsException()
        {
            _powerNetMock.Setup(x => x.CheckIfNodeIsOverdetermined()).Returns(true);

            _calculator.CalculateNodeVoltages(_powerNetMock.Object);
        }

        [TestMethod]
        public void CalculateNodeVoltages_GeneratorAndLoad_CorrectResults()
        {
            _powerNet.AddNode("feedInNode", 1);
            _powerNet.AddNode("loadNode", 1);
            _powerNet.AddFeedIn("feedInNode", "feedIn", new Complex(1.05, 0));
            _powerNet.AddLoad("loadNode", "load", new Complex(-0.6, -1));
            _powerNet.AddGenerator("loadNode", "generator", 1.02, -0.4);
            _powerNet.AddLine("connection", "feedInNode", "loadNode", 0, 0.00006366197723675813, 0, 0);

            var nodeVoltages = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey("feedInNode"));
            Assert.IsTrue(nodeVoltages.ContainsKey("loadNode"));
            ComplexAssert.AreEqual(1.05, 0, nodeVoltages["feedInNode"], 0.001);
            ComplexAssert.AreEqual(1.0198, 0.019, nodeVoltages["loadNode"], 0.001);
        }
    }
}
