using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class NewtonRaphsonMethodTest : JacobiMatrixBaseMethodTest
    {
        protected override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new NewtonRaphsonMethod(0.0001, 1000);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNodeVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var powerNet = CreateTestCollapsingSystem();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var powerNet = CreateTestNearlyCollapsingSystem();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNodeVersionThree();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
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
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 0.02);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 20);
        }
    }
}
