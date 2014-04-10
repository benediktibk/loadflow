using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

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
        public void EnforcesPVBus_Empty_True()
        {
            Assert.IsTrue(_generator.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_generator.EnforcesSlackBus);
        }

        [TestMethod]
        public void CreatePVBus_ValidStuff_CorrectResult()
        {
            _generator.CreatePVBus(new Dictionary<IReadOnlyNode, int>(), 3, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetTotalPowerForPQBus_ValidStuff_ThrowsException()
        {
            _generator.GetTotalPowerForPQBus(3);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _generator.GetSlackVoltage(3);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IReadOnlyNode>();
            var generator = new Generator("feed", node.Object);
            var nodes = new HashSet<IReadOnlyNode>();

            generator.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
        }
    }
}
