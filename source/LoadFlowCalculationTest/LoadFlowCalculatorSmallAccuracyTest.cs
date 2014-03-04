using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;
using Complex = System.Numerics.Complex;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorSmallAccuracyTest : LoadFlowCalculatorTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            CreateOneSideSuppliedConnection(0.1, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] {new Node(), new Node()};
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.1, 0.2);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Voltage = _voltages.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Voltage = _voltages.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.01, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Voltage = _voltages.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Voltage = _voltages.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = _powers.At(0);
            nodes[1].Voltage = _voltages.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.3, 400);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Voltage = _voltages.At(2);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.3, 30);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Voltage = _voltages.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.3, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Voltage = _voltages.At(2);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.01, 20);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            CreateFiveNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);
            nodes[2].Voltage = _voltages.At(2);
            nodes[3].Power = _powers.At(3);
            nodes[4].Voltage = _voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.1, 50);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].VoltageMagnitude = _voltages.At(1).Magnitude;
            nodes[1].RealPower = _powers.At(1).Real;

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.01, 0.001);
        }
    }
}
