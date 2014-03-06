using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodTest : JacobiMatrixBaseMethodTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.0001, 10000);
        }

        [TestMethod]
        public void CalculateVoltageChanges_OneConnection_correctResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });
            var powersReal = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -1 });
            var powersImaginary = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0 });
            var voltageAmplitude = voltages[0].Magnitude;
            var voltageAngle = voltages[0].Phase;
            var currentPowerReal = 10*voltageAmplitude*voltageAmplitude - 10*voltageAmplitude*Math.Cos(voltageAngle);
            var currentPowerImaginary = (-10)*voltageAmplitude*Math.Sin(voltageAngle);
            var powerRealDifference = powersReal -
                                      new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new[] { currentPowerReal });
            var powerImaginaryDifference = powersImaginary -
                                      new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new[] { currentPowerImaginary });
            var calculator = new FastDecoupledLoadFlowMethod(0.0001, 10000);
            var pvBuses = new List<PVBus>();
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex())
            };

            var voltageChange = calculator.CalculateVoltageChanges(admittances, voltages, constantCurrents,
                powerRealDifference, powerImaginaryDifference, pqBuses, pvBuses);

            var derivationRealPowerByAngle = 10*voltageAmplitude*Math.Sin(voltageAngle);
            var derivationImaginaryPowerByAmplitude = (-10)*Math.Sin(voltageAngle);
            var voltageChangeShouldBe =
                Complex.FromPolarCoordinates(powerImaginaryDifference[0]/derivationImaginaryPowerByAmplitude,
                    powerRealDifference[0]/derivationRealPowerByAngle);
            Assert.AreEqual(1, voltageChange.Count);
            ComplexAssert.AreEqual(voltageChangeShouldBe, voltageChange[0], 0.00001);
        }

        [TestMethod]
        public void CalculateVoltageChanges_TwoConnectionsAndCorrectVoltage_Zero()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });
            var powersRealDifference = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0, 0 });
            var powersImaginaryDifference = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0, 0 });
            var calculator = new FastDecoupledLoadFlowMethod(0.0001, 10000);
            var pvBuses = new List<PVBus>();
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex()),
                new PQBus(1, new Complex())
            };

            var voltageChange = calculator.CalculateVoltageChanges(admittances, voltages, constantCurrents,
                powersRealDifference, powersImaginaryDifference, pqBuses, pvBuses);

            Assert.AreEqual(2, voltageChange.Count);
            Assert.AreEqual(0, voltageChange[0].Magnitude, 0.0001);
            Assert.AreEqual(0, voltageChange[1].Magnitude, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.05, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var nodes = CreateTestCollapsingSystem();

            _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.0001);
        }
        
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.1, 100);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNode();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.01, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.001, 5);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodeProblemWithOnePVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithTwoPVBuses();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            nodes = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodes, _voltages, _powers, 0.0001, 0.01);
        }
    }
}
