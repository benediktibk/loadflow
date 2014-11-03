using System;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetTest
    {
        private PowerNet _powerNet;
        private Mock<INodeGraph> _nodeGraphMock;

        [TestInitialize]
        public void SetUp()
        {
            _nodeGraphMock = new Mock<INodeGraph>();
            _powerNet = new PowerNet(50, _nodeGraphMock.Object);
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
            _powerNet.AddNode(0, 123, 0, "");

            _powerNet.GetNodeById(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNode_TwiceTheSameName_ThrowsException()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(0, 43, 0, "");
        }

        [TestMethod]
        public void GetNodeByName_AlreadyAdded_AddedNode()
        {
            _powerNet.AddNode(3, 123, 0, "");

            var node = _powerNet.GetNodeById(3);

            Assert.AreEqual(3, node.Id);
            Assert.AreEqual(123, node.NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void AddLine_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            var nodeOne = _powerNet.GetNodeById(0);
            var nodeTwo = _powerNet.GetNodeById(1);

            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LineCount);
        }

        [TestMethod]
        public void AddTransformer_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            var nodeOne = _powerNet.GetNodeById(0);
            var nodeTwo = _powerNet.GetNodeById(1);

            _powerNet.AddTwoWindingTransformer(0, 1, 50, 0.2, 4, 5, 0.1, 2, new Angle(), "");

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.TwoWindingTransformerCount);
        }

        [TestMethod]
        public void AddLoad_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddLoad(0, new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LoadCount);
        }

        [TestMethod]
        public void AddImpedanceLoad_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddImpedanceLoad(0, new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.ImpedanceLoadCount);
        }

        [TestMethod]
        public void AddFeedIn_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddFeedIn(0, new Complex(123, 3), 0, 1.1, 1);

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.FeedInCount);
        }

        [TestMethod]
        public void AddGenerator_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, 0, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddGenerator(0, 5, 3);
            
            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.GeneratorCount);
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithDifferentNominalVoltagesConnectedThroughLine_True()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsTrue(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithSameNominalVoltagesConnectedThroughLine_False()
        {
            _powerNet.AddNode(0, 120, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsFalse(_powerNet.CheckIfNominalVoltagesDoNotMatch());
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_NoElementNeedsGroundNode_False()
        {
            _powerNet.AddNode(0, 120, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddFeedIn(0, new Complex(), 6, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(4, 5));

            Assert.IsFalse(_powerNet.IsGroundNodeNecessary());
        }

        [TestMethod]
        public void CheckIfGroundNodeIsNecessary_OneElementNeedsGroundNode_True()
        {
            _powerNet.AddNode(0, 120, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddFeedIn(0, new Complex(), 6, 1.1, 1);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 0, 1, true);
            _powerNet.AddLoad(1, new Complex(4, 5));

            Assert.IsTrue(_powerNet.IsGroundNodeNecessary());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddNode_NegativeId_ThrowsException()
        {
            _powerNet.AddNode(-1, 230, 0, "");
        }
    }
}
