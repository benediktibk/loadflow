﻿using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LoadTest
    {
        private Node _node;
        private Load _load;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("heinz", 3);
            _load = new Load("hanz", new Complex(4, 1), _node);
        }

        [TestMethod]
        public void Constructor_hanzAsName_NameIshanz()
        {
            Assert.AreEqual("hanz", _load.Name);
        }

        [TestMethod]
        public void Constructor_ValidLoad_LoadIsCorrect()
        {
            ComplexAssert.AreEqual(4, 1, _load.Value, 0.00001);
        }

        [TestMethod]
        public void Constructor_ValidNode_NodeHasNoConnectedElements()
        {
            Assert.AreEqual(0, _node.ConnectedElements.Count);
        }

        [TestMethod]
        public void NominalVoltage_NodeHasNominalVoltageOf3_3()
        {
            Assert.AreEqual(3, _load.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<INode>();
            var load = new Load("feed", new Complex(123, 3), node.Object);
            var nodes = new HashSet<INode>();

            load.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<INode>>()), Times.Once);
        }
    }
}