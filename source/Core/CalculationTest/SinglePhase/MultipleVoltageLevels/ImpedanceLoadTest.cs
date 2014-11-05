using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Misc;
using IAdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class ImpedanceLoadTest
    {
        private ExternalNode _node;
        private ExternalNode _groundNode;
        private ImpedanceLoad _impedanceLoad;
        
        [TestInitialize]
        public void SetUp()
        {
            _node = new ExternalNode(0, 3, "");
            _groundNode = new ExternalNode(1, 0, "");
            _impedanceLoad = new ImpedanceLoad(_node, new Complex(4, 1));
        }

        [TestMethod]
        public void Constructor_NodeWithNominalVoltage3_NominalVoltageIs3()
        {
            Assert.AreEqual(3, _impedanceLoad.NominalVoltage, 0.000001);
        }

        [TestMethod]
        public void Constructor_Impedance4And1_ImpedanceIs4And1()
        {
            ComplexAssert.AreEqual(4, 1, _impedanceLoad.Impedance, 0.000001);
        }

        [TestMethod]
        public void AddConnectedNodes_ValidSet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var impedanceLoad = new ImpedanceLoad(node.Object, new Complex(123, 3));
            var nodes = new HashSet<IExternalReadOnlyNode>();

            impedanceLoad.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ValidSet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var impedanceLoad = new ImpedanceLoad(node.Object, new Complex(123, 3));
            var nodes = new HashSet<IExternalReadOnlyNode>();

            impedanceLoad.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void GetInternalNodes_Empty_CountOfResultIs0()
        {
            var internalNodes = _impedanceLoad.GetInternalNodes();

            Assert.AreEqual(0, internalNodes.Count);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_impedanceLoad.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_True()
        {
            Assert.IsTrue(_impedanceLoad.NeedsGroundNode);
        }

        [TestMethod]
        public void FillInAdmittances_AdmittanceMatrixMock_AdmittanceMatrixGotCallToAddConnection()
        {
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            _impedanceLoad.FillInAdmittances(admittanceMatrix.Object, 5, _groundNode, 1234);

            var admittanceScaled = 9/(5*new Complex(4, 1));
            admittanceMatrix.Verify(x => x.AddConnection(_node, _groundNode, It.Is<Complex>(value => (admittanceScaled - value).MagnitudeSquared() < 0.000001)), Times.Once);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidPowerBase_PqNodeWithNoPower()
        {
            var result = _impedanceLoad.CreateSingleVoltageNode(5);

            var resultAsPqNode = result as PqNode;
            Assert.IsNotNull(resultAsPqNode);
            ComplexAssert.AreEqual(0, 0, resultAsPqNode.Power, 0.000001);
        }
    }
}
