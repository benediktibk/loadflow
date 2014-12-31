using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace MiscTest
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

        [TestMethod]
        public void OperatorMinus_10DegreeAnd30Degree_CorrectValue()
        {
            var one = Angle.FromDegree(10.523127391536455);
            var two = Angle.FromDegree(30);

            var result = one - two;

            Assert.AreEqual(10.523127391536455 - 30 + 360, result.Degree, 0.00001);
        }

        [TestMethod]
        public void Constructor_Empty_0()
        {
            var angle = new Angle();

            Assert.AreEqual(0, angle.Radiant, 0.00001);
        }

        [TestMethod]
        public void FromDegree_90_HalfPi()
        {
            var angle = Angle.FromDegree(90);

            Assert.AreEqual(Math.PI/2, angle.Radiant, 0.00001);
        }

        [TestMethod]
        public void OperatorMultiply_AngleAndDouble_CorrectValue()
        {
            var angle = new Angle(Math.PI / 2);

            var result = angle*(-1);

            Assert.AreEqual(3 * Math.PI / 2, result.Radiant, 0.00001);
        }

        [TestMethod]
        public void OperatorMultiply_DoubleAndAngle_CorrectValue()
        {
            var angle = new Angle(Math.PI / 2);

            var result = (-1) * angle;

            Assert.AreEqual(3 * Math.PI / 2, result.Radiant, 0.00001);
        }

        [TestMethod]
        public void Equal_OneSlightlyBiggerThanTwo_True()
        {
            var one = new Angle(2);
            var two = new Angle(2.00001);

            Assert.IsTrue(Angle.Equal(one, two, 0.001));
        }

        [TestMethod]
        public void Equal_TwoSlightlyBiggerThanOne_True()
        {
            var one = new Angle(2.00001);
            var two = new Angle(2);

            Assert.IsTrue(Angle.Equal(one, two, 0.001));
        }

        [TestMethod]
        public void Equal_TotallyDifferentValues_False()
        {
            var one = new Angle(3);
            var two = new Angle(2);

            Assert.IsFalse(Angle.Equal(one, two, 0.001));
        }

        [TestMethod]
        public void Equal_0AndTwoPi_True()
        {
            var one = new Angle(0);
            var two = new Angle(2*Math.PI-0.0001);

            Assert.IsTrue(Angle.Equal(one, two, 0.001));
        }

        [TestMethod]
        public void Equal_0AndTwoPiVersionTwo_True()
        {
            var one = new Angle(0);
            var two = new Angle(6.2831853071795862);

            Assert.IsTrue(Angle.Equal(one, two, 0.000001));
        }

        [TestMethod]
        public void Equal_Minus10DegreeAndZero_False()
        {
            var one = Angle.FromDegree(-10);
            var two = new Angle();

            Assert.IsFalse(Angle.Equal(one, two, 1e-6));
        }

        [TestMethod]
        public void Degree_270DegreeAsValue_270()
        {
            var angle = Angle.FromDegree(270);

            Assert.AreEqual(270, angle.Degree, 0.000001);
        }

        [TestMethod]
        public void DegreeAroundZero_270DegreeAsValue_Minus90()
        {
            var angle = Angle.FromDegree(270);

            Assert.AreEqual(-90, angle.DegreeAroundZero, 0.000001);
        }

        [TestMethod]
        public void DegreeAroundZero_90DegreeAsValue_90()
        {
            var angle = Angle.FromDegree(90);

            Assert.AreEqual(90, angle.DegreeAroundZero, 0.000001);
        }
    }
}
