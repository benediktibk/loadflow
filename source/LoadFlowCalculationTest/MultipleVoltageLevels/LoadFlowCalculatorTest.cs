using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        private LoadFlowCalculator _calculator;
        private PowerNet _powerNet;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new LoadFlowCalculator(5, 2);
            _powerNet = new PowerNet(50);
        }

        [TestMethod]
        public void Constructor_5AsScaleBaseForVoltage_ScaleBaseForVoltageIs5()
        {
            Assert.AreEqual(5, _calculator.ScaleBasisVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_2AsScaleBaseForPower_ScaleBaseForPowerIs2()
        {
            Assert.AreEqual(2, _calculator.ScaleBasisPower, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForCurrentIsCorrect()
        {
            Assert.AreEqual(_calculator.ScaleBasisPower / _calculator.ScaleBasisVoltage, _calculator.ScaleBasisCurrent, 0.00001);
        }

        [TestMethod]
        public void Constructor_5And2_ScaleBaseForImpedanceIsCorrect()
        {
            Assert.AreEqual(_calculator.ScaleBasisVoltage / _calculator.ScaleBasisCurrent, _calculator.ScaleBasisImpedance, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeVoltages_OneFeedInAndOneLoad_CorrectResults()
        {
            _powerNet.AddNode("feedInNode", 100);
            _powerNet.AddNode("loadNode", 100);
            _powerNet.AddFeedIn("feedInNode", "feedIn", new Complex(100, 0));
            _powerNet.AddLoad("loadNode", "load", new Complex(-10, 0));
            _powerNet.AddLine("connection", "feedInNode", "loadNode", 10, 0, 0, 0);

            var nodeVoltages = _calculator.CalculateNodeVoltages(_powerNet);

            Assert.AreEqual(2, nodeVoltages.Count);
            Assert.IsTrue(nodeVoltages.ContainsKey("feedInNode"));
            Assert.IsTrue(nodeVoltages.ContainsKey("loadNode"));
            ComplexAssert.AreEqual(100, 0, nodeVoltages["feedInNode"], 0.0001);
            ComplexAssert.AreEqual(98.989794855663561963945681494118, 0, nodeVoltages["loadNode"], 0.0001);
        }
    }
}
