using System.Collections.Generic;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LineTest
    {
        private Line _line;
        private Node _sourceNode;
        private Node _targetNode;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNode = new Node("source", 102);
            _targetNode = new Node("target", 12);
            _line = new Line("connect", _sourceNode, _targetNode);
        }

        [TestMethod]
        public void Constructor_NameSetToconnect_NameIsconnect()
        {
            Assert.AreEqual("connect", _line.Name);
        }

        [TestMethod]
        public void SourceNominalVoltage_SourceNodeVoltageSetTo102_102()
        {
            Assert.AreEqual(102, _line.SourceNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void TargetNominalVoltage_TargetNodeVoltageSetTo12_12()
        {
            Assert.AreEqual(12, _line.TargetNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var source = new Mock<IReadOnlyNode>();
            var target = new Mock<IReadOnlyNode>();
            var line = new Line("blub", source.Object, target.Object);
            var nodes = new HashSet<IReadOnlyNode>();

            line.AddConnectedNodes(nodes);

            source.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
            target.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
        }
    }
}
