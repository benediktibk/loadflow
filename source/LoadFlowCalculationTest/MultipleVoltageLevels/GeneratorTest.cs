using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class GeneratorTest
    {
        private Node _node;
        private Generator _generator;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("ultimateNode", 103);
            _generator = new Generator("dada", _node);
        }

        [TestMethod]
        public void Constructor_NameSetTodada_NameIsdada()
        {
            Assert.AreEqual("dada", _generator.Name);
        }

        [TestMethod]
        public void NominalVoltage_Empty_103()
        {
            Assert.AreEqual(103, _generator.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<INode>();
            var generator = new Generator("feed", node.Object);
            var nodes = new HashSet<INode>();

            generator.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<INode>>()), Times.Once);
        }
    }
}
