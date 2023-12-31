#include "CalculatorRegister.h"
#include "ConsoleOutput.h"
#include "UnitTest.h"
#include "MultiPrecision.h"
#include <Windows.h>

using namespace std;

CalculatorRegister calculatorRegister;

extern "C" __declspec(dllexport) int __cdecl CreateLoadFlowCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, bool iterativeSolver)
{
	return calculatorRegister.createCalculatorLongDouble(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, iterativeSolver);
}

extern "C" __declspec(dllexport) int __cdecl CreateLoadFlowCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision, bool iterativeSolver)
{
	return calculatorRegister.createCalculatorMultiPrecision(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, bitPrecision, iterativeSolver);
}

extern "C" __declspec(dllexport) void __cdecl DeleteLoadFlowCalculator(int calculator)
{
	calculatorRegister.remove(calculator);
}

extern "C" __declspec(dllexport) void __cdecl SetAdmittance(int calculator, int row, int column, double real, double imaginary)
{
	calculatorRegister.get(calculator).setAdmittance(row, column, Complex<long double>(real, imaginary));
}

extern "C" __declspec(dllexport) void __cdecl SetAdmittanceRowSum(int calculator, int row, double real, double imaginary)
{
	calculatorRegister.get(calculator).setAdmittanceRowSum(row, Complex<long double>(real, imaginary));
}

extern "C" __declspec(dllexport) void __cdecl SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary)
{
	calculatorRegister.get(calculator).setPQBus(busId, node, Complex<long double>(powerReal, powerImaginary));
}

extern "C" __declspec(dllexport) void __cdecl SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude)
{
	calculatorRegister.get(calculator).setPVBus(busId, node, powerReal, voltageMagnitude);
}

extern "C" __declspec(dllexport) void __cdecl SetConstantCurrent(int calculator, int node, double real, double imaginary)
{
	calculatorRegister.get(calculator).setConstantCurrent(node, Complex<long double>(real, imaginary));
}

extern "C" __declspec(dllexport) void __cdecl Calculate(int calculator)
{
	calculatorRegister.get(calculator).calculate();
}

extern "C" __declspec(dllexport) double __cdecl GetVoltageReal(int calculator, int node)
{
	return calculatorRegister.get(calculator).getVoltageReal(node);
}

extern "C" __declspec(dllexport) double __cdecl GetVoltageImaginary(int calculator, int node)
{
	return calculatorRegister.get(calculator).getVoltageImaginary(node);
}

extern "C" __declspec(dllexport) double __cdecl GetCoefficientReal(int calculator, int step, int node)
{
	return calculatorRegister.get(calculator).getCoefficientReal(step, node);
}

extern "C" __declspec(dllexport) double __cdecl GetCoefficientImaginary(int calculator, int step, int node)
{
	return calculatorRegister.get(calculator).getCoefficientImaginary(step, node);
}

extern "C" __declspec(dllexport) double __cdecl GetInverseCoefficientReal(int calculator, int step, int node)
{
	return calculatorRegister.get(calculator).getInverseCoefficientReal(step, node);
}

extern "C" __declspec(dllexport) double __cdecl GetInverseCoefficientImaginary(int calculator, int step, int node)
{
	return calculatorRegister.get(calculator).getInverseCoefficientImaginary(step, node);
}

extern "C" __declspec(dllexport) int __cdecl GetLastNodeCount(int calculator)
{
	return calculatorRegister.get(calculator).getNodeCount();
}

extern "C" __declspec(dllexport) double __cdecl GetProgress(int calculator)
{
	return calculatorRegister.get(calculator).getProgress();
}

extern "C" __declspec(dllexport) double __cdecl GetRelativePowerError(int calculator)
{
	return calculatorRegister.get(calculator).getRelativePowerError();
}

extern "C" __declspec(dllexport) int __cdecl GetMaximumPossibleCoefficientCount(int calculator)
{
	return calculatorRegister.get(calculator).getMaximumPossibleCoefficientCount();
}

extern "C" { int _afxForceUSRDLL; }

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  ul_reason_for_call, 
                      LPVOID lpReserved)
{
    switch(ul_reason_for_call) 
	{
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
		MultiPrecision::setDefaultPrecision();
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
		break;
    }

    return TRUE;
}
