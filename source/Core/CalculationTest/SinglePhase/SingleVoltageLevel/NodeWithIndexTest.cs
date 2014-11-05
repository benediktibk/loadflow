using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class NodeWithIndexTest
    {
        private NodeWithIndex _nodeWithIndex;
        private Mock<INode> _nodeMock;
        private Vector<Complex> _vector;

        [TestInitialize]
        public void SetUp()
        {
            _nodeMock = new Mock<INode>();
            _nodeWithIndex = new NodeWithIndex(_nodeMock.Object, 4);
            _vector = new DenseVector(5);
        }

        [TestMethod]
        public void Constructor_IndexSetTo4_IndexIs4()
        {
            Assert.AreEqual(4, _nodeWithIndex.Index);
        }

        [TestMethod]
        public void Constructor_NodeMock_NodeIsNodeMock()
        {
            Assert.AreEqual(_nodeMock.Object, _nodeWithIndex.Node);
        }

        [TestMethod]
        public void SetVoltageIn_VoltageVector_NodeMockGotCorrectCall()
        {
            _nodeWithIndex.SetVoltageIn(_vector);

            _nodeMock.Verify(x => x.SetVoltageIn(_vector, 4));
        }

        [TestMethod]
        public void SetVoltageMagnitudeIn_VoltageVector_NodeMockGotCorrectCall()
        {
            _nodeWithIndex.SetVoltageMagnitudeIn(_vector);

            _nodeMock.Verify(x => x.SetVoltageMagnitudeIn(_vector, 4));
        }

        [TestMethod]
        public void SetPowerIn_PowerVector_NodeMockGotCorrectCall()
        {
            _nodeWithIndex.SetPowerIn(_vector);

            _nodeMock.Verify(x => x.SetPowerIn(_vector, 4));
        }

        [TestMethod]
        public void SetRealPowerIn_PowerVector_NodeMockGotCorrectCall()
        {
            _nodeWithIndex.SetRealPowerIn(_vector);

            _nodeMock.Verify(x => x.SetRealPowerIn(_vector, 4));
        }
    }
}
