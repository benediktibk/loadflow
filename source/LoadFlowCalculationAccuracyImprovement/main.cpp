#include "CalculatorRegister.h"
#include "ConsoleOutput.h"

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

extern "C" __declspec(dllexport) void __cdecl SetAdmittanceReal(int calculator, int row, int column, double value)
{
	calculatorRegister.get(calculator)->setAdmittanceReal(row, column, value);
}

extern "C" __declspec(dllexport) void __cdecl SetAdmittanceImaginary(int calculator, int row, int column, double value)
{
	calculatorRegister.get(calculator)->setAdmittanceImaginary(row, column, value);
}

extern "C" __declspec(dllexport) void __cdecl SetPQBusPowerReal(int calculator, int busId, int node, double value)
{
	calculatorRegister.get(calculator)->setPQBusPowerReal(busId, node, value);
}

extern "C" __declspec(dllexport) void __cdecl SetPQBusPowerImaginary(int calculator, int busId, int node, double value)
{
	calculatorRegister.get(calculator)->setPQBusPowerImaginary(busId, node, value);
}

extern "C" __declspec(dllexport) void __cdecl SetPVBusPowerReal(int calculator, int busId, int node, double value)
{
	calculatorRegister.get(calculator)->setPVBusPowerReal(busId, node, value);
}

extern "C" __declspec(dllexport) void __cdecl SetPVBusVoltageMagnitude(int calculator, int busId, int node, double value)
{
	calculatorRegister.get(calculator)->setPVBusVoltageMagnitude(busId, node, value);
}

extern "C" __declspec(dllexport) void __cdecl SetConstantCurrentReal(int calculator, int node, double value)
{
	calculatorRegister.get(calculator)->setConstantCurrentReal(node, value);
}

extern "C" __declspec(dllexport) void __cdecl SetConstantCurrentImaginary(int calculator, int node, double value)
{
	calculatorRegister.get(calculator)->setConstantCurrentImaginary(node, value);
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