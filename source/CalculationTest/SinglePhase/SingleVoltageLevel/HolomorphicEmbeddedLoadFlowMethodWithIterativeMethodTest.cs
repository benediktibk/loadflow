using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

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
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var nodes = CreateTestCollapsingSystem();

            _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var nodes = CreateTestNearlyCollapsingSystem();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionThree();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithSmallResistance();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNode();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodeProblemWithOnePVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnection();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }
    }
}
