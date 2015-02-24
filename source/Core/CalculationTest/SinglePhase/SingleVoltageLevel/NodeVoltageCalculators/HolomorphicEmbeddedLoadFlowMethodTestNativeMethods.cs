using System.Runtime.InteropServices;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    class HolomorphicEmbeddedLoadFlowMethodTestNativeMethods
    {
        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsComplexDouble();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsComplexMultiPrecision();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsMultiPrecision();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsCoefficientStoragePQ();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsCoefficientStoragePV();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsCoefficientStorageMixed();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsAnalyticContinuationStepByStep();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsAnalyticContinuationBunchAtOnce();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsLinearEquationSystemOne();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsLinearEquationSystemTwo();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsLinearEquationSystemThree();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsLinearEquationSystemFour();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsLinearEquationSystemFive();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorConstructor();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorCopyConstructor();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorAssignment();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorDotProduct();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorSquaredNorm();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorWeightedSum();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorAddWeightedSum();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorPointwiseMultiply();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorSubtract();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorStreaming();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorConjugate();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsVectorMultiPrecision();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixConstructor();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixGet();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixSet();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixStreaming();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixMultiply();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixRowIteration();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixRowIterationWithStartColumn();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixFindAbsoluteMaximumOfColumn();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixCalculateBandwidth();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixChangeRows();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixAssignment();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixGetRowValuesAndColumns();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixCompress();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixAddWeightedRowElements();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsSparseMatrixMultiplyWithStartAndEndColumn();

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTestsGraphCalculateReverseCuthillMcKee();
    }
}