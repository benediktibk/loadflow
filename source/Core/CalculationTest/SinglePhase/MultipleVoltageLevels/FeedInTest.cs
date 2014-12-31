using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;
using IAdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class FeedInTest
    {
        private ExternalNode _node;
        private FeedIn _feedIn;
        private IdGenerator _idGenerator;

        [TestInitialize]
        public void SetUp()
        {
            _idGenerator = new IdGenerator();
            _node = new ExternalNode(0, 2, "");
            _feedIn = new FeedIn(_node, new Complex(4, 3), new Complex(6, 5), _idGenerator);
        }

        [TestMethod]
        public void Constructor_ValidVoltageSet_VoltageIsCorrect()
        {
            ComplexAssert.AreEqual(new Complex(4, 3), _feedIn.Voltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_NoInternalImpedance_ThrowsNoException()
        {
            var feedIn = new FeedIn(_node, new Complex(4, 3), new Complex(), _idGenerator);
            ComplexAssert.AreEqual(new Complex(), feedIn.InternalImpedance, 0.0001);
        }

        [TestMethod]
        public void NominalVoltage_Empty_SameAsNode()
        {
            Assert.AreEqual(2, _feedIn.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void InputImpedance_ShortCircuitPowerNotZero_CorrectResult()
        {
            ComplexAssert.AreEqual(6, 5, _feedIn.InternalImpedance, 0.0001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var feedIn = new FeedIn(node.Object, new Complex(123, 3), new Complex(5, 6), _idGenerator);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            feedIn.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var feedIn = new FeedIn(node.Object, new Complex(123, 3), new Complex(5, 6), _idGenerator);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            feedIn.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_feedIn.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_InternalImpedanceSetTo0_EmptyList()
        {
            var feedIn = new FeedIn(_node, new Complex(123, 4), new Complex(), _idGenerator);

            var result = feedIn.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetInternalNodes_InternalImpedanceNot0_OneSlackNode()
        {
            var result = _feedIn.GetInternalNodes();

            Assert.AreEqual(1, result.Count);
            var node = result[0];
            Assert.IsTrue((node as DerivedInternalSlackNode) != null);
        }

        [TestMethod]
        public void FillInAdmittances_InternalImpedanceSetTo0_NoChangeInAdmittanceMatrix()
        {
            var feedIn = new FeedIn(_node, new Complex(123, 4), new Complex(), _idGenerator);

            feedIn.FillInAdmittances(null, 1, null, 1);
        }

        [TestMethod]
        public void FillInAdmittances_InternalImpedanceNot0_CallToAddConnection()
        {
            var internalNodes = _feedIn.GetInternalNodes();
            var internalNode = internalNodes[0];
            var admittances = new Mock<IAdmittanceMatrix>();
            var scaler = new DimensionScaler(_node.NominalVoltage, 3);
            var inputAdmittanceScaled = scaler.ScaleAdmittance(1/_feedIn.InternalImpedance);

            _feedIn.FillInAdmittances(admittances.Object, 3, null, 2);

            admittances.Verify(
                x => x.AddConnection(internalNode, _node, inputAdmittanceScaled),
                Times.Once);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_feedIn.NeedsGroundNode);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_NominalVoltageSetTo0_SlackNodeWithVoltage0()
        {
            var node = new ExternalNode(1, 0, "");
            var feedIn = new FeedIn(node, new Complex(), new Complex(3, 4), _idGenerator);

            var result = feedIn.CreateSingleVoltageNode(5);

            var resultAsSlackNode = result as SlackNode;
            Assert.IsNotNull(resultAsSlackNode);
            ComplexAssert.AreEqual(0, 0, resultAsSlackNode.Voltage, 0.000001);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidPowerBase_SlackNodeWithScaledVoltage()
        {
            var result = _feedIn.CreateSingleVoltageNode(5);

            var resultAsSlackNode = result as SlackNode;
            Assert.IsNotNull(resultAsSlackNode);
            ComplexAssert.AreEqual(2, 1.5, resultAsSlackNode.Voltage, 0.000001);
        }
    }
}
