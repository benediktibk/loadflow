using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PowerNetComputableTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltagesAndPowers_OverdeterminedProblem_ExceptionThrown()
        {
            var nodeVoltageCalculator = new Mock<INodeVoltageCalculator>();
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestWithOverdeterminedProblem(nodeVoltageCalculator.Object);

            powerNetTestCase.CalculateNodeResults();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltagesAndPowers_UnderdeterminedProblem_ExceptionThrown()
        {
            var nodeVoltageCalculator = new Mock<INodeVoltageCalculator>();
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestWithUnderdeterminedProblem(nodeVoltageCalculator.Object);

            powerNetTestCase.CalculateNodeResults();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CalculateNodeVoltagesAndPowers_OnlyPowersKnown_ThrowsException()
        {
            var nodeVoltageCalculator = new Mock<INodeVoltageCalculator>();
            var powerNetTestCase = PowerNetTestCaseGenerator.CreateTestWithNoSlackBus(nodeVoltageCalculator.Object);

            powerNetTestCase.CalculateNodeResults();
        }

        [TestMethod]
        public void CalculatePowerLoss_TwoNodeSystem_CorrectResult()
        {
            var admittances =
                new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
                {{new Complex(1, 0), new Complex(-1, 0)}, {new Complex(-1, 0), new Complex(1, 0)}}));
            var voltages = new DenseVector(new[] {new Complex(1, 0), new Complex(0.5, 0)});

            var powerLoss = PowerNetComputable.CalculatePowerLoss(admittances, voltages);

            ComplexAssert.AreEqual(0.25, 0, powerLoss, 0.0001);
        }

        [TestMethod]
        public void CalculatePowerLoss_ThreeNodeSystem_CorrectResult()
        {
            var admittances = new DenseMatrix(3, 3);
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0.5, 0), new Complex(0.25, 0) });
            admittances[0, 0] = new Complex(1 + 1.0/3, 0);
            admittances[0, 1] = new Complex(-1, 0);
            admittances[0, 2] = new Complex(-1.0 / 3, 0);
            admittances[1, 0] = new Complex(-1, 0);
            admittances[1, 1] = new Complex(1.5, 0);
            admittances[1, 2] = new Complex(-0.5, 0);
            admittances[2, 0] = new Complex(-1.0 / 3, 0);
            admittances[2, 1] = new Complex(-0.5, 0);
            admittances[2, 2] = new Complex(1.0 / 3 + 0.5, 0);

            var powerLoss = PowerNetComputable.CalculatePowerLoss(new AdmittanceMatrix(admittances), voltages);

            ComplexAssert.AreEqual(0.46875, 0, powerLoss, 0.0000001);
        }
    }
}
