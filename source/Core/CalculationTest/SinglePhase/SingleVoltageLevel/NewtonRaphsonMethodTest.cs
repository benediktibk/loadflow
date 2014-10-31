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
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var nodes = CreateTestCollapsingSystem();

            _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var nodes = CreateTestNearlyCollapsingSystem();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionThree();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithSmallResistance();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 0.02);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 20);
        }
    }
}
