using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DimensionScalerTest
    {
        private DimensionScaler _scaler;

        [TestInitialize]
        public void Setup()
        {
            _scaler = new DimensionScaler(4, 3);
        }

        [TestMethod]
        public void Constructor_4And3_AllBasesAreCorrect()
        {
            Assert.AreEqual(4, _scaler.VoltageBase, 0.00001);
            Assert.AreEqual(3, _scaler.PowerBase, 0.00001);
            Assert.AreEqual(3.0/4, _scaler.CurrentBase, 0.00001);
            Assert.AreEqual(4.0*4/3, _scaler.ImpedanceBase, 0.00001);
        }

        [TestMethod]
        public void ScaleVoltage_Double_CorrectResult()
        {
            Assert.AreEqual(5.0/4, _scaler.ScaleVoltage(5), 0.00001);
        }

        [TestMethod]
        public void ScaleVoltage_Complex_CorrectResult()
        {
            ComplexAssert.AreEqual(10.0/4, 3.0/4, _scaler.ScaleVoltage(new Complex(10, 3)), 0.00001);
        }

        [TestMethod]
        public void UnscaleVoltage_Complex_CorrectResult()
        {
            var source = new Complex(10, 3);

            var result = _scaler.UnscaleVoltage(source);

            ComplexAssert.AreEqual(10*4, 3*4, result, 0.00001);
        }

        [TestMethod]
        public void ScalePower_Double_CorrectResult()
        {
            Assert.AreEqual(5.0 / 3, _scaler.ScalePower(5), 0.00001);
        }

        [TestMethod]
        public void ScalePower_Complex_CorrectResult()
        {
            ComplexAssert.AreEqual(10.0 / 3, 1, _scaler.ScalePower(new Complex(10, 3)), 0.00001);
        }

        [TestMethod]
        public void ScaleCurrent_Double_CorrectResult()
        {
            Assert.AreEqual(20.0 / 3 , _scaler.ScaleCurrent(5), 0.00001);
        }

        [TestMethod]
        public void ScaleCurrent_Complex_CorrectResult()
        {
            ComplexAssert.AreEqual(40.0 / 3, 4, _scaler.ScaleCurrent(new Complex(10, 3)), 0.00001);
        }

        [TestMethod]
        public void ScaleImpedance_Double_CorrectResult()
        {
            Assert.AreEqual(15.0 / 16, _scaler.ScaleImpedance(5), 0.00001);
        }

        [TestMethod]
        public void ScaleImpedance_Complex_CorrectResult()
        {
            ComplexAssert.AreEqual(30.0 / 16, 9.0 / 16, _scaler.ScaleImpedance(new Complex(10, 3)), 0.00001);
        }

        [TestMethod]
        public void ScaleAdmittance_Double_CorrectResult()
        {
            Assert.AreEqual(80.0 / 3, _scaler.ScaleAdmittance(5), 0.00001);
        }

        [TestMethod]
        public void ScaleAdmittance_Complex_CorrectResult()
        {
            ComplexAssert.AreEqual(160.0 / 3, 16, _scaler.ScaleAdmittance(new Complex(10, 3)), 0.00001);
        }
    }
}
