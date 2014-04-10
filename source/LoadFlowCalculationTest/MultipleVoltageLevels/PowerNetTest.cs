using System;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetTest
    {
        private PowerNet _powerNet;

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNet(50);
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
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blob", "heinz", 3, 5, 5, 2);

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
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blub", "heinz", 3, 5, 5, 2);

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
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blub", "heinz", 3, 5, 5, 2);

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
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blub", "heinz", 3, 5, 5, 2);

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
            _powerNet.AddLine("line", "blub", "blob", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blob", "heinz", 3, 5, 5, 2);
            _powerNet.AddLine("line", "blub", "heinz", 3, 5, 5, 2);

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
        public void AddTransformator_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode("blub", 123);
            _powerNet.AddNode("blob", 120);
            var nodeOne = _powerNet.GetNodeByName("blub");
            var nodeTwo = _powerNet.GetNodeByName("blob");

            _powerNet.AddTransformator("blub", "blob", "line", 3, 5, 5, 2, 3);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.TransformatorCount);
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

            _powerNet.AddFeedIn("blub", "load", new Complex(123, 3));

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
    }
}
