using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using CalculationTest.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationIntegrationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class NewtonRaphsonMethodTest
    {
        private NewtonRaphsonMethod _nodeVoltageCalculator;

        [TestInitialize]
        public void SetUp()
        {
            _nodeVoltageCalculator = new NewtonRaphsonMethod(0.0001, 1000);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithGroundNodeVersionThree(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedAndInverseInformationGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndOnlyVoltagesGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemAndVoltagesAndPowersGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.001, 0.02);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 10);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.02, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithDecoupledPQAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.02, 20);
        }
    }
}
