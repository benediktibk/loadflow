using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Complex = System.Numerics.Complex;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorHighAccuracyTest : LoadFlowCalculatorSmallAccuracyTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Voltage = _voltages.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Power = _powers.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Power = _powers.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }
        
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Voltage = _voltages.At(1);
            nodes[2].Voltage = _voltages.At(2);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Power = _powers.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            CreateCollapsingOneSideSuppliedConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);

            _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }
    }
}
