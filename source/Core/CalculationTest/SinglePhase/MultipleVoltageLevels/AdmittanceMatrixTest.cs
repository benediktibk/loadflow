using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;
using IAdmittanceMatrix = Calculation.SinglePhase.SingleVoltageLevel.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        private AdmittanceMatrix _admittanceMatrix;
        private Mock<IAdmittanceMatrix> _singleVoltageAdmittanceMatrix;
        private IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;
        private IReadOnlyNode _firstNode;
        private IReadOnlyNode _secondNode;
        private IReadOnlyNode _thirdNode;

        [TestInitialize]
        public void SetUp()
        {
            _firstNode = new Node(0, 1, "");
            _secondNode = new Node(1, 1, "");
            _thirdNode = new Node(2, 1, "");
            _nodeIndexes = new Dictionary<IReadOnlyNode, int>()
            {
                {_firstNode, 0},
                {_secondNode, 2},
                {_thirdNode, 1},
            };
            _singleVoltageAdmittanceMatrix = new Mock<IAdmittanceMatrix>();
            _admittanceMatrix = new AdmittanceMatrix(_singleVoltageAdmittanceMatrix.Object, _nodeIndexes);
        }

        [TestMethod]
        public void AddConnection_OnceCalled_OneCallToAddConnection()
        {
            var admittance = new Complex(1, 2);

            _admittanceMatrix.AddConnection(_firstNode, _thirdNode, admittance);

            _singleVoltageAdmittanceMatrix.Verify(x => x.AddConnection(0, 1, admittance), Times.Once);
        }

        [TestMethod]
        public void NodeCount_MockReturns4_4()
        {
            _singleVoltageAdmittanceMatrix.Setup(x => x.NodeCount).Returns(4);

            Assert.AreEqual(4, _admittanceMatrix.NodeCount);
        }

        [TestMethod]
        public void Indexer_MockReturns4And5_4And5()
        {
            _singleVoltageAdmittanceMatrix.Setup(x => x[1, 2]).Returns(new Complex(4, 5));

            ComplexAssert.AreEqual(4, 5, _admittanceMatrix[1, 2], 0.00001);
        }

        [TestMethod]
        public void AddVoltageControlledCurrentSource_ValidNodeIds_MockGotCorrectCall()
        {
            _admittanceMatrix.AddVoltageControlledCurrentSource(_firstNode, _secondNode, _thirdNode, _firstNode, 5);

            _singleVoltageAdmittanceMatrix.Verify(x => x.AddVoltageControlledCurrentSource(0, 2, 1, 0, 5), Times.Once);
        }

        [TestMethod]
        public void AddGyrator_ValidNodeIds_MockGotCorrectCall()
        {
            _admittanceMatrix.AddGyrator(_firstNode, _secondNode, _thirdNode, _firstNode, 5);

            _singleVoltageAdmittanceMatrix.Verify(x => x.AddGyrator(0, 2, 1, 0, 5), Times.Once);
        }

        [TestMethod]
        public void AddIdealTransformer_ValidNodeIds_MockGotCorrectCall()
        {
            _admittanceMatrix.AddIdealTransformer(_firstNode, _secondNode, _thirdNode, _firstNode, _secondNode, new Complex(4, 5), 6);

            _singleVoltageAdmittanceMatrix.Verify(x => x.AddIdealTransformer(0, 2, 1, 0, 2, It.Is<Complex>(ratio => (new Complex(4, 5) - ratio).Magnitude < 0.000001), 6), Times.Once);
        }
    }
}
