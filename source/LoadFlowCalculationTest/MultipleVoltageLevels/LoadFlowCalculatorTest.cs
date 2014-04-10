using System;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;
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
            _calculator = new LoadFlowCalculator(5, 2, new CurrentIteration(0.00001, 1000));
            _powerNet = new PowerNet(50);
            _powerNetMock = new Mock<IReadOnlyPowerNet>();
        }

        [TestMethod]
        public void Constructor_5AsScaleBaseForVoltage_ScaleBaseForVoltageIs5()
        {
            Assert.AreEqual(5, _calculator.ScaleBasisVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_2AsScaleBaseForPower_ScaleBaseForPowerIs2()
        {
            Assert.AreEqual(2, _calculator.ScaleBasisPower, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForCurrentIsCorrect()
        {
            Assert.AreEqual(_calculator.ScaleBasisPower / _calculator.ScaleBasisVoltage, _calculator.ScaleBasisCurrent, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForImpedanceIsCorrect()
        {
            Assert.AreEqual(_calculator.ScaleBasisVoltage / _calculator.ScaleBasisCurrent, _calculator.ScaleBasisImpedance, 0.00001);
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
            _powerNetMock.Setup(x => x.CheckIfNominalVoltagesDoNotMatch()).Returns(false);

            _calculator.CalculateNodeVoltages(_powerNetMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltages_NominalVoltageMismatch_ThrowsException()
        {
            _powerNetMock.Setup(x => x.CheckIfFloatingNodesExists()).Returns(false);
            _powerNetMock.Setup(x => x.CheckIfNominalVoltagesDoNotMatch()).Returns(true);

            _calculator.CalculateNodeVoltages(_powerNetMock.Object);
        }
    }
}
