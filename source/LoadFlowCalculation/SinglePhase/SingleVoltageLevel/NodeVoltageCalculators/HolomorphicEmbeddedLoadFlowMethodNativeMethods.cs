using System.Runtime.InteropServices;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    class HolomorphicEmbeddedLoadFlowMethodNativeMethods
    {
        public delegate void StringCallback(string text);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateLoadFlowCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, [MarshalAs(UnmanagedType.I1)]bool calculatePartialResults);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateLoadFlowCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision, [MarshalAs(UnmanagedType.I1)]bool calculatePartialResults);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DeleteLoadFlowCalculator(int calculator);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAdmittance(int calculator, int row, int column, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAdmittanceRowSum(int calculator, int row, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetConstantCurrent(int calculator, int node, double real, double imaginary);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Calculate(int calculator);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetVoltageReal(int calculator, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetVoltageImaginary(int calculator, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double SetConsoleOutput(int calculator, StringCallback function);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetCoefficientReal(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetCoefficientImaginary(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInverseCoefficientReal(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInverseCoefficientImaginary(int calculator, int step, int node);

        [DllImport("LoadFlowCalculationHELM.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetLastNodeCount(int calculator);
    }
}