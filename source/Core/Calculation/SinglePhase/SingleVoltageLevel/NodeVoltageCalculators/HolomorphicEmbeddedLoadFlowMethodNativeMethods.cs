using System.Runtime.InteropServices;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    static class HolomorphicEmbeddedLoadFlowMethodNativeMethods
    {
        public delegate void StringCallback(string text);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateLoadFlowCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateLoadFlowCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteLoadFlowCalculator(int calculator);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAdmittance(int calculator, int row, int column, double real, double imaginary);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAdmittanceRowSum(int calculator, int row, double real, double imaginary);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetConstantCurrent(int calculator, int node, double real, double imaginary);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Calculate(int calculator);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetVoltageReal(int calculator, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetVoltageImaginary(int calculator, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double SetConsoleOutput(int calculator, StringCallback function);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetCoefficientReal(int calculator, int step, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetCoefficientImaginary(int calculator, int step, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInverseCoefficientReal(int calculator, int step, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInverseCoefficientImaginary(int calculator, int step, int node);

        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetLastNodeCount(int calculator);
    }
}