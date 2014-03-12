#include "Calculator.h"
#include <boost/numeric/ublas/lu.hpp>

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
	InitializeAdmittanceFactorization();
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

void Calculator::writeLine(const char *text)
{
	if (_consoleOutput != 0)
		_consoleOutput(text);
}

void Calculator::InitializeAdmittanceFactorization()
{	
	typedef permutation_matrix<std::size_t> pmatrix;

	// create a working copy of the input
	matrix< complex<double> > admittances(_admittances);

	// create a permutation matrix for the LU-factorization
	pmatrix permutationMatrix(admittances.size1());

	// perform LU-factorization
	int result = lu_factorize(admittances, permutationMatrix);
	assert(result == 0);

	// create identity matrix of "inverse"
	_admittancesInverse.assign(identity_matrix< complex<double> > (permutationMatrix.size()));

	// backsubstitute to get the inverse
	lu_substitute(admittances, permutationMatrix, _admittancesInverse);
}

ublas::vector< complex<double> > Calculator::solveAdmittanceEquationSystem(const ublas::vector< complex<double> > &rightHandSide)
{
	return prod(_admittancesInverse, rightHandSide);
}