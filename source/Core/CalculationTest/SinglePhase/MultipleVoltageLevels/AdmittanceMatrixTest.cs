using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;
using IAdmittanceMatrix = Calculation.SinglePhase.SingleVoltageLevel.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        private AdmittanceMatrix _matrix;
        private Mock<IAdmittanceMatrix> _singleVoltageAdmittanceMatrix;
        private IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;
        private IReadOnlyNode _firstNode;
        private IReadOnlyNode _secondNode;
        private IReadOnlyNode _thirdNode;

        [TestInitialize]
        public void SetUp()
        {
            _firstNode = new Node(0, 1, 0, "");
            _secondNode = new Node(1, 1, 0, "");
            _thirdNode = new Node(2, 1, 0, "");
            _nodeIndexes = new Dictionary<IReadOnlyNode, int>()
            {
                {_firstNode, 0},
                {_secondNode, 2},
                {_thirdNode, 1},
            };
            _singleVoltageAdmittanceMatrix = new Mock<IAdmittanceMatrix>();
            _matrix = new AdmittanceMatrix(_singleVoltageAdmittanceMatrix.Object, _nodeIndexes);
        }

        [TestMethod]
        public void AddConnection_OnceCalled_OneCallToAddConnection()
        {
            var admittance = new Complex(1, 2);

            _matrix.AddConnection(_firstNode, _thirdNode, admittance);

            _singleVoltageAdmittanceMatrix.Verify(x => x.AddConnection(0, 1, admittance), Times.Once);
        }
    }
}
