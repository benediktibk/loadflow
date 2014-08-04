using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace CalculationTest.ThreePhase
{
    [TestClass]
    public class SymmetricPowerNetTest
    {
        #region variables

        private SymmetricPowerNet _powerNet;
        private INodeVoltageCalculator _calculator;

        #endregion

        #region setup

        [TestInitialize]
        public void SetUp()
        {
            _powerNet = new SymmetricPowerNet(50);
            _calculator = new NewtonRaphsonMethod(0.000001, 1000);
        }

        #endregion

        #region verification with SINCAL

        [TestMethod]
        public void CalculateNodeVoltages_OneLineBetweenFeedInAndLoad_CorrectResults()
        {
            const double omega = 2*Math.PI*50;
            _powerNet.AddNode(1, 400);
            _powerNet.AddNode(2, 400);
            _powerNet.AddFeedIn(1, new Complex(400, 0), 0);
            _powerNet.AddLoad(2, new Complex(-20000, -2000));
            _powerNet.AddLine(1, 2, 0.1, 0.4/omega, 100/(400*400), 1e-10, 1);

            _powerNet.CalculateNodeVoltages(_calculator);

            var sourceVoltage = _powerNet.GetNodeVoltage(1);
            var loadVoltage = _powerNet.GetNodeVoltage(2);
            var sourceVoltageShouldBe = Complex.FromPolarCoordinates(400, 0);
            var loadVoltageShouldBe = Complex.FromPolarCoordinates(392.367, -2.850*Math.PI/180);
            ComplexAssert.AreEqual(sourceVoltageShouldBe, sourceVoltage, 0.01);
            ComplexAssert.AreEqual(loadVoltageShouldBe, loadVoltage, 0.01);
        }

        #endregion
    }
}
