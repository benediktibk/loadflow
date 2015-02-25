using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodTest
    {
        [TestMethod]
        public void ComplexDouble()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsComplexDouble());
        }

        [TestMethod]
        public void ComplexMultiPrecision()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsComplexMultiPrecision());
        }

        [TestMethod]
        public void MultiPrecision()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsMultiPrecision());
        }

        [TestMethod]
        public void CoefficientStoragePQ()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsCoefficientStoragePQ());
        }

        [TestMethod]
        public void CoefficientStoragePV()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsCoefficientStoragePV());
        }

        [TestMethod]
        public void CoefficientStorageMixed()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsCoefficientStorageMixed());
        }

        [TestMethod]
        public void AnalyticContinuationStepByStep()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsAnalyticContinuationStepByStep());
        }

        [TestMethod]
        public void AnalyticContinuationBunchAtOnce()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsAnalyticContinuationBunchAtOnce());
        }

        [TestMethod]
        public void LinearEquationSystemOne()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemOne());
        }

        [TestMethod]
        public void LinearEquationSystemTwo()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemTwo());
        }

        [TestMethod]
        public void LinearEquationSystemThree()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemThree());
        }

        [TestMethod]
        public void LinearEquationSystemFour()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemFour());
        }

        [TestMethod]
        public void LinearEquationSystemFive()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemFive());
        }

        [TestMethod]
        public void LinearEquationSystemSix()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemSix());
        }

        [TestMethod]
        public void LinearEquationSystemSeven()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsLinearEquationSystemSeven());
        }

        [TestMethod]
        public void VectorConstructor()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorConstructor());
        }

        [TestMethod]
        public void VectorCopyConstructor()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorCopyConstructor());
        }

        [TestMethod]
        public void VectorAssignment()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorAssignment());
        }

        [TestMethod]
        public void VectorDotProduct()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorDotProduct());
        }

        [TestMethod]
        public void VectorSquaredNorm()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorSquaredNorm());
        }

        [TestMethod]
        public void VectorWeightedSum()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorWeightedSum());
        }

        [TestMethod]
        public void VectorAddWeightedSum()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorAddWeightedSum());
        }

        [TestMethod]
        public void VectorPointwiseMultiply()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorPointwiseMultiply());
        }

        [TestMethod]
        public void VectorSubtract()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorSubtract());
        }

        [TestMethod]
        public void VectorStreaming()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorStreaming());
        }

        [TestMethod]
        public void VectorConjugate()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorConjugate());
        }

        [TestMethod]
        public void VectorMultiPrecision()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsVectorMultiPrecision());
        }

        [TestMethod]
        public void SparseMatrixConstructor()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixConstructor());
        }

        [TestMethod]
        public void SparseMatrixGet()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixGet());
        }

        [TestMethod]
        public void SparseMatrixSet()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixSet());
        }

        [TestMethod]
        public void SparseMatrixStreaming()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixStreaming());
        }

        [TestMethod]
        public void SparseMatrixMultiply()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixMultiply());
        }

        [TestMethod]
        public void SparseMatrixRowIteration()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixRowIteration());
        }

        [TestMethod]
        public void SparseMatrixRowIterationWithStartColumn()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixRowIterationWithStartColumn());
        }

        [TestMethod]
        public void SparseMatrixFindAbsoluteMaximumOfColumn()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixFindAbsoluteMaximumOfColumn());
        }

        [TestMethod]
        public void SparseMatrixCalculateBandwidth()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixCalculateBandwidth());
        }

        [TestMethod]
        public void SparseMatrixChangeRows()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixChangeRows());
        }

        [TestMethod]
        public void SparseMatrixAssignment()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixAssignment());
        }

        [TestMethod]
        public void SparseMatrixGetRowValuesAndColumns()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixGetRowValuesAndColumns());
        }

        [TestMethod]
        public void SparseMatrixCompress()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixCompress());
        }

        [TestMethod]
        public void SparseMatrixTranspose()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixTranspose());
        }

        [TestMethod]
        public void SparseMatrixPermutateRows()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixPermutateRows());
        }

        [TestMethod]
        public void SparseMatrixPermutateColumns()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixPermutateColumns());
        }

        [TestMethod]
        public void SparseMatrixReduceBandwidth()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixReduceBandwidth());
        }

        [TestMethod]
        public void SparseMatrixAddWeightedRowElements()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixAddWeightedRowElements());
        }

        [TestMethod]
        public void SparseMatrixMultiplyWithStartAndEndColumn()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsSparseMatrixMultiplyWithStartAndEndColumn());
        }

        [TestMethod]
        public void GraphCalculateReverseCuthillMcKee()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsGraphCalculateReverseCuthillMcKee());
        }

        [TestMethod]
        public void GraphCreateLayeringFrom()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsGraphCreateLayeringFrom());
        }

        [TestMethod]
        public void GraphFindPseudoPeriphereNode()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsGraphFindPseudoPeriphereNode());
        }

        [TestMethod]
        public void GraphFindPseudoPeriphereNodeOfAdmittanceMatrix()
        {
            Assert.IsTrue(HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTestsGraphFindPseudoPeriphereNodeOfAdmittanceMatrix());
        }

        public static void CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out Complex a,
            out Complex b, out Complex c)
        {
            a = new Complex(1.05, 0);
            b = new Complex(-0.062673010380623, -0.0403690888119954);
            c = new Complex(0.0686026762176026, 0.0475978097324825);
        }
    }
}
