using System.Runtime.InteropServices;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    class HolomorphicEmbeddedLoadFlowMethodTestNativeMethods
    {
        [DllImport("HELM.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RunTests();
    }
}