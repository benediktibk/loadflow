using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class NodeGraphTest
    {
        private PowerNet _powerNet;
        private IReadOnlyNodeGraph _nodeGraph;

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new PowerNet(50);
            _nodeGraph = _powerNet.NodeGraph;
        }

        [TestMethod]
        public void FloatingNodesExist_ThreeInACircleConnectedNodesAndOneFloating_True()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            Assert.IsTrue(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoNotConnectedNodes_True()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            Assert.IsTrue(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            Assert.IsFalse(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoConnectedNodesWithGround_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsFalse(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_ThreeInACircleConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            Assert.IsFalse(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_NoNodes_True()
        {
            Assert.IsTrue(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void Segments_NoNodes_EmptyList()
        {
            var sets = _nodeGraph.Segments;

            Assert.AreEqual(0, sets.Count);
        }

        [TestMethod]
        public void Segments_OneNode_CorrectResult()
        {
            _powerNet.AddNode(0, 120, 0, "");

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(1, sets.Count);
            var firstSet = sets.First();
            Assert.AreEqual(1, firstSet.Count);
            var node = firstSet.First();
            Assert.AreEqual(0, node.Id);
            Assert.AreEqual(120, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void Segments_TwoNotConnectedNodes_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void Segments_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void Segments_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void Segments_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void Segments_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_NoNodes_EmptyList()
        {
            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(0, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_OneNode_CorrectResult()
        {
            _powerNet.AddNode(0, 120, 0, "");

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(1, sets.Count);
            var firstSet = sets.First();
            Assert.AreEqual(1, firstSet.Count);
            var node = firstSet.First();
            Assert.AreEqual(0, node.Id);
            Assert.AreEqual(120, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_TwoNotConnectedNodes_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodes_ThreeElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), 0, 0, 0);
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(3, sets.Count);
        }
    }
}
