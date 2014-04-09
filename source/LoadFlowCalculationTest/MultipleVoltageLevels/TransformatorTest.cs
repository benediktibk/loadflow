using System;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
