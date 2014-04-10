using System;
using System.Collections.Generic;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class TransformatorTest
    {
        private Node _upperSideNode;
        private Node _lowerSideNode;
        private Transformator _transformator;

        [TestInitialize]
        public void SetUp()
        {
            _upperSideNode = new Node("upper", 10);
            _lowerSideNode = new Node("lower", 0.25);
            _transformator = new Transformator("blub", 2, 3, 4, 5, 6, _upperSideNode, _lowerSideNode);
        }

        [TestMethod]
        public void Constructor_blubAsName_NameIsblub()
        {
            Assert.AreEqual("blub", _transformator.Name);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_UpperSideNodeHasNominalVoltage10_10()
        {
            Assert.AreEqual(10, _transformator.UpperSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_LowerSideNodeHasNominalVoltage025_025()
        {
            Assert.AreEqual(0.25, _transformator.LowerSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var upperSideNode = new Mock<IReadOnlyNode>();
            var lowerSideNode = new Mock<IReadOnlyNode>();
            var transformator = new Transformator("blub", 2, 3, 4, 5, 6, upperSideNode.Object, lowerSideNode.Object);
            var nodes = new HashSet<IReadOnlyNode>();

            transformator.AddConnectedNodes(nodes);

            upperSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
            lowerSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
        }
    }
}
