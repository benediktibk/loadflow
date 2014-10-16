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
            var nodes = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 1);
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
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodeProblemWithOnePVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithTwoPVBuses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 120);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 60);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithTwoPVBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 200);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 0.05);
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
        public void CalculateNodeVoltagesAndPowers_ThreeNodeSystemWithImaginaryConnectionsAndOnePVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.01, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.2, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.05, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.1, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.15, 0.5);
        }
    }
}
