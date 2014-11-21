using System;
using System.IO;
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
        private NodeGraph _nodeGraph;

        [TestInitialize]
        public void SetUp()
        {
            _nodeGraph = new NodeGraph();
            _powerNet = new PowerNet(50, _nodeGraph);
        }

        [TestMethod]
        public void FloatingNodesExist_ThreeInACircleConnectedNodesAndOneFloating_True()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddNode(3, 120, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            Assert.IsTrue(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoNotConnectedNodes_True()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");

            Assert.IsTrue(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            Assert.IsFalse(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_TwoConnectedNodesWithGround_False()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsFalse(_nodeGraph.FloatingNodesExist);
        }

        [TestMethod]
        public void FloatingNodesExist_ThreeInACircleConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
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
            _powerNet.AddNode(0, 120, "");

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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void Segments_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.Segments;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void Segments_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddNode(3, 120, "");
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
            _powerNet.AddNode(0, 120, "");

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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
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
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddNode(3, 120, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodes_ThreeElements()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var sets = _nodeGraph.SegmentsOnSameVoltageLevel;

            Assert.AreEqual(3, sets.Count);
        }

        [TestMethod]
        public void CalculateNominalPhaseShiftPerNode_AFewNodesConnectedByTransformers_PhaseShiftsAreCorrect()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddNode(3, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");
            _powerNet.AddTwoWindingTransformer(2, 3, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.2), "");

            var phaseShifts = _powerNet.NominalPhaseShiftPerNode;

            Assert.IsTrue(Angle.Equal(new Angle(), phaseShifts[_powerNet.GetNodeById(0)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(0.3), phaseShifts[_powerNet.GetNodeById(1)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(-1.5), phaseShifts[_powerNet.GetNodeById(2)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(-1.3), phaseShifts[_powerNet.GetNodeById(3)], 0.000001));
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodesAndeOneTransformerTwiceWithSamePhaseShift_ThreeElements()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var nominalPhaseShifts = _powerNet.NominalPhaseShiftPerNode;

            Assert.AreEqual(3, nominalPhaseShifts.Count);
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodesAndeOneTransformerTwiceWithSamePhaseShiftInverse_ThreeElements()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(2, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var nominalPhaseShifts = _powerNet.NominalPhaseShiftPerNode;

            Assert.AreEqual(3, nominalPhaseShifts.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CalculateNominalPhaseShiftPerNode_ThreeInACircleByTransformersConnectedConnectedNodesAndeOneTransformerTwiceWithDifferentPhaseShift_ThrowsException()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(2), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var result = _powerNet.NominalPhaseShiftPerNode;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void SegmentsOnSameVoltageLevel_TwoNodesByTransformerTwiceWithDifferentPhaseShiftConnected_ThrowsException()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(2), "");

            var result = _powerNet.NominalPhaseShiftPerNode;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void SegmentsOnSameVoltageLevel_TwoNodesByTransformerTwiceWithDifferentPhaseShiftConnectedInverse_ThrowsException()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(1, 0, 3, 0.5, 0.1, 0, 1, 1, new Angle(2), "");

            var result = _powerNet.NominalPhaseShiftPerNode;
        }

        [TestMethod]
        public void SegmentsOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodesAndAThreeWindingTransformer_ThreeElements()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");
            _powerNet.AddThreeWindingTransformer(0, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, new Angle(0.1), new Angle(0.2), new Angle(-1.6), "");

            var nominalPhaseShifts = _powerNet.NominalPhaseShiftPerNode;

            Assert.AreEqual(3, nominalPhaseShifts.Count);
        }

        [TestMethod]
        public void FloatingNodes_NoFloatingNodes_EmptyList()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var floatingNodes = _powerNet.NodeGraph.FloatingNodes;

            Assert.IsFalse(_powerNet.NodeGraph.FloatingNodesExist);
            Assert.AreEqual(0, floatingNodes.Count);
        }

        [TestMethod]
        public void FloatingNodes_TwoSegmentsWithFloatingNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, "");
            _powerNet.AddNode(1, 120, "");
            _powerNet.AddNode(2, 120, "");
            _powerNet.AddNode(3, 120, "");
            _powerNet.AddNode(4, 120, "");
            _powerNet.AddNode(5, 120, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), new Complex());
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");
            _powerNet.AddTransmissionLine(4, 5, 1, 1, 1, 1, 1, false);

            var floatingNodes = _powerNet.NodeGraph.FloatingNodes;

            Assert.IsTrue(_powerNet.NodeGraph.FloatingNodesExist);
            Assert.AreEqual(3, floatingNodes.Count);
            Assert.IsTrue(floatingNodes.Contains(_powerNet.GetNodeById(3)));
            Assert.IsTrue(floatingNodes.Contains(_powerNet.GetNodeById(4)));
            Assert.IsTrue(floatingNodes.Contains(_powerNet.GetNodeById(5)));
        }
    }
}
