﻿using System;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetTest
    {
        private PowerNet _powerNet;
        private INodeVoltageCalculator _calculator;

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNet(50);
            _calculator = new CurrentIteration(0.00000001, 1000);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetNodeByName_NoNodes_ThrowsException()
        {
            _powerNet.GetNodeById(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetNodeByName_DifferentNodeAdded_ThrowsException()
        {
            _powerNet.AddNode(0, 123);

            _powerNet.GetNodeById(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNode_TwiceTheSameName_ThrowsException()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(0, 43);
        }

        [TestMethod]
        public void GetNodeByName_AlreadyAdded_AddedNode()
        {
            _powerNet.AddNode(3, 123);

            var node = _powerNet.GetNodeById(3);

            Assert.AreEqual(3, node.Id);
            Assert.AreEqual(123, node.NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_NoNodes_EmptyList()
        {
            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(0, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_OneNode_CorrectResult()
        {
            _powerNet.AddNode(0, 120);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var firstSet = sets.First();
            Assert.AreEqual(1, firstSet.Count);
            var node = firstSet.First();
            Assert.AreEqual(0, node.Id);
            Assert.AreEqual(120, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_TwoNotConnectedNodes_TwoElements()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddNode(2, 120);
            _powerNet.AddLine(1, 2, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddNode(2, 120);
            _powerNet.AddLine(1, 2, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 2, 3, 5, 0, 0, 1);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddNode(2, 120);
            _powerNet.AddNode(3, 120);
            _powerNet.AddLine(1, 2, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 2, 3, 5, 0, 0, 1);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodesAndOneFloating_True()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddNode(2, 120);
            _powerNet.AddNode(3, 120);
            _powerNet.AddLine(1, 2, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 2, 3, 5, 0, 0, 1);

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoNotConnectedNodes_True()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoConnectedNodesWithGround_False()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddLine(0, 1, 3, 5, 5, 2, 1);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddNode(2, 120);
            _powerNet.AddLine(1, 2, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);
            _powerNet.AddLine(0, 2, 3, 5, 0, 0, 1);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_NoNodes_True()
        {
            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void AddLine_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            var nodeOne = _powerNet.GetNodeById(0);
            var nodeTwo = _powerNet.GetNodeById(1);

            _powerNet.AddLine(0, 1, 3, 5, 5, 2, 1);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LineCount);
        }

        [TestMethod]
        public void AddTransformer_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            var nodeOne = _powerNet.GetNodeById(0);
            var nodeTwo = _powerNet.GetNodeById(1);

            _powerNet.AddTransformer(0, 1, 50, 0.2, 4, 5, 0.1, 2);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.TransformerCount);
        }

        [TestMethod]
        public void AddLoad_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123);
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddLoad(0, new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LoadCount);
        }

        [TestMethod]
        public void AddFeedIn_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123);
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddFeedIn(0, new Complex(123, 3), 0);

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.FeedInCount);
        }

        [TestMethod]
        public void AddGenerator_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123);
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddGenerator(0, 5, 3);
            
            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.GeneratorCount);
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithDifferentNominalVoltagesConnectedThroughLine_True()
        {
            _powerNet.AddNode(0, 123);
            _powerNet.AddNode(1, 120);
            _powerNet.AddLine(0, 1, 3, 5, 5, 2, 1);

            Assert.IsTrue(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithSameNominalVoltagesConnectedThroughLine_False()
        {
            _powerNet.AddNode(0, 120);
            _powerNet.AddNode(1, 120);
            _powerNet.AddLine(0, 1, 3, 5, 5, 2, 1);

            Assert.IsFalse(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_NoElementNeedsGroundNode_False()
        {
            _powerNet.AddNode(0, 120);
            _powerNet.AddNode(1, 120);
            _powerNet.AddFeedIn(0, new Complex(), 6);
            _powerNet.AddLine(0, 1, 3, 5, 0, 0, 1);
            _powerNet.AddLoad(1, new Complex(4, 5));

            Assert.IsFalse(_powerNet.IsGroundNodeNecessary());
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_OneElementNeedsGroundNode_True()
        {
            _powerNet.AddNode(0, 120);
            _powerNet.AddNode(1, 120);
            _powerNet.AddFeedIn(0, new Complex(), 6);
            _powerNet.AddLine(0, 1, 3, 5, 5, 0, 1);
            _powerNet.AddLoad(1, new Complex(4, 5));

            Assert.IsTrue(_powerNet.IsGroundNodeNecessary());
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 1000);
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0);
            _powerNet.AddLine(0, 1, 5, 700e-6, 0, 0, 1);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, targetNode.Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoadWithNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 1000);
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0);
            _powerNet.AddLine(0, 1, 5, 700e-6, 0, 0, 1);
            _powerNet.AddLoad(1, new Complex(-2693.9, -4118.5));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, targetNode.Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatio_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 400);
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0);
            _powerNet.AddTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5);
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(397.055749878654, -4.5771515495886, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 400);
            _powerNet.AddFeedIn(0, new Complex(1000, 0), 0);
            _powerNet.AddTransformer(0, 1, 3000, 0.02, 30, 50, 0.05, 2.5);
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(397.055749878654, -4.5771515495886, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNotNominalRatioAndNoLoadSimplifiedAndNodePotentialMethod_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 2);
            _powerNet.AddNode(1, 1);
            _powerNet.AddFeedIn(0, new Complex(1.95, 0), 0);
            _powerNet.AddTransformer(0, 1, 4000, 0.02, 30, 50, 0.05, 1.95);
            _powerNet.AddLoad(1, new Complex(0, 0));
            var calculator = new NodePotentialMethod();

            var voltageCollapse = _powerNet.CalculateNodeVoltages(calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1.95, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(0.999489867835125, 0, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNotNominalRatioAndNoLoadSimplified_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 2);
            _powerNet.AddNode(1, 1);
            _powerNet.AddFeedIn(0, new Complex(1.95, 0), 0);
            _powerNet.AddTransformer(0, 1, 4000, 0.02, 30, 50, 0.05, 1.95);
            _powerNet.AddLoad(1, new Complex(0, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1.95, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(0.999489867835125, 0, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNotNominalRatioAndNoLoadSimplifiedWithNodePotentialMethod_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 2);
            _powerNet.AddNode(1, 1);
            _powerNet.AddFeedIn(0, new Complex(1.95, 0), 0);
            _powerNet.AddTransformer(0, 1, 10, 0.02, 0.00000001, 0.00000000001, 0.05, 1.95);
            _powerNet.AddLoad(1, new Complex(0, 0));
            var calculator = new NodePotentialMethod();

            var voltageCollapse = _powerNet.CalculateNodeVoltages(calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1.95, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(0.999500249875063, 0, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioSecondVersion_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 400);
            _powerNet.AddFeedIn(0, new Complex(1002, 0), 0);
            _powerNet.AddTransformer(0, 1, 4000, 0.05, 100, 50, 0.05, 2.5);
            _powerNet.AddLoad(1, new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1002, 0, sourceNode.Voltage, 0.01);
            ComplexAssert.AreEqual(395.0648210267077, -8.5049094628884152, targetNode.Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNotNominalRatioAndNoLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode(0, 1000);
            _powerNet.AddNode(1, 400);
            _powerNet.AddFeedIn(0, new Complex(1002, 0), 0);
            _powerNet.AddTransformer(0, 1, 4000, 0.02, 30, 50, 0.05, 2.495);
            _powerNet.AddLoad(1, new Complex(0, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculator);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeById(0);
            var targetNode = _powerNet.GetNodeById(1);
            ComplexAssert.AreEqual(1002, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(401.398335699717, 0.0287342453164691, targetNode.Voltage, 0.0001);
        }
    }
}