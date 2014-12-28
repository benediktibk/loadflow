using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using CalculationTest.SinglePhase.SingleVoltageLevel;
using CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationIntegrationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodLongDoubleTest
    {
        private HolomorphicEmbeddedLoadFlowMethod _nodeVoltageCalculator;
        private IList<Vector<Complex>> _coefficients;
        private Mock<INodeVoltageCalculator> _nodeVoltageCalculatorMock;

        [TestInitialize]
        public void SetUp()
        {
            _nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00001, 50, 64);
            _nodeVoltageCalculatorMock = new Mock<INodeVoltageCalculator>();
            _nodeVoltageCalculatorMock.Setup(x =>
                x.CalculateUnknownVoltages(It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                    It.IsAny<double>(), It.IsAny<Vector<Complex>>(),
                    It.IsAny<Vector<Complex>>(), It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>()))
                .Returns((IReadOnlyAdmittanceMatrix a, IList<Complex> b, double c, Vector<Complex> d, Vector<Complex> e, IList<PqNodeWithIndex> f, IList<PvNodeWithIndex> g) =>
                    _nodeVoltageCalculator.CalculateUnknownVoltages(a, b, c, d, e, f, g, out _coefficients, 3));
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError)
                .Returns(_nodeVoltageCalculator.MaximumRelativePowerError);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.025, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithGroundNodeVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 2);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestCollapsingSystem(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestNearlyCollapsingSystem(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedConnectionWithBigResistance(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedConnectionWithSmallResistance(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedAndInverseInformationGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyVoltagesGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndVoltagesAndPowersGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemAndTwoVoltagesGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithGroundNode(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithMostlyImaginaryConnections(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithMostlyImaginaryConnections(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.2);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithTwoPVBuses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.04, 5);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.025, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnection(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeSystemWithImaginaryConnectionsAndOnePVBus_ThrowsNoException()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CoefficientsCorrect()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo(_nodeVoltageCalculatorMock.Object);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(_nodeVoltageCalculator);
            var firstCoefficientShouldBe = new DenseVector(new[] { new Complex(1.05, 0) });
            var secondCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0289649928938644, -0.019047619047619) });
            var thirdCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0011445548616427, 1.14628982339312E-100) });
            var firstCoefficient = _coefficients[0];
            var secondCoefficient = _coefficients[1];
            var thirdCoefficient = _coefficients[2];
            ComplexAssert.AreAllEqual(firstCoefficientShouldBe, firstCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(secondCoefficientShouldBe, secondCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(thirdCoefficientShouldBe, thirdCoefficient, 0.0001);
            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectCoefficients()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo(_nodeVoltageCalculatorMock.Object);

            powerNetTestCase.CalculateNodeResults();

            Complex a;
            Complex b;
            Complex c;
            HolomorphicEmbeddedLoadFlowMethodTest.CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out a, out b, out c);
            var firstCoefficient = _coefficients[0][0];
            var secondCoefficient = _coefficients[1][0];
            var thirdCoefficient = _coefficients[2][0];
            ComplexAssert.AreEqual(a, firstCoefficient, 0.00001);
            ComplexAssert.AreEqual(b, secondCoefficient, 0.00001);
            ComplexAssert.AreEqual(c, thirdCoefficient, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithDecoupledPQAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 0.2);
        }
    }
}
