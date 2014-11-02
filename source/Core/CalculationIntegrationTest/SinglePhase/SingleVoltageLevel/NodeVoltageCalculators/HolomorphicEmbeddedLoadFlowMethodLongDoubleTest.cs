using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using CalculationTest.SinglePhase.SingleVoltageLevel;
using CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationIntegrationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodLongDoubleTest
    {
        private HolomorphicEmbeddedLoadFlowMethod _nodeVoltageCalculator;

        [TestInitialize]
        public void SetUp()
        {
            _nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00001, 50, new PrecisionLongDouble());
        }

        [TestCleanup]
        public void TearDown()
        {
            _nodeVoltageCalculator.Dispose();
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithGroundNodeVersionThree(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 2);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.03, 5);
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
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(_nodeVoltageCalculator);
            var firstCoefficientShouldBe = new DenseVector(new[] { new Complex(1.05, 0) });
            var secondCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0289649928938644, -0.019047619047619) });
            var thirdCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0011445548616427, 1.14628982339312E-100) });
            var firstInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.952380952380952, 0) });
            var secondInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.0262721023980629, 0.0172767519706295) });
            var thirdInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.00144946906527004, 0.00095318285344446) });
            var firstCoefficient = _nodeVoltageCalculator.GetCoefficients(0);
            var secondCoefficient = _nodeVoltageCalculator.GetCoefficients(1);
            var thirdCoefficient = _nodeVoltageCalculator.GetCoefficients(2);
            var firstInverseCoefficient = _nodeVoltageCalculator.GetInverseCoefficients(0);
            var secondInverseCoefficient = _nodeVoltageCalculator.GetInverseCoefficients(1);
            var thirdInverseCoefficient = _nodeVoltageCalculator.GetInverseCoefficients(2);
            ComplexAssert.AreAllEqual(firstCoefficientShouldBe, firstCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(firstInverseCoefficientShouldBe, firstInverseCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(secondCoefficientShouldBe, secondCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(secondInverseCoefficientShouldBe, secondInverseCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(thirdCoefficientShouldBe, thirdCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(thirdInverseCoefficientShouldBe, thirdInverseCoefficient, 0.0001);
            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectCoefficients()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo(_nodeVoltageCalculator);

            powerNetTestCase.CalculateNodeResults();

            Complex a;
            Complex b;
            Complex c;
            HolomorphicEmbeddedLoadFlowMethodTest.CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out a, out b, out c);
            var firstCoefficient = _nodeVoltageCalculator.GetCoefficients(0)[0];
            var secondCoefficient = _nodeVoltageCalculator.GetCoefficients(1)[0];
            var thirdCoefficient = _nodeVoltageCalculator.GetCoefficients(2)[0];
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 20);
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
