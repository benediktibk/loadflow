using System;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class IdGeneratorTest
    {
        private IdGenerator _generator;

        [TestInitialize]
        public void SetUp()
        {
            _generator = new IdGenerator();
        }

        [TestMethod]
        public void Constructor_Empty_CountIs0()
        {
            Assert.AreEqual(0, _generator.Count);
        }

        [TestMethod]
        public void Add_5_CountIs1()
        {
            _generator.Add(5);

            Assert.AreEqual(1, _generator.Count);
        }

        [TestMethod]
        public void Add_ThreeTimesCalled_CountIs3()
        {
            _generator.Add(0);
            _generator.Add(5);
            _generator.Add(2);

            Assert.AreEqual(3, _generator.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_SameIdTwiceCalled_ExceptionIsThrown()
        {
            _generator.Add(5);
            _generator.Add(5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Add_Minus2_ExceptionIsThrown()
        {
            _generator.Add(-2);
        }

        [TestMethod]
        public void Generate_ThriceCalled_NegativeValues()
        {
            Assert.IsTrue(_generator.Generate() < 0);
            Assert.IsTrue(_generator.Generate() < 0);
            Assert.IsTrue(_generator.Generate() < 0);
        }

        [TestMethod]
        public void Generate_ThriceCalled_ThreeDifferentValues()
        {
            var first = _generator.Generate();
            var second = _generator.Generate();
            var third = _generator.Generate();

            Assert.AreNotEqual(first, second);
            Assert.AreNotEqual(first, third);
            Assert.AreNotEqual(second, third);
        }

        [TestMethod]
        public void Generate_OnceCalled_ResultIsInUse()
        {
            var value = _generator.Generate();

            Assert.IsTrue(_generator.IsAlreadyUsed(value));
        }

        [TestMethod]
        public void IsAlreadyUsed_NothingAdded_False()
        {
            Assert.IsFalse(_generator.IsAlreadyUsed(4));
        }
    }
}
