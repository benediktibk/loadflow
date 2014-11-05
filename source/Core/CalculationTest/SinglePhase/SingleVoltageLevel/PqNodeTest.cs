using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PqNodeTest
    {
        private PqNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new PqNode(new Complex(3, 4));
        }

        [TestMethod]
        public void Constructor_3And4_PowerIs3And4()
        {
            ComplexAssert.AreEqual(3, 4, _node.Power, 0.000001);
        }

        [TestMethod]
        public void SetVoltageIn_NullAndValidId_SetsNoValue()
        {
            _node.SetVoltageIn(null, 1);
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
        public void SetPowerIn_ValidId_CorrectValueSet()
        {
            var powers = new DenseVector(3);

            _node.SetPowerIn(powers, 2);

            ComplexAssert.AreEqual(0, 0, powers[0], 0.0001);
            ComplexAssert.AreEqual(0, 0, powers[1], 0.0001);
            ComplexAssert.AreEqual(3, 4, powers[2], 0.0001);
        }
    }
}
