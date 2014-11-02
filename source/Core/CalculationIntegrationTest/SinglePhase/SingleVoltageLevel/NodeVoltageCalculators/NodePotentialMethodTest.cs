using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using CalculationTest.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationIntegrationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class NodePotentialMethodTest
    {
        private NodePotentialMethod _nodeVoltageCalculator;

        [TestInitialize]
        public void SetUp()
        {
            _nodeVoltageCalculator = new NodePotentialMethod();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFromOneSideSuppliedConnectionWithBigResistance(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 1);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemAndTwoVoltagesGiven(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.02, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithGroundNode(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithMostlyImaginaryConnections(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithMostlyImaginaryConnections(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodeProblemWithOnePVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.2, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithTwoPVBuses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 120);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 60);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestFiveNodeProblemWithTwoPVBusses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.2, 200);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnection(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 0.05);
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
        public void CalculateNodeVoltagesAndPowers_ThreeNodeSystemWithImaginaryConnectionsAndOnePVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.01, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.05, 10);
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
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.1, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus(_nodeVoltageCalculator);

            var nodeResults = powerNetTestCase.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, powerNetTestCase.CorrectVoltages, powerNetTestCase.CorrectPowers, 0.15, 0.5);
        }
    }
}
