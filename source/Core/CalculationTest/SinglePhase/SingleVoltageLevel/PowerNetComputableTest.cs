using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics;
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
        private double _relativePowerError;

        [TestInitialize]
        public void SetUp()
        {
            _admittanceMatrixMock = new Mock<IAdmittanceMatrix>();
            _admittanceMatrixMock.Setup(x => x.NodeCount).Returns(3);
            _admittanceMatrixMock.Setup(x => x.CalculateCurrents(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => 2*voltages);
            _nodeVoltageCalculatorMock = new Mock<INodeVoltageCalculator>();
            _powerNet = new PowerNetComputable(_nodeVoltageCalculatorMock.Object, _admittanceMatrixMock.Object, 5, new Complex[3]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
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
        [ExpectedException(typeof(ArgumentException))]
        public void CalculateNodeResults_NoSlackBus_ThrowsException()
        {
            _powerNet.AddNode(new PqNode(new Complex(1, 2)));
            _powerNet.AddNode(new PqNode(new Complex(5, 6)));
            _powerNet.AddNode(new PvNode(3, 4));

            _powerNet.CalculateNodeResults(out _relativePowerError);
        }

        [TestMethod]
        public void CalculateNodeResults_AllVoltagesKnown_CorrectVoltages()
        {
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(100);
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new SlackNode(new Complex(5, 6)));
            _powerNet.AddNode(new SlackNode(new Complex(3, 4)));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 2, nodeResults[0].Voltage, 0.00001);
            ComplexAssert.AreEqual(5, 6, nodeResults[1].Voltage, 0.00001);
            ComplexAssert.AreEqual(3, 4, nodeResults[2].Voltage, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeResults_AllVoltagesKnown_NodeVoltageCalculatorGotNoCall()
        {
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new SlackNode(new Complex(5, 6)));
            _powerNet.AddNode(new SlackNode(new Complex(3, 4)));

            _powerNet.CalculateNodeResults(out _relativePowerError);

            _nodeVoltageCalculatorMock.Verify(
                x =>
                    x.CalculateUnknownVoltages(It.IsAny<AdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>()), Times.Never);
        }

        [TestMethod]
        public void CalculateNodeResults_RelativePowerErrorTooBig_Null()
        {
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new DenseVector(new[] { new Complex(10, 10), new Complex(10, 10), new Complex(10, 10) }));
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(0);
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new SlackNode(new Complex(5, 6)));
            _powerNet.AddNode(new SlackNode(new Complex(3, 4)));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeResults_AllVoltagesKnown_CorrectPowers()
        {
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(100);
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new DenseVector(new []{new Complex(1, 2), new Complex(3, 4), new Complex(5, 6) }));
            var voltageOne = new Complex(1, 2);
            var voltageTwo = new Complex(5, 6);
            var voltageThree = new Complex(3, 4);
            _powerNet.AddNode(new SlackNode(voltageOne));
            _powerNet.AddNode(new SlackNode(voltageTwo));
            _powerNet.AddNode(new SlackNode(voltageThree));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
            ComplexAssert.AreEqual(1, 2, nodeResults[0].Power, 0.00001);
            ComplexAssert.AreEqual(3, 4, nodeResults[1].Power, 0.00001);
            ComplexAssert.AreEqual(5, 6, nodeResults[2].Power, 0.00001);
        }

        [TestMethod]
        public void CalculateNodeResults_AllTypesOfBuses_NodeVoltageCalculatorGotCorrectCall()
        {
            var admittanceMatrixReduced = new Mock<IReadOnlyAdmittanceMatrix>();
            Vector<Complex> totalAdmittanceRowSums = new SparseVector(3);
            Vector<Complex> constantCurrents = new SparseVector(2);
            _admittanceMatrixMock.Setup(
                x =>
                    x.CreateReducedAdmittanceMatrix(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<int>>(),
                        It.IsAny<Vector<Complex>>(), out constantCurrents))
                .Returns(admittanceMatrixReduced.Object);
            _admittanceMatrixMock.Setup(x => x.CalculateRowSums()).Returns(totalAdmittanceRowSums);
            _nodeVoltageCalculatorMock.Setup(x => x.CalculateUnknownVoltages(It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>())).Returns(new SparseVector(2));
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new PqNode(new Complex(3, 4)));
            _powerNet.AddNode(new PvNode(5, 6));

            _powerNet.CalculateNodeResults(out _relativePowerError);

            _nodeVoltageCalculatorMock.Verify(
                x =>
                    x.CalculateUnknownVoltages(admittanceMatrixReduced.Object, totalAdmittanceRowSums, 5,
                        It.Is<Vector<Complex>>(
                            initialVoltages =>
                                initialVoltages.All(
                                    initalVoltage => (initalVoltage - new Complex(5, 0)).MagnitudeSquared() < 0.000001)),
                        constantCurrents, 
                        It.Is<IList<PqNodeWithIndex>>(pqBuses => pqBuses.Count == 1 && (pqBuses.First().Power - new Complex(3, 4)).Magnitude < 0.000001), 
                        It.Is<IList<PvNodeWithIndex>>(pvBuses => pvBuses.Count == 1 && Math.Abs(pvBuses.First().RealPower - 5) < 0.00001 && Math.Abs(pvBuses.First().VoltageMagnitude - 6) < 0.00001)), 
                        Times.Once);
            _nodeVoltageCalculatorMock.Verify(
                x =>
                    x.CalculateUnknownVoltages(It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>()), Times.Once);
        }

        [TestMethod]
        public void CalculateNodeResults_NoLoad_NoVoltageCollapse()
        {
            var admittanceMatrixReduced = new Mock<IReadOnlyAdmittanceMatrix>();
            Vector<Complex> totalAdmittanceRowSums = new SparseVector(3);
            Vector<Complex> constantCurrents = new SparseVector(2);
            _admittanceMatrixMock.Setup(
                x =>
                    x.CreateReducedAdmittanceMatrix(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<int>>(),
                        It.IsAny<Vector<Complex>>(), out constantCurrents))
                .Returns(admittanceMatrixReduced.Object);
            _admittanceMatrixMock.Setup(x => x.CalculateRowSums()).Returns(totalAdmittanceRowSums);
            _admittanceMatrixMock.Setup(x => x.CalculateCurrents(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(100);
            _nodeVoltageCalculatorMock.Setup(x => x.CalculateUnknownVoltages(It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>())).Returns(new SparseVector(2));
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new PqNode(new Complex()));
            _powerNet.AddNode(new PvNode(0, 6));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeResults_TwoSlackNodesAndOnePqNode_NoVoltageCollapse()
        {
            var admittanceMatrixReduced = new Mock<IReadOnlyAdmittanceMatrix>();
            Vector<Complex> totalAdmittanceRowSums = new SparseVector(3);
            Vector<Complex> constantCurrents = new SparseVector(1);
            _admittanceMatrixMock.Setup(
                x =>
                    x.CreateReducedAdmittanceMatrix(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<int>>(),
                        It.IsAny<Vector<Complex>>(), out constantCurrents))
                .Returns(admittanceMatrixReduced.Object);
            _admittanceMatrixMock.Setup(x => x.CalculateRowSums()).Returns(totalAdmittanceRowSums);
            _admittanceMatrixMock.Setup(x => x.CalculateCurrents(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _nodeVoltageCalculatorMock.Setup(x => x.MaximumRelativePowerError).Returns(100);
            _nodeVoltageCalculatorMock.Setup(x => x.CalculateUnknownVoltages(It.IsAny<IReadOnlyAdmittanceMatrix>(), It.IsAny<IList<Complex>>(),
                        It.IsAny<double>(), It.IsAny<Vector<Complex>>(), It.IsAny<Vector<Complex>>(),
                        It.IsAny<IList<PqNodeWithIndex>>(), It.IsAny<IList<PvNodeWithIndex>>())).Returns(new SparseVector(1));
            _admittanceMatrixMock.Setup(x => x.CalculateAllPowers(It.IsAny<Vector<Complex>>()))
                .Returns<Vector<Complex>>(voltages => new SparseVector(voltages.Count));
            _powerNet.AddNode(new SlackNode(new Complex(1, 2)));
            _powerNet.AddNode(new PqNode(new Complex(3, 4)));
            _powerNet.AddNode(new SlackNode(new Complex(5, 6)));

            var nodeResults = _powerNet.CalculateNodeResults(out _relativePowerError);

            Assert.IsNotNull(nodeResults); 
        }
    }
}
