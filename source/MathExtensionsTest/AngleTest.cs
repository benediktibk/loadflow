using System;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExtensionsTest
{
    [TestClass]
    public class AngleTest
    {
        [TestMethod]
        public void Radiant_RadiantValueSetTo2_2()
        {
            var angle = new Angle(2);

            Assert.AreEqual(2, angle.Radiant, 0.00001);
        }

        [TestMethod]
        public void Degree_RadiantValueSetToPi_180()
        {
            var angle = new Angle(Math.PI);

            Assert.AreEqual(180, angle.Degree, 0.00001);
        }

        [TestMethod]
        public void Degree_RadiantValueSetToMinusPiHalf_270()
        {
            var angle = new Angle(Math.PI/2*(-1));

            Assert.AreEqual(270, angle.Degree, 0.00001);
        }

        [TestMethod]
        public void Degree_RadiantValueSetToHighNegativeValue_CorrectValue()
        {
            var angle = new Angle(Math.PI/2 * (-1) - 10*Math.PI);

            Assert.AreEqual(270, angle.Degree, 0.00001);
        }

        [TestMethod]
        public void Degree_RadiantValueSetToHighValue_CorrectValue()
        {
            var angle = new Angle(Math.PI / 2 + 10 * Math.PI);

            Assert.AreEqual(90, angle.Degree, 0.00001);
        }

        [TestMethod]
        public void OperatorPlus_2And9_CorrectValue()
        {
            var one = new Angle(2);
            var two = new Angle(9);

            var result = one + two;

            Assert.AreEqual(11 - 2*Math.PI, result.Radiant, 0.00001);
        }

        [TestMethod]
        public void OperatorMinus_2And9_CorrectValue()
        {
            var one = new Angle(2);
            var two = new Angle(9);

            var result = one - two;

            Assert.AreEqual(-7 + 4 * Math.PI, result.Radiant, 0.00001);
        }
    }
}
