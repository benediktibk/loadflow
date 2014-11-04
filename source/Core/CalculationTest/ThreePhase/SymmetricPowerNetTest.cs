using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.ThreePhase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.ThreePhase
{
    [TestClass]
    public class SymmetricPowerNetTest
    {
        private SymmetricPowerNet _powerNet;
        private Mock<IPowerNetComputable> _singlePhasePowerNetMock;

        [TestInitialize]
        public void SetUp()
        {
            _singlePhasePowerNetMock = new Mock<IPowerNetComputable>();
            _powerNet = new SymmetricPowerNet(_singlePhasePowerNetMock.Object);
        }

        [TestMethod]
        public void SlackPhaseShift_MockPowerNet_SlackPhaseShiftOfMockPowerNet()
        {
            var resultShouldBe = new Angle(2);
            _singlePhasePowerNetMock.Setup(x => x.SlackPhaseShift).Returns(resultShouldBe);

            var result = _powerNet.SlackPhaseShift;

            Assert.IsTrue(Angle.Equal(resultShouldBe, result, 0.000001));
        }

        [TestMethod]
        public void AddNode_ValidData_MockPowerNetGotCallToAddCorrectNode()
        {
            _powerNet.AddNode(3, 5, "asdf");

            _singlePhasePowerNetMock.Verify(x => x.AddNode(3, 5/Math.Sqrt(3), "asdf"),
                Times.Once);
        }

        [TestMethod]
        public void AddTransmissionLine_ValidData_MockPowerNetGotCallToAddCorrectTransmissionLine()
        {
            _powerNet.AddTransmissionLine(3, 4, 5, 6, 7, 8, 9, false);

            _singlePhasePowerNetMock.Verify(x => x.AddTransmissionLine(3, 4, 5, 6, 7, 8, 9, false),
                Times.Once);
        }

        [TestMethod]
        public void AddGenerator_ValidData_MockPowerNetGotCallToAddCorrectGenerator()
        {
            _powerNet.AddGenerator(3, 4, 5);

            _singlePhasePowerNetMock.Verify(x => x.AddGenerator(3, 4/Math.Sqrt(3), 5.0/3),
                Times.Once);
        }

        [TestMethod]
        public void AddFeedIn_ValidData_MockPowerNetGotCallToAddCorrectFeedIn()
        {
            _powerNet.AddFeedIn(3, new Complex(4, 5), 6, 7, 8);

            _singlePhasePowerNetMock.Verify(x => x.AddFeedIn(3, new Complex(4, 5) / Math.Sqrt(3), 6.0/3, 7, 8),
                Times.Once);
        }

        [TestMethod]
        public void AddTwoWindingTransformer_ValidData_MockPowerNetGotCallToAddCorrectTwoWindingTransformer()
        {
            _powerNet.AddTwoWindingTransformer(3, 4, 5, 6, 7, 8, 9, 10, new Angle(2), "asdf");

            _singlePhasePowerNetMock.Verify(x => x.AddTwoWindingTransformer(3, 4, 5.0 / 3, 6, 7.0 / 3, 8.0 / 3, 9, 10, It.Is<Angle>(a => Angle.Equal(a, new Angle(2), 0.00001)), "asdf"),
                Times.Once);
        }

        [TestMethod]
        public void AddThreeWindingTransformer_ValidData_MockPowerNetGotCallToAddCorrectThreeWindingTransformer()
        {
            _powerNet.AddThreeWindingTransformer(3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, new Angle(1),
                new Angle(2), new Angle(3), "asdf");

            _singlePhasePowerNetMock.Verify(x => x.AddThreeWindingTransformer(
                3, 4, 5, 6.0 / 3, 7.0 / 3, 8.0 / 3, 9, 10, 11, 12.0 / 3, 13.0 / 3, 14.0 / 3, 15.0 / 3, 16,
                It.Is<Angle>(a => Angle.Equal(a, new Angle(1), 0.00001)),
                It.Is<Angle>(a => Angle.Equal(a, new Angle(2), 0.00001)),
                It.Is<Angle>(a => Angle.Equal(a, new Angle(3), 0.00001)), "asdf"),
                Times.Once);
        }

        [TestMethod]
        public void AddLoad_ValidData_MockPowerNetGotCallToAddCorrectLoad()
        {
            _powerNet.AddLoad(3, new Complex(4, 5));

            _singlePhasePowerNetMock.Verify(x => x.AddLoad(3, new Complex(4, 5) / 3),
                Times.Once);
        }

        [TestMethod]
        public void AddImpedanceLoad_ValidData_MockPowerNetGotCallToAddCorrectImpedanceLoad()
        {
            _powerNet.AddImpedanceLoad(3, new Complex(4, 5));

            _singlePhasePowerNetMock.Verify(x => x.AddImpedanceLoad(3, new Complex(4, 5)),
                Times.Once);
        }

        [TestMethod]
        public void CalculateAdmittanceMatrix_MockPowerNet_MockGotCallToCalculateAdmittanceMatrix()
        {
            AdmittanceMatrix matrix;
            IReadOnlyList<string> nodeNames;
            double powerBase;
            _singlePhasePowerNetMock.Setup(x => x.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase));

            _powerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);

            _singlePhasePowerNetMock.Verify(x => x.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase), Times.Once);
        }

        [TestMethod]
        public void CalculateNominalPhaseShiftPerNode_MockPowerNet_MockGotCallToCalculateNominalPhaseShiftPerNode()
        {
            var resultShouldBe = new Dictionary<IExternalReadOnlyNode, Angle>();
            _singlePhasePowerNetMock.Setup(x => x.NominalPhaseShiftPerNode).Returns(resultShouldBe);

            var result = _powerNet.CalculateNominalPhaseShiftPerNode();

            Assert.AreEqual(resultShouldBe, result);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MockPowerNet_MockGotCallToCalculateNodeVoltages()
        {
            _powerNet.CalculateNodeVoltages();

            _singlePhasePowerNetMock.Verify(x => x.CalculateNodeResults(), Times.Once);
        }

        [TestMethod]
        public void CalculateNodeVoltages_CalculationFailed_Null()
        {
            var result = _powerNet.CalculateNodeVoltages();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void CalculateNodeVoltages_MockPowerNet_NodeResultsAreUnscaled()
        {
            _singlePhasePowerNetMock.Setup(x => x.CalculateNodeResults())
                .Returns(new Dictionary<long, NodeResult> {{3, new NodeResult(new Complex(1, 2), new Complex(3, 4))}});

            var result = _powerNet.CalculateNodeVoltages();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, result.First().Key);
            var nodeResult = result.First().Value;
            ComplexAssert.AreEqual(new Complex(1, 2) * Math.Sqrt(3), nodeResult.Voltage, 0.00001);
            ComplexAssert.AreEqual(new Complex(3, 4) * 3, nodeResult.Power, 0.00001);
        }
    }
}
