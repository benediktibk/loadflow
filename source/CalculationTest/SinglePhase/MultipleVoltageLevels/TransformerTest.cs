using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;
using IAdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class TransformerTest
    {
        private Node _upperSideNode;
        private Node _lowerSideNode;
        private Node _groundNode;
        private Transformer _transformerWithNotNominalRatio;
        private Transformer _transformerWithNominalRatio;
        private IdGenerator _idGenerator;

        [TestInitialize]
        public void SetUp()
        {
            _idGenerator = new IdGenerator();
            _upperSideNode = new Node(0, 10, 0, "");
            _lowerSideNode = new Node(1, 0.25, 0, "");
            _groundNode = new Node(2, 0, 0, "");
            _transformerWithNotNominalRatio = new Transformer(_upperSideNode, _lowerSideNode, 50, 0.2, 4, 5, 0.1, 2, "", _idGenerator);
            _transformerWithNominalRatio = new Transformer(_upperSideNode, _lowerSideNode, 50, 0.2, 4, 5, 0.1, 40, "", _idGenerator);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_UpperSideNodeHasNominalVoltage10_10()
        {
            Assert.AreEqual(10, _transformerWithNotNominalRatio.UpperSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_LowerSideNodeHasNominalVoltage025_025()
        {
            Assert.AreEqual(0.25, _transformerWithNotNominalRatio.LowerSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var upperSideNode = new Mock<IExternalReadOnlyNode>();
            var lowerSideNode = new Mock<IExternalReadOnlyNode>();
            var transformer = new Transformer(upperSideNode.Object, lowerSideNode.Object, 50, 0.2, 4, 5, 0.1, 2, "", _idGenerator);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            transformer.AddConnectedNodes(nodes);

            upperSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
            lowerSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_transformerWithNotNominalRatio.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_transformerWithNotNominalRatio.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _transformerWithNotNominalRatio.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            var result = _transformerWithNotNominalRatio.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(0, 0, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _transformerWithNotNominalRatio.GetSlackVoltage(45);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_transformerWithNotNominalRatio.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_True()
        {
            Assert.IsTrue(_transformerWithNotNominalRatio.NeedsGroundNode);
        }

        [TestMethod]
        public void Ratio_Empty_2()
        {
            Assert.AreEqual(2, _transformerWithNotNominalRatio.Ratio, 0.000001);
        }

        [TestMethod]
        public void NominalRatio_Empty_40()
        {
            Assert.AreEqual(40, _transformerWithNotNominalRatio.NominalRatio, 0.000001);
        }

        [TestMethod]
        public void RelativeRatio_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(2.0/40, 0, _transformerWithNotNominalRatio.RelativeRatio, 0.000001);
        }

        [TestMethod]
        public void LengthAdmittance_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(1.000000007, -2.291287854, _transformerWithNotNominalRatio.LengthAdmittance, 0.00001);
        }

        [TestMethod]
        public void ShuntAdmittance_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(0.025, -0.025, _transformerWithNotNominalRatio.ShuntAdmittance, 0.00001);
        }

        [TestMethod]
        public void GetInternalNodes_NominalRatio_ResultCountIs0()
        {
            var internalNodes = _transformerWithNominalRatio.GetInternalNodes();

            Assert.AreEqual(0, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NotNominalRatio_ResultCountIs2()
        {
            var internalNodes = _transformerWithNotNominalRatio.GetInternalNodes();

            Assert.AreEqual(2, internalNodes.Count);
        }

        [TestMethod]
        public void FillInAdmittances_NominalRatio_ThreeConnections()
        {
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            _transformerWithNominalRatio.FillInAdmittances(admittanceMatrix.Object, 1, _groundNode, 1);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
            admittanceMatrix.Verify(x => x.AddGyrator(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddVoltageControlledCurrentSource(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>(), 1),
                Times.Never);
        }

        [TestMethod]
        public void FillInAdmittances_NotNominalRatio_ThreeConnectionsAndOneIdealTransformer()
        {
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            _transformerWithNotNominalRatio.FillInAdmittances(admittanceMatrix.Object, 1, _groundNode, 1);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>(), It.IsAny<double>()),
                Times.Once);
        }
    }
}
