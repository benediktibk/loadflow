using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PowerNetComputableTest
    {
        private PowerNetComputable _powerNet;
        private Mock<IAdmittanceMatrix> _admittanceMatrixMock;
        private Mock<INodeVoltageCalculator> _nodeVoltageCalculatorMock;

        [TestInitialize]
        public void SetUp()
        {
            _admittanceMatrixMock = new Mock<IAdmittanceMatrix>();
            _admittanceMatrixMock.Setup(x => x.NodeCount).Returns(3);
            _admittanceMatrixMock.Setup(x => x.CalculateCurrents(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => voltages);
            _nodeVoltageCalculatorMock = new Mock<INodeVoltageCalculator>();
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(100);
            _powerNet = new PowerNetComputable(_nodeVoltageCalculatorMock.Object, _admittanceMatrixMock.Object, 5);
        }
            
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CalculateNodeResults_NoSlackBus_ThrowsException()
        {
            _powerNet.SetNode(0, new Node() { Power = new Complex(1, 2) });
            _powerNet.SetNode(1, new Node() { Power = new Complex(5, 6) });
            _powerNet.SetNode(2, new Node() { RealPower = 3, VoltageMagnitude = 4});

            _powerNet.CalculateNodeResults();
        }

        [TestMethod]
        public void CalculateNodeResults_AllVoltagesKnown_CorrectVoltages()
        {
            _powerNet.SetNode(0, new Node() { Voltage = new Complex(1, 2) });
            _powerNet.SetNode(1, new Node() { Voltage = new Complex(5, 6) });
            _powerNet.SetNode(2, new Node() { Voltage = new Complex(3, 4) });

            var nodeResults = _powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 2, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(5, 6, nodeResults[1].Voltage, 0.00001);
            ComplexAssert.AreEqual(3, 4, nodeResults[2].Voltage, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeResults_AllVoltagesKnown_NodeVoltageCalculatorGotNoCall()
        {
            _powerNet.SetNode(0, new Node() { Voltage = new Complex(1, 2) });
            _powerNet.SetNode(1, new Node() { Voltage = new Complex(5, 6) });
            _powerNet.SetNode(2, new Node() { Voltage = new Complex(3, 4) });

            _powerNet.CalculateNodeResults();

            _nodeVoltageCalculatorMock.Verify(
                x =>
                    x.CalculateUnknownVoltages(It.IsAny<AdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqBus>>(), It.IsAny<IList<PvBus>>()), Times.Never);
        }
    }
}
