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
    }
}
