using System;
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
    }
}
