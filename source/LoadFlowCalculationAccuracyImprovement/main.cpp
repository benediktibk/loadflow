#include "CalculatorRegister.h"
#include "ConsoleOutput.h"

using namespace std;

CalculatorRegister calculatorRegister;
ConsoleOutput consoleOutput = 0;

extern "C" __declspec(dllexport) int __cdecl CreateLoadFlowCalculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount)
{
	return calculatorRegister.create(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount);
}

extern "C" __declspec(dllexport) void __cdecl DeleteLoadFlowCalculator(int calculator)
{
	calculatorRegister.remove(calculator);
}

extern "C" __declspec(dllexport) void __cdecl SetAdmittance(int calculator, int row, int column, double real, double imaginary)
{
	calculatorRegister.get(calculator)->setAdmittance(row, column, complex<double>(real, imaginary));
}

extern "C" __declspec(dllexport) void __cdecl SetPQBus(int calculator, int busId, int node, double powerReal, double powerImaginary)
{
	calculatorRegister.get(calculator)->setPQBus(busId, node, complex<double>(powerReal, powerImaginary));
}

extern "C" __declspec(dllexport) void __cdecl SetPVBus(int calculator, int busId, int node, double powerReal, double voltageMagnitude)
{
	calculatorRegister.get(calculator)->setPVBus(busId, node, powerReal, voltageMagnitude);
}

extern "C" __declspec(dllexport) void __cdecl SetConstantCurrent(int calculator, int node, double real, double imaginary)
{
	calculatorRegister.get(calculator)->setConstantCurrent(node, complex<double>(real, imaginary));
}

extern "C" __declspec(dllexport) void __cdecl Calculate(int calculator)
{
	calculatorRegister.get(calculator)->calculate();
}

extern "C" __declspec(dllexport) double __cdecl GetVoltageReal(int calculator, int node)
{
	return calculatorRegister.get(calculator)->getVoltageReal(node);
}

extern "C" __declspec(dllexport) double __cdecl GetVoltageImaginary(int calculator, int node)
{
	return calculatorRegister.get(calculator)->getVoltageImaginary(node);
}

extern "C" __declspec(dllexport) void __cdecl SetConsoleOutput(ConsoleOutput function)
{
	consoleOutput = function;
}

void WriteLine(const char * text)
{
	if (consoleOutput != 0)
		consoleOutput(text);
}