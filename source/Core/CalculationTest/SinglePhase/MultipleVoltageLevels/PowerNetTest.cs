using System;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

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
        public void GetSetsOfConnectedNodes_NoNodes_EmptyList()
        {
            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(0, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_OneNode_CorrectResult()
        {
            _powerNet.AddNode(0, 120, 0, "");

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
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodes_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodes();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_NoNodes_EmptyList()
        {
            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(0, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_OneNode_CorrectResult()
        {
            _powerNet.AddNode(0, 120, 0, "");

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(1, sets.Count);
            var firstSet = sets.First();
            Assert.AreEqual(1, firstSet.Count);
            var node = firstSet.First();
            Assert.AreEqual(0, node.Id);
            Assert.AreEqual(120, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_TwoNotConnectedNodes_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_TwoConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(2, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_ThreeInARowConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_ThreeInACircleConnectedNodes_CorrectResults()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(1, sets.Count);
            var set = sets.First();
            Assert.AreEqual(3, set.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_ThreeInACircleConnectedNodesAndOneFloating_TwoElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(2, sets.Count);
        }

        [TestMethod]
        public void GetSetsOfConnectedNodesOnSameVoltageLevel_ThreeInACircleByTransformersConnectedConnectedNodes_ThreeElements()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), 0, 0, 0);
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");

            var sets = _powerNet.GetSetsOfConnectedNodesOnSameVoltageLevel();

            Assert.AreEqual(3, sets.Count);
        }

        [TestMethod]
        public void GetNominalPhaseShiftPerNode_AFewNodesConnectedByTransformers_PhaseShiftsAreCorrect()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddFeedIn(0, new Complex(234, 0), 0, 0, 0);
            _powerNet.AddTwoWindingTransformer(1, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.8), "");
            _powerNet.AddTwoWindingTransformer(0, 2, 3, 0.5, 0.1, 0, 1, 1, new Angle(-1.5), "");
            _powerNet.AddTwoWindingTransformer(0, 1, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.3), "");
            _powerNet.AddTwoWindingTransformer(2, 3, 3, 0.5, 0.1, 0, 1, 1, new Angle(0.2), "");

            var phaseShifts = _powerNet.GetNominalPhaseShiftPerNode();

            Assert.IsTrue(Angle.Equal(new Angle(), phaseShifts[_powerNet.GetNodeById(0)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(0.3), phaseShifts[_powerNet.GetNodeById(1)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(-1.5), phaseShifts[_powerNet.GetNodeById(2)], 0.000001));
            Assert.IsTrue(Angle.Equal(new Angle(-1.3), phaseShifts[_powerNet.GetNodeById(3)], 0.000001));
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodesAndOneFloating_True()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddNode(3, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoNotConnectedNodes_True()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");

            Assert.IsTrue(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_TwoConnectedNodesWithGround_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 5, 2, 1, true);

            Assert.IsFalse(_powerNet.CheckIfFloatingNodesExists());
        }

        [TestMethod]
        public void CheckIfFloatingNodesExists_ThreeInACircleConnectedNodes_False()
        {
            _powerNet.AddNode(0, 123, 0, "");
            _powerNet.AddNode(1, 120, 0, "");
            _powerNet.AddNode(2, 120, 0, "");
            _powerNet.AddTransmissionLine(1, 2, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 1, 3, 5, 0, 0, 1, true);
            _powerNet.AddTransmissionLine(0, 2, 3, 5, 0, 0, 1, true);

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
