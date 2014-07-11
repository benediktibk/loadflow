using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        private AdmittanceMatrix _matrix;
        private IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;
        private IReadOnlyNode _firstNode;
        private IReadOnlyNode _secondNode;
        private IReadOnlyNode _thirdNode;

        [TestInitialize]
        public void SetUp()
        {
            _firstNode = new Node("first", 1);
            _secondNode = new Node("second", 1);
            _thirdNode = new Node("third", 1);
            _nodeIndexes = new Dictionary<IReadOnlyNode, int>()
            {
                {_firstNode, 0},
                {_secondNode, 1},
                {_thirdNode, 2},
            };
            _matrix = new AdmittanceMatrix(3, _nodeIndexes);
        }

        [TestMethod]
        public void AddConnection_OnceCalled_CorrectValues()
        {
            _matrix.AddConnection(_firstNode, _thirdNode, new Complex(1, 2));

            var values = _matrix.GetValues();
            ComplexAssert.AreEqual(1, 2, values[0, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[0, 1], 0.00001);
            ComplexAssert.AreEqual(-1, -2, values[0, 2], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 1], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 2], 0.00001);
            ComplexAssert.AreEqual(-1, -2, values[2, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[2, 1], 0.00001);
            ComplexAssert.AreEqual(1, 2, values[2, 2], 0.00001);
        }
    }
}
