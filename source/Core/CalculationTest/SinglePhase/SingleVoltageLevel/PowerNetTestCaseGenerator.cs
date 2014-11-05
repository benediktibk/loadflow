using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    public class PowerNetTestCaseGenerator
    {
        private static AdmittanceMatrix _admittances;
        private static Vector<Complex> _voltages;
        private static Vector<Complex> _powers;
        private static double _nominalVoltage;

        public static PowerNetTestCase CreateTestWithUnderdeterminedProblem(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,] {   {new Complex(2, -1),    new Complex(-2, 1)},
                            {new Complex(-2, 1), new Complex(2, -1)}}));
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, admittances, 1);
            powerNet.AddNode(new PqNode(new Complex(0.5, -1)));
            return new PowerNetTestCase(powerNet, new DenseVector(2), new DenseVector(2));
        }

        public static PowerNetTestCase CreateTestWithNoSlackBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, 1);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new PqNode(_powers.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithGroundNodeVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithGroundNodeVersionThree(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemAndOnlyVoltagesGiven(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new SlackNode(_voltages.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemAndVoltagesAndPowersGiven(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithGroundNode(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new SlackNode(_voltages.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithMostlyImaginaryConnections(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            powerNet.AddNode(new PqNode(_powers.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new PvNode(_powers.At(2).Real, _voltages.At(2).Magnitude));
            powerNet.AddNode(new SlackNode(_voltages.At(3)));
            powerNet.AddNode(new SlackNode(_voltages.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFiveNodeProblemWithTwoPVBusses(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateFiveNodeProblem(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            powerNet.AddNode(new PvNode(_powers.At(2).Real, _voltages.At(2).Magnitude));
            powerNet.AddNode(new PvNode(_powers.At(3).Real, _voltages.At(3).Magnitude));
            powerNet.AddNode(new PqNode(_powers.At(4)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeSystemWithImaginaryConnectionsAndOnePVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithImaginaryConnections(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemAndTwoVoltagesGiven(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithGroundNode(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithMostlyImaginaryConnections(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new SlackNode(_voltages.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithTwoPVBuses(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithMostlyImaginaryConnections(out _admittances, out _voltages, out _powers,
                out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PvNode(_powers.At(2).Real, _voltages.At(2).Magnitude));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithOnePVAndOnePQBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateAsymmetricThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateAsymmetricThreeNodeProblem(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PvNode(_powers.At(2).Real, _voltages.At(2).Magnitude));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithDecoupledPQAndPVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithTwoDecoupledNodes(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateThreeNodeProblemWithRealValues(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            powerNet.AddNode(new PqNode(_powers.At(2)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestCollapsingSystem(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateCollapsingOneSideSuppliedConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestNearlyCollapsingSystem(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateNearlyCollapsingOneSideSuppliedConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFromOneSideSuppliedConnectionWithBigResistance(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.1, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFromOneSideSuppliedConnectionWithSmallResistance(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestFromOneSideSuppliedAndInverseInformationGiven(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new PqNode(_powers.At(0)));
            powerNet.AddNode(new SlackNode(_voltages.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestTwoNodeProblemWithOnePVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedConnection(0.001, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestTwoNodesWithImaginaryConnection(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedImaginaryConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestTwoNodesWithImaginaryConnectionWithPVBus(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedImaginaryConnection(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedImaginaryConnectionVersionTwo(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PvNode(_powers.At(1).Real, _voltages.At(1).Magnitude));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        public static PowerNetTestCase CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo(INodeVoltageCalculator nodeVoltageCalculator)
        {
            CreateOneSideSuppliedImaginaryConnectionVersionTwo(out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, _admittances, _nominalVoltage);
            powerNet.AddNode(new SlackNode(_voltages.At(0)));
            powerNet.AddNode(new PqNode(_powers.At(1)));
            return new PowerNetTestCase(powerNet, _voltages, _powers);
        }

        private static void CreateOneSideSuppliedConnection(double R, out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            var Y = 1.0 / R;
            var admittancesArray = new[,] { { new Complex(Y, 0), new Complex((-1) * Y, 0) }, { new Complex((-1) * Y, 0), new Complex(Y, 0) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex((1 + Math.Sqrt(1 - 4 * R)) / 2, 0);
            var voltageDifference = 1 - outputVoltage.Real;
            var inputPower = new Complex(1 + voltageDifference * voltageDifference / R, 0);
            var outputPower = new Complex(-1, 0);
            voltages = new DenseVector(new[] { inputVoltage, outputVoltage });
            powers = new DenseVector(new[] { inputPower, outputPower });
            nominalVoltage = 1;
        }

        private static void CreateOneSideSuppliedImaginaryConnection(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            const double Y = 100;
            var admittancesArray = new[,] { { new Complex(0, (-1) * Y), new Complex(0, Y) }, { new Complex(0, Y), new Complex(0, (-1) * Y) } };
            admittances = new AdmittanceMatrix(DenseMatrix.OfArray(admittancesArray));

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex((0.5 + Math.Sqrt(0.25 - 1 / Y)), 0);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateAsymmetricThreeNodeProblem(out AdmittanceMatrix admittances,
            out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(100, 200), new Complex(50, -100), new Complex(200, 600));
            admittances.AddUnsymmetricAdmittance(1, 2, new Complex(4, 1));
            admittances.AddUnsymmetricAdmittance(2, 1, new Complex(-4, -1));

            voltages = new DenseVector(new[] { new Complex(1.1, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithGroundNode(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1000, 500), new Complex(0, 0), new Complex(10, -60));

            voltages = new DenseVector(new[] { new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0, 0) });
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }

        private static void CreateThreeNodeProblemWithMostlyImaginaryConnections(out AdmittanceMatrix admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(5, 500), new Complex(20, -300), new Complex(10, 1000));

            voltages = new DenseVector(new[] { new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0.95, 0.05) });
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
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
            powers = AdmittanceMatrix.CalculateAllPowers(admittances, voltages);
            nominalVoltage = 1;
        }
    }
}
