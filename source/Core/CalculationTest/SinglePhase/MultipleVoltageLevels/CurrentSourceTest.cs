using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IAdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class CurrentSourceTest
    {
        private CurrentSource _currentSource;
        private Mock<IExternalReadOnlyNode> _node;
        private Mock<IAdmittanceMatrix> _admittanceMatrix;
        private Mock<IReadOnlyNode> _groundNode;
        private Complex _internalImpedance;
        private Complex _current;

        [TestInitialize]
        public void SetUp()
        {
            _internalImpedance = new Complex(5, 23);
            _node = new Mock<IExternalReadOnlyNode>();
            _current = new Complex(123, 34);
            _currentSource = new CurrentSource(_node.Object, _current, _internalImpedance, new IdGenerator());
            _admittanceMatrix = new Mock<IAdmittanceMatrix>();
            _groundNode = new Mock<IReadOnlyNode>();
            _node.Setup(x => x.NominalVoltage).Returns(6);
            _groundNode.Setup(x => x.NominalVoltage).Returns(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ZeroCurrent_ExceptionThrown()
        {
            new CurrentSource(_node.Object, new Complex(), new Complex(124, 5), new IdGenerator());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ZeroImpedance_ExceptionThrown()
        {
            new CurrentSource(_node.Object, new Complex(124, 5), new Complex(), new IdGenerator());
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_currentSource.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_currentSource.NeedsGroundNode);
        }

        [TestMethod]
        public void AddConnectedNodes_Empty_NodeGotCallToAddConnectedNodes()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>();

            _currentSource.AddConnectedNodes(visitedNodes);

            _node.Verify(x => x.AddConnectedNodes(visitedNodes), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_Empty_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>();

            _currentSource.AddConnectedNodesOnSameVoltageLevel(visitedNodes);

            _node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(visitedNodes), Times.Once);
        }

        [TestMethod]
        public void GetDirectConnectedNodes_Empty_EmptyList()
        {
            var result = _currentSource.GetDirectConnectedNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidParams_PqNodeWithZeroPower()
        {
            var result = _currentSource.CreateSingleVoltageNode(1, new HashSet<IExternalReadOnlyNode>(), true);

            var resultAsPq = result as PqNode;
            Assert.IsNotNull(resultAsPq);
            Assert.AreEqual(0, resultAsPq.Power.MagnitudeSquared());
        }

        [TestMethod]
        public void GetInternalNodes_Empty_OneDerivedPqNode()
        {
            var result = _currentSource.GetInternalNodes();

            Assert.AreEqual(1, result.Count);
            var node = result[0] as DerivedInternalPQNode;
            Assert.IsNotNull(node);
            Assert.AreEqual(6, node.NominalVoltage);
            _node.Verify(x => x.NominalVoltage, Times.AtLeastOnce);
        }

        [TestMethod]
        public void FillInAdmittances_MockMatrix_MatrixGotOneCallToAddConnection()
        {
            _currentSource.FillInAdmittances(_admittanceMatrix.Object, 10, _groundNode.Object, 1234);
            const double scaleBase = 5.0/18;
            var admittance = 1/_internalImpedance;
            var internalNode = _currentSource.GetInternalNodes().First();

            _admittanceMatrix.Verify(x => x.AddConnection(internalNode, _node.Object, It.Is<Complex>(y => (y - admittance/scaleBase).MagnitudeSquared() < 10e-10)));
        }
    }
}
