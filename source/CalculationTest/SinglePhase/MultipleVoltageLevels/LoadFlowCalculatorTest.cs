using System;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        private LoadFlowCalculator _calculator;
        private LoadFlowCalculator _calculatorWithNoPowerScaling;
        private LoadFlowCalculator _calculatorWithDummyMethod;
        private PowerNet _powerNet;
        private Mock<IReadOnlyPowerNet> _powerNetMock;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new LoadFlowCalculator(2, new CurrentIteration(0.00001, 1000));
            _calculatorWithNoPowerScaling = new LoadFlowCalculator(1, new CurrentIteration(0.00001, 1000));
            _calculatorWithDummyMethod = new LoadFlowCalculator(1, new DummyMethod());
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
        public void CalculateNodeVoltages_OneFeedInAndOneLoadWithoutPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 100, 0, "");
            _powerNet.AddNode(1, 100, 0, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddLine(0, 1, 10, 0, 0, 0, 1);

            var nodeVoltages = _calculatorWithNoPowerScaling.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey(0));
            Assert.IsTrue(nodeVoltages.ContainsKey(1));
            ComplexAssert.AreEqual(1, 0, nodeVoltages[0], 0.000001);
            ComplexAssert.AreEqual(0.98989794855663561963945681494118, 0, nodeVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneFeedInAndOneLoadWithPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 100, 0, "");
            _powerNet.AddNode(1, 100, 0, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddLine(0, 1, 10, 0, 0, 0, 1);

            var nodeVoltages = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey(0));
            Assert.IsTrue(nodeVoltages.ContainsKey(1));
            ComplexAssert.AreEqual(1, 0, nodeVoltages[0], 0.000001);
            ComplexAssert.AreEqual(0.98989794855663561963945681494118, 0, nodeVoltages[1], 0.000001);
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
        public void CalculateNodeVoltages_GeneratorAndLoadWithNoPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1);

            var nodeVoltages = _calculatorWithNoPowerScaling.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey(0));
            Assert.IsTrue(nodeVoltages.ContainsKey(1));
            ComplexAssert.AreEqual(1.05, 0, nodeVoltages[0], 0.001);
            ComplexAssert.AreEqual(1.0198, -0.019, nodeVoltages[1], 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_GeneratorAndLoadWithPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1);

            var nodeVoltages = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey(0));
            Assert.IsTrue(nodeVoltages.ContainsKey(1));
            ComplexAssert.AreEqual(1.05, 0, nodeVoltages[0], 0.001);
            ComplexAssert.AreEqual(1.0198, -0.019, nodeVoltages[1], 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CalculationFails_Null()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1, "");
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1);

            var nodeVoltages = _calculatorWithDummyMethod.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(null, nodeVoltages);
        }
    }
}
