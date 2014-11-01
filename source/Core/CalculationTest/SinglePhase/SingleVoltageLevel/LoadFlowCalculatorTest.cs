using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    abstract public class LoadFlowCalculatorTest
    {
        protected PowerNetComputable _calculator;
        protected AdmittanceMatrix _admittances;
        protected Vector<Complex> _voltages;
        protected Vector<Complex> _powers;
        protected double _nominalVoltage;
        protected bool _voltageCollapse;

        abstract protected INodeVoltageCalculator CreateNodeVoltageCalculator();

        #region basic tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltagesAndPowers_OverdeterminedProblem_ExceptionThrown()
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new [,] {   {new Complex(2, -1),    new Complex(-2, 1)},
                            {new Complex(-2, 1), new Complex(2, -1)}}));
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), admittances, 1);
            powerNet.SetNode(0, new Node() { Power = new Complex(-1, 2), Voltage = new Complex(1, 2) });
            powerNet.SetNode(1, new Node() { Power = new Complex(0.5, -1) });

            powerNet.CalculateNodeResults();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CalculateNodeVoltagesAndPowers_UnderdeterminedProblem_ExceptionThrown()
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,] {   {new Complex(2, -1),    new Complex(-2, 1)},
                            {new Complex(-2, 1), new Complex(2, -1)}}));
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), admittances, 1);
            powerNet.SetNode(1, new Node() { Power = new Complex(0.5, -1) });

            powerNet.CalculateNodeResults();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CalculateNodeVoltagesAndPowers_OnlyPowersKnown_ThrowsException()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, 1);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Power = _powers.At(1) });

            powerNet.CalculateNodeResults();
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

        #endregion

        #region test creation
        #region five nodes
        protected PowerNetComputable CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() {Power = _powers.At(4)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithGroundNodeVersionTwo()
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithGroundNodeVersionThree()
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemAndOnlyVoltagesGiven()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() { Voltage = _voltages.At(3) });
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemAndVoltagesAndPowersGiven()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithGroundNode()
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() { Voltage = _voltages.At(3) });
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithMostlyImaginaryConnections()
        {
            CreateFiveNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            powerNet.SetNode(3, new Node() {Power = _powers.At(3)});
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus()
        {
            CreateFiveNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { Power = _powers.At(1) });
            powerNet.SetNode(2, new Node() { VoltageMagnitude = _voltages.At(2).Magnitude, RealPower = _powers.At(2).Real});
            powerNet.SetNode(3, new Node() { Voltage = _voltages.At(3) });
            powerNet.SetNode(4, new Node() { Voltage = _voltages.At(4) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFiveNodeProblemWithTwoPVBusses()
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            powerNet.SetNode(2, new Node() { VoltageMagnitude = _voltages.At(2).Magnitude, RealPower = _powers.At(2).Real });
            powerNet.SetNode(3, new Node() { VoltageMagnitude = _voltages.At(3).Magnitude, RealPower = _powers.At(3).Real });
            powerNet.SetNode(4, new Node() {Power = _powers.At(4)});
            return powerNet;
        }
        #endregion

        #region three nodes
        protected PowerNetComputable CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus()
        {
            CreateThreeNodeProblemWithImaginaryConnections(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Power = _powers.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo()
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemAndTwoVoltagesGiven()
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithMostlyImaginaryConnections()
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() { Voltage = _voltages.At(2) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus()
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithTwoPVBuses()
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() { VoltageMagnitude = _voltages.At(2).Magnitude, RealPower = _powers.At(2).Real });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithOnePVAndOnePQBus()
        {
            CreateThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() { Power = _powers.At(2) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses()
        {
            CreateAsymmetricThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses()
        {
            CreateAsymmetricThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() { VoltageMagnitude = _voltages.At(2).Magnitude, RealPower = _powers.At(2).Real });
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithDecoupledPQAndPVBus()
        {
            CreateThreeNodeProblemWithTwoDecoupledNodes(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus()
        {
            CreateThreeNodeProblemWithRealValues(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            powerNet.SetNode(2, new Node() {Power = _powers.At(2)});
            return powerNet;
        }
        #endregion

        #region collapse
        protected PowerNetComputable CreateTestCollapsingSystem()
        {
            CreateCollapsingOneSideSuppliedConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            return powerNet;
        }

         protected PowerNetComputable CreateTestNearlyCollapsingSystem()
        {
            CreateNearlyCollapsingOneSideSuppliedConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            return powerNet;
        }
        #endregion

        #region from one side supplied connection
        protected PowerNetComputable CreateTestFromOneSideSuppliedConnectionWithBigResistance()
        {
            CreateOneSideSuppliedConnection(0.1, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { Power = _powers.At(1) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFromOneSideSuppliedConnectionWithSmallResistance()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestFromOneSideSuppliedAndInverseInformationGiven()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() {Power = _powers.At(0)});
            powerNet.SetNode(1, new Node() { Voltage = _voltages.At(1) });
            return powerNet;
        }

        protected PowerNetComputable CreateTestTwoNodeProblemWithOnePVBus()
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            return powerNet;
        }

        protected PowerNetComputable CreateTestTwoNodesWithImaginaryConnection()
        {
            CreateOneSideSuppliedImaginaryConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            return powerNet;
        }

        protected PowerNetComputable CreateTestTwoNodesWithImaginaryConnectionWithPVBus()
        {
            CreateOneSideSuppliedImaginaryConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            return powerNet;
        }

        protected PowerNetComputable CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo()
        {
            CreateOneSideSuppliedImaginaryConnectionVersionTwo(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() { VoltageMagnitude = _voltages.At(1).Magnitude, RealPower = _powers.At(1).Real });
            return powerNet;
        }

        protected PowerNetComputable CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo()
        {
            CreateOneSideSuppliedImaginaryConnectionVersionTwo(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(CreateNodeVoltageCalculator(), _admittances, _nominalVoltage);
            powerNet.SetNode(0, new Node() { Voltage = _voltages.At(0) });
            powerNet.SetNode(1, new Node() {Power = _powers.At(1)});
            return powerNet;
        }
        #endregion
        #endregion

        #region system creation
        private static void CreateOneSideSuppliedConnection(double R, out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            var Y = 1.0 / R;
            var admittancesArray = new[,]{ { new Complex(Y, 0), new Complex((-1) * Y, 0) }, { new Complex((-1) * Y, 0), new Complex(Y, 0) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex((1 + Math.Sqrt(1 - 4*R))/2, 0);
            var voltageDifference = 1 - outputVoltage.Real;
            var inputPower = new Complex(1 + voltageDifference*voltageDifference/R, 0);
            var outputPower = new Complex(-1, 0);
            voltages = new DenseVector(new []{inputVoltage, outputVoltage});
            powers = new DenseVector(new []{inputPower, outputPower});
            nominalVoltage = 1;
        }

        private static void CreateOneSideSuppliedImaginaryConnection(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            const double Y = 100;
            var admittancesArray = new[,] { { new Complex(0, (-1) * Y), new Complex(0, Y) }, { new Complex(0, Y), new Complex(0, (-1) * Y) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex((0.5 + Math.Sqrt(0.25 - 1/Y)), 0);
            var voltageDifference = 1 - outputVoltage.Real;
            var inputPower = new Complex(0, 1 + voltageDifference * voltageDifference * Y);
            var outputPower = new Complex(0, -1);
            voltages = new DenseVector(new[] { inputVoltage, outputVoltage });
            powers = new DenseVector(new[] { inputPower, outputPower });
            nominalVoltage = 1;
        }

        private static void CreateOneSideSuppliedImaginaryConnectionVersionTwo(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            var X = new Complex(0, 0.02);
            var admittancesArray = new[,] { { 1 / X, -1 / X }, { -1 / X, 1 / X } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = Complex.FromPolarCoordinates(1.05, 0);
            var outputVoltage = new Complex(1.019822135751, -0.0190476096206661);
            var inputPower = new Complex(1, 1.5843378730725);
            var outputPower = new Complex(-1, -1.52066212692788);
            voltages = new DenseVector(new[] { inputVoltage, outputVoltage });
            powers = new DenseVector(new[] { inputPower, outputPower });
            nominalVoltage = 1;
        }

        private static void CreateCollapsingOneSideSuppliedConnection(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            var admittancesArray = new[,] { { new Complex(1, 0), new Complex(-1, 0) }, { new Complex(-1, 0), new Complex(1, 0) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex();
            var inputPower = new Complex();
            var outputPower = new Complex(-10, 0);
            voltages = new DenseVector(new[] { inputVoltage, outputVoltage });
            powers = new DenseVector(new[] { inputPower, outputPower });
            nominalVoltage = 1;
        }

        private static void CreateNearlyCollapsingOneSideSuppliedConnection(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            var admittancesArray = new[,] { { new Complex(1, 0), new Complex(-1, 0) }, { new Complex(-1, 0), new Complex(1, 0) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex(0.6, 0);
            var inputPower = new Complex(0.4, 0);
            var outputPower = new Complex(-0.24, 0);
            voltages = new DenseVector(new[] { inputVoltage, outputVoltage });
            powers = new DenseVector(new[] { inputPower, outputPower });
            nominalVoltage = 1;
        }

        private static void CreateFiveNodeProblem(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateFiveNodeProblemAdmittanceMatrix(
                new Complex(1000, 500), new Complex(0, 0), new Complex(200, -100), new Complex(0, -200),
                new Complex(100, 300), new Complex(0, 0), new Complex(0, 0),
                new Complex(200, -500), new Complex(0, 0),
                new Complex(700, 500));

            voltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0.99, -0.12) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateFiveNodeProblemWithGroundNode(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateFiveNodeProblemAdmittanceMatrix(
                new Complex(1000, 500), new Complex(0, 0), new Complex(200, -100), new Complex(50, -20),
                new Complex(100, 300), new Complex(0, 0), new Complex(0, 0),
                new Complex(200, -500), new Complex(0, 0),
                new Complex(10, -5));

            voltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0, 0) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateFiveNodeProblemWithMostlyImaginaryConnections(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateFiveNodeProblemAdmittanceMatrix(
                new Complex(0, 500), new Complex(0, 0), new Complex(20, -100), new Complex(5, -200),
                new Complex(1, 300), new Complex(0, -100), new Complex(0, 0),
                new Complex(2, -500), new Complex(0, 0),
                new Complex(1, 1000));

            voltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.1), new Complex(0.97, -0.15), new Complex(1.01, -0.02) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static AdmittanceMatrix CreateFiveNodeProblemAdmittanceMatrix(Complex oneTwo, Complex oneThree,
            Complex oneFour, Complex oneFive,
            Complex twoThree, Complex twoFour, Complex twoFive, Complex threeFour, Complex threeFive, Complex fourFive)
        {
            return new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {
                    oneTwo + oneThree + oneFour + oneFive, (-1)*oneTwo, (-1)*oneThree, (-1)*oneFour, (-1)*oneFive
                },
                {
                    (-1)*oneTwo, oneTwo + twoThree + twoFour + twoFive, (-1)*twoThree, (-1)*twoFour, (-1)*twoFive
                },
                {
                    (-1)*oneThree, (-1)*twoThree, oneThree + twoThree + threeFour + threeFive, (-1)*threeFour, (-1)*threeFive
                },
                {
                    (-1)*oneFour, (-1)*twoFour, (-1)*threeFour, oneFour + twoFour + threeFour + fourFive, (-1)*fourFive
                },
                {
                    (-1)*oneFive, (-1)*twoFive, (-1)*threeFive, (-1)*fourFive, oneFive + twoFive + threeFive + fourFive
                }
            }));
        }

        private static void CreateThreeNodeProblemWithImaginaryConnections(out AdmittanceMatrix admittances,
            out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(0, 500), new Complex(0, 0), new Complex(0, -600));

            voltages = new DenseVector(new[] { new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0, 0) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateAsymmetricThreeNodeProblem(out AdmittanceMatrix admittances,
            out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(100, 200), new Complex(50, -100), new Complex(200, 600));
            admittances.AddUnsymmetricAdmittance(1, 2, new Complex(4, 1));
            admittances.AddUnsymmetricAdmittance(2, 1, new Complex(-4, -1));
            
            voltages = new DenseVector(new[] { new Complex(1.1, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithGroundNode(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1000, 500), new Complex(0, 0), new Complex(10, -60));

            voltages = new DenseVector(new []{new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0, 0)});
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithMostlyImaginaryConnections(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(5, 500), new Complex(20, -300), new Complex(10, 1000));

            voltages = new DenseVector(new[] { new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static AdmittanceMatrix CreateThreeNodeProblemAdmittanceMatrix(Complex oneTwo, Complex oneThree,
            Complex twoThree)
        {
            return new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {oneTwo + oneThree, (-1)*oneTwo, (-1)*oneThree},
                {(-1)*oneTwo, oneTwo + twoThree, (-1)*twoThree},
                {(-1)*oneThree, (-1)*twoThree, oneThree + twoThree}
            }));
        }

        private static void CreateThreeNodeProblem(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1000, 500), new Complex(200, -200),
                new Complex(100, 300));
            voltages = new DenseVector(3);
            voltages[0] = new Complex(1, 0.2);
            voltages[1] = new Complex(1.1, -0.1);
            voltages[2] = new Complex(0.8, 0);
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithTwoDecoupledNodes(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1000, 500), new Complex(200, -300),
                new Complex(0, 0));
            voltages = new DenseVector(3);
            voltages[0] = new Complex(1, 0.1);
            voltages[1] = new Complex(0.95, 0.08);
            voltages[2] = new Complex(0.9, 0.12);
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithRealValues(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1, 0), new Complex(1, 0),
                new Complex(1, 0));
            voltages = new DenseVector(3);
            voltages[0] = new Complex(1, 0);
            voltages[1] = new Complex(0.5, 0);
            voltages[2] = new Complex(0.5, 0);
            powers = PowerNetComputable.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }
        #endregion
    }
}
