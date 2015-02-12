using System;
using System.Linq;
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
            _powerNet.AddNode(0, 123, "");

            _powerNet.GetNodeById(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddNode_TwiceTheSameName_ThrowsException()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(0, 43, "");
        }

        [TestMethod]
        public void GetNodeByName_AlreadyAdded_AddedNode()
        {
            _powerNet.AddNode(3, 123, "");

            var node = _powerNet.GetNodeById(3);

            Assert.AreEqual(3, node.Id);
            Assert.AreEqual(123, node.NominalVoltage, 0.0001);
        }

        [TestMethod]
        public void AddLine_TwoValidNodes_BothNodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddLoad(0, new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.LoadCount);
        }

        [TestMethod]
        public void AddImpedanceLoad_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddImpedanceLoad(0, new Complex(12, 34));

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.ImpedanceLoadCount);
        }

        [TestMethod]
        public void AddFeedIn_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddFeedIn(0, new Complex(123, 3), new Complex());

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.FeedInCount);
        }

        [TestMethod]
        public void AddGenerator_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddGenerator(0, 5, 3);
            
            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.GeneratorCount);
        }

        [TestMethod]
        public void AddThreeWindingTransformer_ValidNode_NodesHaveOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 123, "");
            _powerNet.AddNode(2, 123, "");
            var nodeOne = _powerNet.GetNodeById(0);
            var nodeTwo = _powerNet.GetNodeById(1);
            var nodeThree = _powerNet.GetNodeById(2);

            _powerNet.AddThreeWindingTransformer(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, new Angle(), new Angle(),
                new Angle(), "");

            Assert.AreEqual(1, nodeOne.ConnectedElements.Count);
            Assert.AreEqual(1, nodeTwo.ConnectedElements.Count);
            Assert.AreEqual(1, nodeThree.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.ThreeWindingTransformerCount);
        }

        [TestMethod]
        public void AddCurrentSource_ValidNode_NodeHasOneConnectedElement()
        {
            _powerNet.AddNode(0, 123, "");
            var node = _powerNet.GetNodeById(0);

            _powerNet.AddCurrentSource(0, new Complex(12, 23), new Complex(45, 63));

            Assert.AreEqual(1, node.ConnectedElements.Count);
            Assert.AreEqual(1, _powerNet.CurrentSourceCount);
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithDifferentNominalVoltagesConnectedThroughLine_True()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsTrue(_powerNet.NominalVoltagesDoNotMatch);
        }

        [TestMethod]
        public void CheckIfNominalVoltagesDoNotMatch_TwoNodesWithSameNominalVoltagesConnectedThroughLine_False()
        {
            _powerNet.AddNode(0, 120, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsFalse(_powerNet.NominalVoltagesDoNotMatch);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddNode_NegativeId_ThrowsException()
        {
            _powerNet.AddNode(-1, 230, "");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SlackPhaseShift_NoFeedIn_ThrowsException()
        {
            var result = _powerNet.SlackPhaseShift;
        }

        [TestMethod]
        public void SlackPhaseShift_OneFeedInWithPhaseShift2_2()
        {
            _powerNet.AddNode(0, 3, "");
            _powerNet.AddFeedIn(0, Complex.FromPolarCoordinates(5, 2), new Complex(3, 4));
            var result = _powerNet.SlackPhaseShift;

            Assert.AreEqual(2, result.Radiant, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NominalPhaseShiftPerNode_NoFeedIn_ThrowsException()
        {
            var result = _powerNet.NominalPhaseShiftPerNode;
        }
    }
}
