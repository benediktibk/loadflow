using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodMultiPrecisionTest : HolomorphicEmbeddedLoadFlowMethodTest
    {
        protected override HolomorphicEmbeddedLoadFlowMethod CreateHELMNodeVoltageCalculator()
        {
            return new HolomorphicEmbeddedLoadFlowMethod(0.00001, 100, new PrecisionMulti(300));
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var nodes = CreateTestCollapsingSystem();

            _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var nodes = CreateTestNearlyCollapsingSystem();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNodeVersionThree();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionWithSmallResistance();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var nodes = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemAndVoltagesAndPowersGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.2);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodeProblemWithOnePVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 3);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            var nodeVoltageCalculator =  new HolomorphicEmbeddedLoadFlowMethod(0.000000001, 150, new PrecisionMulti(400));
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            nodeVoltageCalculator.Dispose();
            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 0.2);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithTwoPVBuses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var nodes = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CoefficientsCorrect()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();
            var nodeVoltageCalculator = CreateHELMNodeVoltageCalculator();
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);

            var nodeResults = loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            var firstCoefficientShouldBe = new DenseVector(new[] { new Complex(1.05, 0) });
            var secondCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0289649928938644, -0.019047619047619) } );
            var thirdCoefficientShouldBe = new DenseVector(new[] { new Complex(-0.0011445548616427, 1.14628982339312E-100) });
            var firstInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.952380952380952, 0) });
            var secondInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.0262721023980629, 0.0172767519706295) });
            var thirdInverseCoefficientShouldBe = new DenseVector(new[] { new Complex(0.00144946906527004, 0.00095318285344446) });
            var firstCoefficient = nodeVoltageCalculator.GetCoefficients(0);
            var secondCoefficient = nodeVoltageCalculator.GetCoefficients(1);
            var thirdCoefficient = nodeVoltageCalculator.GetCoefficients(2);
            var firstInverseCoefficient = nodeVoltageCalculator.GetInverseCoefficients(0);
            var secondInverseCoefficient = nodeVoltageCalculator.GetInverseCoefficients(1);
            var thirdInverseCoefficient = nodeVoltageCalculator.GetInverseCoefficients(2);
            ComplexAssert.AreAllEqual(firstCoefficientShouldBe, firstCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(firstInverseCoefficientShouldBe, firstInverseCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(secondCoefficientShouldBe, secondCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(secondInverseCoefficientShouldBe, secondInverseCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(thirdCoefficientShouldBe, thirdCoefficient, 0.0001);
            ComplexAssert.AreAllEqual(thirdInverseCoefficientShouldBe, thirdInverseCoefficient, 0.0001);
            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
            nodeVoltageCalculator.Dispose();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectCoefficients()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();
            var nodeVoltageCalculator = CreateHELMNodeVoltageCalculator();
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);

            loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Complex a;
            Complex b;
            Complex c;
            CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out a, out b, out c);
            var firstCoefficient = nodeVoltageCalculator.GetCoefficients(0)[0];
            var secondCoefficient = nodeVoltageCalculator.GetCoefficients(1)[0];
            var thirdCoefficient = nodeVoltageCalculator.GetCoefficients(2)[0];
            ComplexAssert.AreEqual(a, firstCoefficient, 0.00001);
            ComplexAssert.AreEqual(b, secondCoefficient, 0.00001);
            ComplexAssert.AreEqual(c, thirdCoefficient, 0.00001);
            nodeVoltageCalculator.Dispose();
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00001, 200, new PrecisionMulti(400));
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);
            var nodes = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00001, 200, new PrecisionMulti(400));
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);
            var nodes = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var nodes = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = _calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            var nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethod(0.00001, 200, new PrecisionMulti(500));
            var loadFlowCalculator = new LoadFlowCalculator(nodeVoltageCalculator);
            var nodes = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            var nodeResults = loadFlowCalculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes.Cast<IReadOnlyNode>().ToList(), out _voltageCollapse);

            Assert.IsFalse(_voltageCollapse);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
    }
}
