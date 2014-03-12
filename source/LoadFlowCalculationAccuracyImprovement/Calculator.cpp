#include "Calculator.h"

using namespace std;

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(targetPrecision),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_admittances(nodeCount, nodeCount),
	_constantCurrents(nodeCount, complex<double>(0, 0)),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount, complex<double>(0, 0)),
	_consoleOutput(0)
{ }

void Calculator::setAdmittance(int row, int column, complex<double> value)
{
	_admittances(row, column) = value;
}

void Calculator::setPQBus(int busId, int node, complex<double> power)
{
	_pqBuses[busId] = PQBus(node, power);
}

void Calculator::setPVBus(int busId, int node, double powerReal, double voltageMagnitude)
{
	_pvBuses[busId] = PVBus(node, powerReal, voltageMagnitude);
}

void Calculator::setConstantCurrent(int node, complex<double> value)
{
	_constantCurrents[node] = value;
}

void Calculator::calculate()
{
	writeLine("calculating");
}

double Calculator::getVoltageReal(int node) const
{
	return _voltages[node].real();
}

double Calculator::getVoltageImaginary(int node) const
{
	return _voltages[node].imag();
}

void Calculator::setConsoleOutput(ConsoleOutput function)
{
	_consoleOutput = function;
}

void Calculator::writeLine(const char *text)
{
	if (_consoleOutput != 0)
		_consoleOutput(text);
}