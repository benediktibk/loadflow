#include "Calculator.h"
#include <boost/numeric/ublas/triangular.hpp>
#include <sstream>

using namespace std;
using namespace boost::numeric;
using namespace boost::numeric::ublas;

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(targetPrecision),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_admittances(nodeCount, nodeCount),
	_admittancesQ(nodeCount, nodeCount),
	_admittancesR(nodeCount, nodeCount),
	_constantCurrents(nodeCount, complex<double>(0, 0)),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount, complex<double>(0, 0)),
	_consoleOutput(0)
{ 
	assert(numberOfCoefficients > 0);
	assert(nodeCount > 0);
	assert(pqBusCount >= 0);
	assert(pvBusCount >= 0);
}

void Calculator::setAdmittance(int row, int column, complex<double> value)
{
	_admittances(row, column) = value;
}

void Calculator::setAdmittanceQ(int row, int column, complex<double> value)
{
	_admittancesQ(row, column) = value;
}

void Calculator::setAdmittanceR(int row, int column, complex<double> value)
{
	_admittancesR(row, column) = value;
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
	ublas::vector< complex<double> > rightHandSide(_nodeCount);

	for (size_t i = 0; i < _nodeCount; ++i)
		rightHandSide(i) = _constantCurrents[i];

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		const complex<double> &power = bus.getPower();
		rightHandSide(bus.getId()) += complex<double>(power.real(), (-1)*power.imag());
	}

	ublas::vector< complex<double> > result = solveAdmittanceEquationSystem(rightHandSide);

	for (size_t i = 0; i < _nodeCount; ++i)
		_voltages[i] = result(i);
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

void Calculator::writeLine(const matrix< complex<double> > &matrix)
{
	stringstream stream;
	
	for (size_t i = 0; i < matrix.size1(); ++i)
	{
		for (size_t j = 0; j < matrix.size2(); ++j)
			stream << matrix(i, j) << " - ";

		stream << "/";
	}

	writeLine(stream.str());
}

void Calculator::writeLine(const string &text)
{
	writeLine(text.c_str());
}

void Calculator::writeLine(const char *text)
{
	if (_consoleOutput != 0)
		_consoleOutput(text);
}

ublas::vector< complex<double> > Calculator::solveAdmittanceEquationSystem(const ublas::vector< complex<double> > &rightHandSide)
{
	ublas::vector< complex<double> > rightHandSideWithQ = prod(_admittancesQ, rightHandSide);
	return solve(_admittancesR, rightHandSideWithQ, upper_tag());
}