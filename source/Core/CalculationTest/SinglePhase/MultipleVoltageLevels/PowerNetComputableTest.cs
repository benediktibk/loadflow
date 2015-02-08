using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Calculation;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;
using IAdmittanceMatrix = Calculation.SinglePhase.SingleVoltageLevel.IAdmittanceMatrix;
using IPowerNetComputable = Calculation.SinglePhase.SingleVoltageLevel.IPowerNetComputable;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class PowerNetComputableTest
    {
        private Mock<IPowerNetFactory> _powerNetFactoryMock;
        private Mock<IPowerNetComputable> _singleVoltagePowerNetMock;
        private Mock<INodeGraph> _nodeGraphMock;
        private PowerNetComputable _powerNet;
        private double _relativePowerError;

        [TestInitialize]
        public void SetUp()
        {
            _powerNetFactoryMock = new Mock<IPowerNetFactory>();
            _singleVoltagePowerNetMock = new Mock<IPowerNetComputable>();
            _nodeGraphMock = new Mock<INodeGraph>();
            _powerNet = new PowerNetComputable(50, _powerNetFactoryMock.Object, _nodeGraphMock.Object);
            _powerNetFactoryMock.Setup(
                x =>
                    x.Create(It.IsAny<IAdmittanceMatrix>(),
                        It.IsAny<double>(), It.IsAny<IReadOnlyList<Complex>>())).Returns(_singleVoltagePowerNetMock.Object);
            _relativePowerError = 0.1;
        }

        [TestMethod]
        public void CalculateNodeResults_CalculationFails_Null()
        {
            _powerNet.AddNode(0, 1, "");
            _powerNet.AddNode(1, 1, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), new Complex());
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddGenerator(1, 1.02, -0.4);
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);
            var relativePowerError = 0.1;
            _nodeGraphMock.Setup(x => x.Segments)
                .Returns(new ISet<IExternalReadOnlyNode>[] { new HashSet<IExternalReadOnlyNode>{ _powerNet.GetNodeById(0), _powerNet.GetNodeById(1) } });
            _singleVoltagePowerNetMock.Setup(c => c.CalculateNodeResults(out relativePowerError)).Returns((IList<NodeResult>) null);

            var nodeResults = _powerNet.CalculateNodeResults(out relativePowerError);

            Assert.IsNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeResults_CalculationSucceeds_NodeResultsAreUnscaled()
        {
            _powerNet.AddNode(0, 7, "");
            _powerNet.AddNode(1, 7, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), new Complex());
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);
            var sourceVoltageInternal = new Complex(2, 3);
            var sourcePowerInternal = new Complex(4, 5);
            var loadVoltageInternal = new Complex(6, 7);
            var loadPowerInternal = new Complex(8, 9);
            _nodeGraphMock.Setup(x => x.Segments)
                .Returns(new ISet<IExternalReadOnlyNode>[] { new HashSet<IExternalReadOnlyNode> { _powerNet.GetNodeById(0), _powerNet.GetNodeById(1) } });
            _singleVoltagePowerNetMock.Setup(c => c.CalculateNodeResults(out _relativePowerError)).Returns(new List<NodeResult>
            {
                new NodeResult(sourceVoltageInternal, sourcePowerInternal), new NodeResult(loadVoltageInternal, loadPowerInternal)
            });

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            Assert.AreEqual(2, nodeResults.Count);
            var sourceNodeResult = nodeResults[0];
            var loadNodeResult = nodeResults[1];
            ComplexAssert.AreEqual(sourceVoltageInternal * 7, sourceNodeResult.Voltage, 0.00001);
            ComplexAssert.AreEqual(loadVoltageInternal * 7, loadNodeResult.Voltage, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void CalculateNodeResults_NominalVoltagesDoNotMatch_ThrowsException()
        {
            _nodeGraphMock.Setup(x => x.FloatingNodesExist).Returns(false);
            _powerNet.AddNode(0, 7, "");
            _powerNet.AddNode(1, 8, "");
            _powerNet.AddFeedIn(0, new Complex(1.05, 0), new Complex());
            _powerNet.AddLoad(1, new Complex(-0.6, -1));
            _powerNet.AddTransmissionLine(0, 1, 0, 0.00006366197723675813, 0, 0, 1, true);

            _powerNet.CalculateNodeResults(out _relativePowerError);
        }
    }
}
