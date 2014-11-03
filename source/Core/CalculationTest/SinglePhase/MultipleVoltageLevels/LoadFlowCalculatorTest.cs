using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Misc;
using LoadFlowCalculator = Calculation.SinglePhase.MultipleVoltageLevels.LoadFlowCalculator;
using PowerNet = Calculation.SinglePhase.MultipleVoltageLevels.PowerNet;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        private LoadFlowCalculator _calculator;
        private LoadFlowCalculator _calculatorWithNoPowerScaling;
        private PowerNet _powerNet;
        private Mock<IReadOnlyPowerNet> _powerNetMock;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new LoadFlowCalculator(2, new CurrentIteration(0.00001, 1000));
            _calculatorWithNoPowerScaling = new LoadFlowCalculator(1, new CurrentIteration(0.00001, 1000));
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
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneFeedInAndOneLoadWithoutPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 100, 0, "");
            _powerNet.AddNode(1, 100, 0, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddTransmissionLine(0, 1, 10, 0, 0, 0, 1, true);

            var nodeResults = _calculatorWithNoPowerScaling.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 0.000001);
            ComplexAssert.AreEqual(0.98989794855663561963945681494118, 0, nodeResults[1].Voltage, 0.000001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_OneFeedInAndOneLoadWithPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 100, 0, "");
            _powerNet.AddNode(1, 100, 0, "");
            _powerNet.AddFeedIn(0, new Complex(100, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-10, 0));
            _powerNet.AddTransmissionLine(0, 1, 10, 0, 0, 0, 1, true);

            var nodeResults = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 0.000001);
            ComplexAssert.AreEqual(0.98989794855663561963945681494118, 0, nodeResults[1].Voltage, 0.000001);
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
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_GeneratorAndLoadWithNoPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);

            var nodeResults = _calculatorWithNoPowerScaling.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(1.05, 0, nodeResults[0].Voltage, 0.001);
            ComplexAssert.AreEqual(1.0198, -0.019, nodeResults[1].Voltage, 0.001);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CalculateNodeVoltages_GeneratorAndLoadWithPowerScaling_CorrectResults()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);

            var nodeResults = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeResults.Count);
            Assert.IsTrue(nodeResults.ContainsKey(0));
            Assert.IsTrue(nodeResults.ContainsKey(1));
            ComplexAssert.AreEqual(1.05, 0, nodeResults[0].Voltage, 0.001);
            ComplexAssert.AreEqual(1.0198, -0.019, nodeResults[1].Voltage, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CalculationFails_Null()
        {
            _powerNet.AddNode(0, 1, 0, "");
            _powerNet.AddNode(1, 1, 0, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), 0, 1.1, 1);
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);
            var nodeVoltageCalculator = new Mock<INodeVoltageCalculator>();
            nodeVoltageCalculator.Setup(
                c =>
                    c.CalculateUnknownVoltages(
                        It.IsAny<Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix>(),
                        It.IsAny<IList<Complex>>(), It.IsAny<double>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<Vector<Complex>>(), It.IsAny<IList<PqBus>>(), It.IsAny<IList<PvBus>>()))
                .Returns(DenseVector.Create(2, i => new Complex(0, 0)));
            var calculator = new LoadFlowCalculator(1, nodeVoltageCalculator.Object);

            var nodeResults = calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(null, nodeResults);
        }
    }
}
