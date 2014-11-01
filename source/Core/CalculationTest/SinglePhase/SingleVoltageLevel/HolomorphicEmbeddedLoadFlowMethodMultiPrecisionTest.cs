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
        private int _coefficientCount;
        private int _bitPrecision;

        protected override HolomorphicEmbeddedLoadFlowMethod CreateHELMNodeVoltageCalculator()
        {
            return new HolomorphicEmbeddedLoadFlowMethod(0.00001, _coefficientCount, new PrecisionMulti(_bitPrecision));
        }

        [TestInitialize]
        public void SetUp()
        {
            _coefficientCount = 100;
            _bitPrecision = 300;
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndVoltagesAndPowersGivenVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionOne();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyOneVoltageGivenVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemAndTwoVoltagesGivenVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNodeVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_CollapsingSystem_VoltageCollapse()
        {
            var powerNet = CreateTestCollapsingSystem();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNull(nodeResults);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_NearlyCollapsingSystem_NoVoltageCollapse()
        {
            var powerNet = CreateTestNearlyCollapsingSystem();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionThree_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNodeVersionThree();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithBigResistance_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionWithBigResistance();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionWithSmallResistance_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionWithSmallResistance();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnectionAndOnlyVoltagesKnown_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedConnectionAndOnlyVoltagesKnown();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedAndInverseInformationGiven_CorrectResults()
        {
            var powerNet = CreateTestFromOneSideSuppliedAndInverseInformationGiven();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndOnlyVoltagesGiven();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGiven_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemAndVoltagesAndPowersGiven();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGiven_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemAndTwoVoltagesGiven();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNode_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithGroundNode();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithMostlyImaginaryConnections_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithMostlyImaginaryConnections();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.2);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_TwoNodeProblemWithOnePVBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodeProblemWithOnePVBus();

            var nodeResults = powerNet.CalculateNodeResults();

            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 3);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithTwoPVBuses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithTwoPVBuses();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithSlackBusAtTheEndAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestFiveNodeProblemWithSlackBusAtTheEndAndPVBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.001, 1);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnection();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPVBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectResults()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPQBusVersionTwo_CoefficientsCorrect()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPQBusVersionTwo();
            var nodeVoltageCalculator = powerNet.NodeVoltageCalculator as HolomorphicEmbeddedLoadFlowMethod;

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeVoltageCalculator);
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
            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
            nodeVoltageCalculator.Dispose();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectCoefficients()
        {
            var powerNet = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();
            var nodeVoltageCalculator = powerNet.NodeVoltageCalculator as HolomorphicEmbeddedLoadFlowMethod;

            powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeVoltageCalculator);
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
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPQBusses_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPQBusses();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithDecoupledPQAndPVBus_CorrectResults()
        {
            var powerNet = CreateTestThreeNodeProblemWithDecoupledPQAndPVBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemWithOnePVBusAndOnePQBus_CorrectResults()
        {
            _coefficientCount = 150;
            _bitPrecision = 400;
            var powerNet = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.02, 0.2);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithPQAndPVBus_CorrectResults()
        {
            _coefficientCount = 200;
            _bitPrecision = 400;
            var powerNet = CreateTestThreeNodeProblemWithOnePVBusAndOnePQBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0002, 0.05);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithAsymmetricAdmittancesAndPVBusses_CorrectResults()
        {
            _coefficientCount = 200;
            _bitPrecision = 400;
            var powerNet = CreateTestThreeNodeProblemWithAsymmetricAdmittancesAndTwoPVBusses();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }

        [TestMethod]
        [TestCategory("Unit, slow")]
        public void CalculateNodeVoltagesAndPowers_ThreeNodesWithRealValuesAndPQAndPVBus_CorrectResults()
        {
            _coefficientCount = 200;
            _bitPrecision = 500;
            var powerNet = CreateTestThreeNodeProblemWithRealValuesAndOnePQAndPVBus();

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            NodeAssert.AreEqual(nodeResults, _voltages, _powers, 0.0001, 0.01);
        }
    }
}
