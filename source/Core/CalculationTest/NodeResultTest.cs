using System.Numerics;
using Calculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest
{
    [TestClass]
    public class NodeResultTest
    {
        private NodeResult _nodeResult;

        [TestInitialize]
        public void SetUp()
        {
            _nodeResult = new NodeResult(new Complex(2, 3), new Complex(4, 5));
        }

        [TestMethod]
        public void Constructor_VoltageSetTo2And3_VoltageIs2And3()
        {
            ComplexAssert.AreEqual(2, 3, _nodeResult.Voltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_PowerSetTo4And5_PowerIs4And5()
        {
            ComplexAssert.AreEqual(4, 5, _nodeResult.Power, 0.00001);
        }

        [TestMethod]
        public void Unscale_ValidScaleBase_VoltageOfResultIsUnscaled()
        {
            var unscaled = _nodeResult.Unscale(10, 100);

            ComplexAssert.AreEqual(2, 3, _nodeResult.Voltage, 0.0000001);
            ComplexAssert.AreEqual(20, 30, unscaled.Voltage, 0.0000001);
        }

        [TestMethod]
        public void Unscale_ValidScaleBase_PowerOfResultIsUnscaled()
        {
            var unscaled = _nodeResult.Unscale(10, 100);

            ComplexAssert.AreEqual(4, 5, _nodeResult.Power, 0.0000001);
            ComplexAssert.AreEqual(400, 500, unscaled.Power, 0.0000001);
        }
    }
}
