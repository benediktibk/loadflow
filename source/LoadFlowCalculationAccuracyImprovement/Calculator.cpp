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
	_coefficients.reserve(_numberOfCoefficients);
	_inverseCoefficients.reserve(_numberOfCoefficients);
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
	writeLine("calculating initial coefficients");
	_coefficients.clear();
	_inverseCoefficients.clear();
	std::vector< complex<double> > admittanceRowSums = calculateAdmittanceRowSum();
	calculateFirstCoefficient(admittanceRowSums);
	_inverseCoefficients.push_back(divide(complex<double>(1, 0), _coefficients.front()));
	calculateSecondCoefficient(admittanceRowSums);
	calculateNextInverseCoefficient();
	
	writeLine("calculating coefficients");

	while (_coefficients.size() < _numberOfCoefficients)
	{
		calculateNextCoefficient();
		calculateNextInverseCoefficient();
	}

	writeLine("calculating analytic continuation");
	calculateVoltagesFromCoefficients();
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

void Calculator::writeLine(const char *text, size_t argument)
{	
	stringstream stream;
	stream << text << ": " << argument;
	writeLine(stream.str());
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

std::vector< complex<double> > Calculator::solveAdmittanceEquationSystem(const std::vector< complex<double> > &rightHandSide)
{
	ublas::vector< complex<double> > rightHandSideConverted(rightHandSide.size());

	for (size_t i = 0; i < rightHandSide.size(); ++i)
		rightHandSideConverted[i] = rightHandSide[i];

	ublas::vector< complex<double> > rightHandSideWithQ = prod(_admittancesQ, rightHandSideConverted);
	ublas::vector< complex<double> > resultUblasVector = solve(_admittancesR, rightHandSideWithQ, upper_tag());
	std::vector< complex<double> > resultConverted(resultUblasVector.size());

	for (size_t i = 0; i < resultUblasVector.size(); ++i)
		resultConverted[i] = resultUblasVector[i];

	return resultConverted;
}

std::vector< complex<double> > Calculator::calculateAdmittanceRowSum() const
{
	std::vector< complex<double> > result(_nodeCount, complex<double>(0, 0));

	for (size_t row = 0; row < _nodeCount; ++row)
		for (size_t column = 0; column < _nodeCount; ++column)
			result[row] += _admittances(row, column);

	return result;
}

void Calculator::calculateFirstCoefficient(const std::vector< complex<double> > &admittanceRowSums)
{
	assert(_coefficients.size() == 0);

	std::vector< complex<double> > rightHandSide(_nodeCount, complex<double>(0, 0));

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()]*(-1);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()] + _constantCurrents[bus.getId()];
	}


	std::vector< complex<double> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateSecondCoefficient(const std::vector< complex<double> > &admittanceRowSums)
{
	assert(_coefficients.size() == 1);
	assert(_inverseCoefficients.size() == 1);

	const std::vector< complex<double> > &previousCoefficients = _coefficients.back();
	const std::vector< complex<double> > &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector< complex<double> > rightHandSide(_nodeCount, complex<double>(0, 0));
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		complex<double> ownCurrent = bus.getPower()*previousInverseCoefficients[id];
		complex<double> constantCurrent = _constantCurrents[id];
		complex<double> totalCurrent = conj(ownCurrent) + constantCurrent;
		rightHandSide[id] = admittanceRowSums[id] + totalCurrent;
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		double realPower = bus.getPowerReal();
		complex<double> previousCoefficient = previousCoefficients[id];
		complex<double> previousInverseCoefficient = previousInverseCoefficients[id];
		complex<double> admittanceRowSum = admittanceRowSums[id];
		double magnitudeSquare = bus.getVoltageMagnitude()*bus.getVoltageMagnitude();
		rightHandSide[id] = (2*realPower*previousCoefficient - previousInverseCoefficient)/magnitudeSquare - admittanceRowSum;
	}
	
	std::vector< complex<double> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextCoefficient()
{
	assert(_coefficients.size() > 1);
	assert(_inverseCoefficients.size() > 1);

	const std::vector< complex<double> > &previousCoefficients = _coefficients.back();
	const std::vector< complex<double> > &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector< complex<double> > rightHandSide(_nodeCount, complex<double>(0, 0));
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		rightHandSide[id] = conj(bus.getPower()*previousInverseCoefficients[id]);
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		double realPower = bus.getPowerReal();
		complex<double> previousCoefficient = previousCoefficients[id];
		complex<double> previousInverseCoefficient = previousInverseCoefficients[id];
		double magnitudeSquare = bus.getVoltageMagnitude()*bus.getVoltageMagnitude();
		rightHandSide[id] = (2*realPower*previousCoefficient - previousInverseCoefficient)/magnitudeSquare;
	}
	
	std::vector< complex<double> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextInverseCoefficient()
{
	assert(_inverseCoefficients.size() > 0);
	assert(_inverseCoefficients.size() + 1 == _coefficients.size());

	size_t n = _coefficients.size() - 1;
	std::vector< complex<double> > result(_nodeCount, complex<double>(0, 0));

	for (size_t i = 0; i < n; ++i)
	{
		const std::vector< complex<double> > &inverseCoefficient = _inverseCoefficients[i];
		const std::vector< complex<double> > &coefficient = _coefficients[n - i];
		
		assert(inverseCoefficient.size() == _nodeCount);
		assert(coefficient.size() == _nodeCount);

		std::vector< complex<double> > summand = pointwiseMultiply(coefficient, inverseCoefficient);
		result = add(result, summand);
	}

	result = pointwiseDivide(result, _coefficients[0]);
	result = multiply(result, complex<double>(-1, 0));
	assert(result.size() == _nodeCount);
	_inverseCoefficients.push_back(result);
}

void Calculator::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		std::vector< complex<double> > coefficients(_numberOfCoefficients);

		for (size_t j = 0; j < _coefficients.size(); ++j)
			coefficients[j] = _coefficients[j][i];

		_voltages[i] = calculateVoltageFromCoefficients(coefficients);
	}
}

complex<double> Calculator::calculateVoltageFromCoefficients(const std::vector< complex<double> > &coefficients)
{
	assert(coefficients.size() == _numberOfCoefficients);
	std::vector< complex<double> > previousEpsilon(_numberOfCoefficients + 1, complex<double>(0, 0));
	std::vector< complex<double> > currentEpsilon(_numberOfCoefficients, complex<double>(0, 0));

	complex<double> sum(0, 0);
	for (size_t i = 0; i < _numberOfCoefficients; ++i)
	{
		sum += coefficients[i];
		currentEpsilon[i] = sum;
	}

	while(currentEpsilon.size() > 2)
	{
		std::vector< complex<double> > nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
        {
            complex<double> previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			nextEpsilon[j] = previousEpsilon[j + 1] + complex<double>(1, 0)/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	return coefficients.size() % 2 ? currentEpsilon.back() : previousEpsilon.back();
}

std::vector< complex<double> > Calculator::pointwiseMultiply(const std::vector< complex<double> > &one, const std::vector< complex<double> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<double> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

std::vector< complex<double> > Calculator::pointwiseDivide(const std::vector< complex<double> > &one, const std::vector< complex<double> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<double> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

std::vector< complex<double> > Calculator::add(const std::vector< complex<double> > &one, const std::vector< complex<double> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<double> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

std::vector< complex<double> > Calculator::multiply(const std::vector< complex<double> > &one, const complex<double> &two)
{
	std::vector< complex<double> > result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

std::vector< complex<double> > Calculator::divide(const complex<double> &one, const std::vector< complex<double> > &two)
{
	std::vector< complex<double> > result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}