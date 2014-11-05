using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class SlackNodeTest
    {
        private SlackNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new SlackNode(new Complex(3, 4));
        }

        [TestMethod]
        public void Constructor_3And4_PowerIs3And4()
        {
            ComplexAssert.AreEqual(3, 4, _node.Voltage, 0.000001);
        }

        [TestMethod]
        public void SetVoltageIn_NullAndValidId_CorrectVoltageSet()
        {
            var voltages = new DenseVector(3);

            _node.SetVoltageIn(voltages, 0);

            ComplexAssert.AreEqual(3, 4, voltages[0], 0.0001);
            ComplexAssert.AreEqual(0, 0, voltages[1], 0.0001);
            ComplexAssert.AreEqual(0, 0, voltages[2], 0.0001);
        }

        [TestMethod]
        public void SetVoltageMagnitudeIn_NullAndValidId_SetsNoValue()
        {
            _node.SetVoltageMagnitudeIn(null, 1);
        }

        [TestMethod]
        public void SetRealPowerIn_NullAndValidId_SetsNoValue()
        {
            _node.SetRealPowerIn(null, 1);
        }

        [TestMethod]
        public void SetPowerIn_NullAndValidId_SetsNoValue()
        {
            _node.SetPowerIn(null, 2);
        }
    }
}
