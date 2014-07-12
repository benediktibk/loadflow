using System;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetTest
    {
        private PowerNet _powerNet;
        private LoadFlowCalculator _calculatorWithPowerScaling;
        private LoadFlowCalculator _calculatorWithoutPowerScaling;

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNet(50);
            _calculatorWithPowerScaling = new LoadFlowCalculator(1000, new CurrentIteration(0.000001, 1000));
            _calculatorWithoutPowerScaling = new LoadFlowCalculator(1, new CurrentIteration(0.000001, 1000));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetNodeByName_NoNodes_ThrowsException()
        {
            _powerNet.GetNodeByName("blub");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetNodeByName_DifferentNodeAdded_ThrowsException()
        {
            _powerNet.AddNode("differentOne", 123);

            _powerNet.GetNodeByName("blub");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddNode_TwiceTheSameName_ThrowsException()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blub", 43);
        }

        [TestMethod]
        public void GetNodeByName_AlreadyAdded_AddedNode()
        {
            _powerNet.AddNode("blub", 123);

            var node = _powerNet.GetNodeByName("blub");

            Assert.AreEqual("blub", node.Name);
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
            _powerNet.AddNode("blob", 120);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var firstSet = sets.First();
            Assert.AreEqual(1, firstSet.Count);
            var node = firstSet.First();
            Assert.AreEqual("blob", node.Name);
            Assert.AreEqual(120, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_TwoNotConnectedNodes_TwoElements()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddNode("heinz", 120);
            _powerNet.AddLine("line1", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line2", "blob", "heinz", 3, 5, 5, 2);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddNode("heinz", 120);
            _powerNet.AddLine("line1", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line2", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line3", "blub", "heinz", 3, 5, 5, 2);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddNode("heinz", 120);
            _powerNet.AddNode("hanz", 120);
            _powerNet.AddLine("line1", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line2", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line3", "blub", "heinz", 3, 5, 5, 2);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodesAndOneFloating_True()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddNode("heinz", 120);
            _powerNet.AddNode("hanz", 120);
            _powerNet.AddLine("line1", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line2", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line3", "blub", "heinz", 3, 5, 5, 2);

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoNotConnectedNodes_True()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoConnectedNodes_False()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodes_False()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddNode("heinz", 120);
            _powerNet.AddLine("line1", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line2", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line3", "blub", "heinz", 3, 5, 5, 2);

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
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            var nodeOne = _powerNet.GetNodeByName("blub");
            var nodeTwo = _powerNet.GetNodeByName("blob");

            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LineCount);
        }

        [TestMethod]
        public void AddTransformer_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            var nodeOne = _powerNet.GetNodeByName("blub");
            var nodeTwo = _powerNet.GetNodeByName("blob");

            _powerNet.AddTransformer("blub", "blob", "line", new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 10);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.TransformerCount);
        }

        [TestMethod]
        public void AddLoad_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode("blub", 123);
            var node = _powerNet.GetNodeByName("blub");

            _powerNet.AddLoad("blub", "load", new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LoadCount);
        }

        [TestMethod]
        public void AddFeedIn_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode("blub", 123);
            var node = _powerNet.GetNodeByName("blub");

            _powerNet.AddFeedIn("blub", "load", new Complex(123, 3), 0);

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.FeedInCount);
        }

        [TestMethod]
        public void AddGenerator_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode("blub", 123);
            var node = _powerNet.GetNodeByName("blub");

            _powerNet.AddGenerator("blub", "load", 5, 3);
            
            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.GeneratorCount);
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithDifferentNominalVoltagesConnectedThroughLine_True()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            Assert.IsTrue(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithSameNominalVoltagesConnectedThroughLine_False()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            Assert.IsFalse(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfOneNodeHasTwoFeedIns_TwoNodesWithSameNominalVoltagesConnectedThroughLine_False()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);

            Assert.IsFalse(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddLoad_SameNameAsNode_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddLoad("blub", "blub", new Complex());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddFeedIn_SameNameAsNode_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddFeedIn("blub", "blub", new Complex(), 6);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddGenerator_SameNameAsNode_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddGenerator("blub", "blub", 3, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddLine_SameNameAsNode_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddLine("blub", "blub", "blob", 3, 5, 5, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddTransformer_SameNameAsNode_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddTransformer("blub", "blob", "blub", new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddTransformer_NameContainsHashKey_ThrowsException()
        {
            _powerNet.AddNode("blub", 120);
            _powerNet.AddNode("blob", 120);
            _powerNet.AddTransformer("blub", "blob", "heinz#hanz", new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 10);
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_NoElementNeedsGroundNode_False()
        {
            _powerNet.AddNode("sourceNode", 120);
            _powerNet.AddNode("targetNode", 120);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(), 6);
            _powerNet.AddLine("line", "sourceNode", "targetNode", 3, 5, 0, 0);
            _powerNet.AddLoad("targetNode", "load", new Complex(4, 5));

            Assert.IsFalse(_powerNet.CheckIfGroundNodeIsNecessary());
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_OneElementNeedsGroundNode_True()
        {
            _powerNet.AddNode("sourceNode", 120);
            _powerNet.AddNode("targetNode", 120);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(), 6);
            _powerNet.AddLine("line", "sourceNode", "targetNode", 3, 5, 5, 0);
            _powerNet.AddLoad("targetNode", "load", new Complex(4, 5));

            Assert.IsTrue(_powerNet.CheckIfGroundNodeIsNecessary());
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoad_VoltagesAreCorrect()
        {
            _powerNet.AddNode("sourceNode", 1000);
            _powerNet.AddNode("targetNode", 1000);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1000, 0), 0);
            _powerNet.AddLine("line", "sourceNode", "targetNode", 5, 700e-6, 0, 0);
            _powerNet.AddLoad("targetNode", "load", new Complex(-2693.9, -4118.5));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculatorWithPowerScaling);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeByName("sourceNode");
            var targetNode = _powerNet.GetNodeByName("targetNode");
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, targetNode.Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_SimpleConnectionFromFeedInToLoadWithNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode("sourceNode", 1000);
            _powerNet.AddNode("targetNode", 1000);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1000, 0), 0);
            _powerNet.AddLine("line", "sourceNode", "targetNode", 5, 700e-6, 0, 0);
            _powerNet.AddLoad("targetNode", "load", new Complex(-2693.9, -4118.5));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculatorWithoutPowerScaling);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeByName("sourceNode");
            var targetNode = _powerNet.GetNodeByName("targetNode");
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.01);
            ComplexAssert.AreEqual(985, 20, targetNode.Voltage, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndNoMainImpedance_VoltagesAreCorrect()
        {
            _powerNet.AddNode("sourceNode", 1000);
            _powerNet.AddNode("targetNode", 400);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1000, 0), 0);
            _powerNet.AddTransformer("sourceNode", "targetNode", "transformer", new Complex(2.46875, 0), new Complex(0.592499, 0), new Complex(), 2.5);
            _powerNet.AddLoad("targetNode", "load", new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculatorWithPowerScaling);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeByName("sourceNode");
            var targetNode = _powerNet.GetNodeByName("targetNode");
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(395, 0, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNominalRatioAndNoMainImpedanceAndNoPowerScaling_VoltagesAreCorrect()
        {
            _powerNet.AddNode("sourceNode", 1000);
            _powerNet.AddNode("targetNode", 400);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1000, 0), 0);
            _powerNet.AddTransformer("sourceNode", "targetNode", "transformer", new Complex(2.46875, 0), new Complex(0.592499, 0), new Complex(), 2.5);
            _powerNet.AddLoad("targetNode", "load", new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculatorWithoutPowerScaling);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeByName("sourceNode");
            var targetNode = _powerNet.GetNodeByName("targetNode");
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(395, 0, targetNode.Voltage, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneTransformerWithNotNominalRatioAndNoMainImpedance_VoltagesAreCorrect()
        {
            _powerNet.AddNode("sourceNode", 1000);
            _powerNet.AddNode("targetNode", 400);
            _powerNet.AddFeedIn("sourceNode", "feedIn", new Complex(1000, 0), 0);
            _powerNet.AddTransformer("sourceNode", "targetNode", "transformer", new Complex(1.50407, 0), new Complex(0.5985, 0), new Complex(), 2.48);
            _powerNet.AddLoad("targetNode", "load", new Complex(-2000, 0));

            var voltageCollapse = _powerNet.CalculateNodeVoltages(_calculatorWithoutPowerScaling);

            Assert.IsFalse(voltageCollapse);
            var sourceNode = _powerNet.GetNodeByName("sourceNode");
            var targetNode = _powerNet.GetNodeByName("targetNode");
            ComplexAssert.AreEqual(1000, 0, sourceNode.Voltage, 0.0001);
            ComplexAssert.AreEqual(399, 0, targetNode.Voltage, 0.0001);
        }
    }
}
