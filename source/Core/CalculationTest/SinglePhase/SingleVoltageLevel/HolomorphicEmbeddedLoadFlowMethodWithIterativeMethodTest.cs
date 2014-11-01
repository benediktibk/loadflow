using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodWithIterativeMethodTest : LoadFlowCalculatorTest
    {
        protected override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            const double targetPrecision = 0.000001;
            return new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision, new CurrentIteration(targetPrecision, 1000));
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.05);
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
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
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodeProblemWithOnePVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            var nodeResults = powerNet.CalculateMissingInformation();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
    }
}
