using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class NodePotentialMethodTest : LoadFlowCalculatorTest
    {
        protected override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new NodePotentialMethod();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionWithSmallResistance();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndVoltagesAndPowersGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodeProblemWithOnePVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithTwoPVBuses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 120);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 60);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithTwoPVBusses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 200);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeSystemWithImaginaryConnectionsAndOnePVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.15, 0.5);
        }
    }
}
