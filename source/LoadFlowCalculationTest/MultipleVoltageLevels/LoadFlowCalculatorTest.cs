using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        [TestMethod]
        public void Constructor_5AsScaleBaseForVoltage_ScaleBaseForVoltageIs5()
        {
            var calculator = new LoadFlowCalculator(5, 2);

            Assert.AreEqual(5, calculator.ScaleBasisVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_2AsScaleBaseForPower_ScaleBaseForPowerIs2()
        {
            var calculator = new LoadFlowCalculator(5, 2);

            Assert.AreEqual(2, calculator.ScaleBasisPower, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForCurrentIsCorrect()
        {
            var calculator = new LoadFlowCalculator(5, 2);

            Assert.AreEqual(calculator.ScaleBasisPower/calculator.ScaleBasisVoltage, calculator.ScaleBasisCurrent, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForImpedanceIsCorrect()
        {
            var calculator = new LoadFlowCalculator(5, 2);

            Assert.AreEqual(calculator.ScaleBasisVoltage / calculator.ScaleBasisCurrent, calculator.ScaleBasisImpedance, 0.00001);
        }
    }
}
