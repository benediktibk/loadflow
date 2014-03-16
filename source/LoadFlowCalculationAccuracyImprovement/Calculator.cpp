#include "Calculator.h"
#include <sstream>
#include <map>
#include <boost/math/special_functions/fpclassify.hpp>

using namespace std;

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(static_cast<floating>(targetPrecision)),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_admittances(nodeCount, nodeCount),
	_constantCurrents(nodeCount, complex<floating>(0, 0)),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount, complex<floating>(0, 0)),
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
	_admittances.insert(row, column) = converToComplexFloating(value);
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
	_factorization.analyzePattern(_admittances);
	_factorization.factorize(_admittances);
	_coefficients.clear();
	_inverseCoefficients.clear();
	std::vector< complex<floating> > admittanceRowSums = calculateAdmittanceRowSum();
	calculateFirstCoefficient(admittanceRowSums);
	_inverseCoefficients.push_back(divide(complex<floating>(1, 0), _coefficients.front()));
	calculateSecondCoefficient(admittanceRowSums);
	calculateNextInverseCoefficient();
	map<floating, int> powerErrors;
	std::vector< std::vector< complex<floating> > > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficients.size() < _numberOfCoefficients)
	{
		calculateNextCoefficient();
		calculateNextInverseCoefficient();

		calculateVoltagesFromCoefficients();
		floating powerError = calculatePowerError();

		if (boost::math::isnormal(static_cast<double>(powerError)))
		{
			powerErrors.insert(pair<floating, int>(powerError, partialResults.size()));
			partialResults.push_back(_voltages);
		}
	}

	if (!powerErrors.empty())
		_voltages = partialResults[powerErrors.begin()->second];
}

double Calculator::getVoltageReal(int node) const
{
	return static_cast<double>(_voltages[node].real());
}

double Calculator::getVoltageImaginary(int node) const
{
	return static_cast<double>(_voltages[node].imag());
}

void Calculator::setConsoleOutput(ConsoleOutput function)
{
	_consoleOutput = function;
}

void Calculator::writeLine(const char *description, const Eigen::SparseMatrix< std::complex<floating> > &matrix)
{
	stringstream stream;
	stream << description << endl;
	stream << matrix.toDense();
	writeLine(stream.str().c_str());
}

void Calculator::writeLine(const char *text)
{
	if (_consoleOutput != 0)
		_consoleOutput(text);
}

std::vector< complex<Calculator::floating> > Calculator::solveAdmittanceEquationSystem(const std::vector< complex<floating> > &rightHandSide)
{
	Eigen::Matrix< std::complex<floating>, Eigen::Dynamic, 1> rightHandSideConverted = stdToUblasVector(rightHandSide);
	return ublasToStdVector(_factorization.solve(rightHandSideConverted));
}

std::vector< complex<Calculator::floating> > Calculator::calculateAdmittanceRowSum()
{
	std::vector< complex<floating> > result(_nodeCount, complex<floating>(0, 0));

	for (size_t row = 0; row < _nodeCount; ++row)
		for (size_t column = 0; column < _nodeCount; ++column)
			result[row] += _admittances.coeffRef(row, column);

	return result;
}

void Calculator::calculateFirstCoefficient(const std::vector< complex<floating> > &admittanceRowSums)
{
	assert(_coefficients.size() == 0);

	std::vector< complex<floating> > rightHandSide(_nodeCount, complex<floating>(0, 0));

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()]*complex<floating>(-1, 0);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()] + _constantCurrents[bus.getId()];
	}


	std::vector< complex<floating> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateSecondCoefficient(const std::vector< complex<floating> > &admittanceRowSums)
{
	assert(_coefficients.size() == 1);
	assert(_inverseCoefficients.size() == 1);

	const std::vector< complex<floating> > &previousCoefficients = _coefficients.back();
	const std::vector< complex<floating> > &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector< complex<floating> > rightHandSide(_nodeCount, complex<floating>(0, 0));
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		complex<floating> ownCurrent = static_cast< complex<floating> >(bus.getPower())*previousInverseCoefficients[id];
		complex<floating> constantCurrent = _constantCurrents[id];
		complex<floating> totalCurrent = conj(ownCurrent) + constantCurrent;
		rightHandSide[id] = admittanceRowSums[id] + totalCurrent;
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		floating realPower = static_cast<floating>(bus.getPowerReal());
		complex<floating> previousCoefficient = previousCoefficients[id];
		complex<floating> previousInverseCoefficient = previousInverseCoefficients[id];
		complex<floating> admittanceRowSum = admittanceRowSums[id];
		floating magnitudeSquare = static_cast<floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (2*realPower*previousCoefficient - previousInverseCoefficient)/magnitudeSquare - admittanceRowSum;
	}
	
	std::vector< complex<floating> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextCoefficient()
{
	assert(_coefficients.size() > 1);
	assert(_inverseCoefficients.size() > 1);

	const std::vector< complex<floating> > &previousCoefficients = _coefficients.back();
	const std::vector< complex<floating> > &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector< complex<floating> > rightHandSide(_nodeCount, complex<floating>(0, 0));
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		complex<floating> power = converToComplexFloating(bus.getPower());
		rightHandSide[id] = conj(power*previousInverseCoefficients[id]);
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		floating realPower = static_cast<floating>(bus.getPowerReal());
		complex<floating> previousCoefficient = previousCoefficients[id];
		complex<floating> previousInverseCoefficient = previousInverseCoefficients[id];
		floating magnitudeSquare = static_cast<floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (2*realPower*previousCoefficient - previousInverseCoefficient)/magnitudeSquare;
	}
	
	std::vector< complex<floating> > coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextInverseCoefficient()
{
	assert(_inverseCoefficients.size() > 0);
	assert(_inverseCoefficients.size() + 1 == _coefficients.size());

	size_t n = _coefficients.size() - 1;
	std::vector< complex<floating> > result(_nodeCount, complex<floating>(0, 0));

	for (size_t i = 0; i < n; ++i)
	{
		const std::vector< complex<floating> > &inverseCoefficient = _inverseCoefficients[i];
		const std::vector< complex<floating> > &coefficient = _coefficients[n - i];
		
		assert(inverseCoefficient.size() == _nodeCount);
		assert(coefficient.size() == _nodeCount);

		std::vector< complex<floating> > summand = pointwiseMultiply(coefficient, inverseCoefficient);
		result = add(result, summand);
	}

	result = pointwiseDivide(result, _coefficients[0]);
	result = multiply(result, complex<floating>(-1, 0));
	assert(result.size() == _nodeCount);
	_inverseCoefficients.push_back(result);
}

void Calculator::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		std::vector< complex<floating> > coefficients(_coefficients.size());

		for (size_t j = 0; j < _coefficients.size(); ++j)
			coefficients[j] = _coefficients[j][i];

		_voltages[i] = calculateVoltageFromCoefficients(coefficients);
	}
}

complex<Calculator::floating> Calculator::calculateVoltageFromCoefficients(const std::vector< complex<floating> > &coefficients)
{
	std::vector< complex<floating> > previousEpsilon(coefficients.size() + 1, complex<floating>(0, 0));
	std::vector< complex<floating> > currentEpsilon(coefficients.size(), complex<floating>(0, 0));

	complex<floating> sum(0, 0);
	for (size_t i = 0; i < coefficients.size(); ++i)
	{
		sum += coefficients[i];
		currentEpsilon[i] = sum;
	}

	size_t initialCount = coefficients.size();

	while(currentEpsilon.size() > 1)
	{
		std::vector< complex<floating> > nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
        {
            complex<floating> previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			nextEpsilon[j] = previousEpsilon[j + 1] + complex<floating>(1, 0)/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	return initialCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
}

Calculator::floating Calculator::calculatePowerError() const
{
	Eigen::Matrix< std::complex<floating>, Eigen::Dynamic, 1> voltages = stdToUblasVector(_voltages);
	Eigen::Matrix< std::complex<floating>, Eigen::Dynamic, 1> currents = _admittances * voltages;
	std::vector< complex<floating> > currentsConverted = ublasToStdVector(currents);
	std::vector< complex<floating> > totalCurrents = subtract(currentsConverted, _constantCurrents);
	std::vector< complex<floating> > powers = pointwiseMultiply(conjugate(totalCurrents), _voltages);
	
	assert(_nodeCount == powers.size());
	floating sum = 0;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const complex<floating> &currentPower = powers[_pqBuses[i].getId()];
		const complex<floating> &powerShouldBe = _pqBuses[i].getPower();
		sum += abs(currentPower - powerShouldBe);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		floating currentPower = powers[_pvBuses[i].getId()].real();
		floating powerShouldBe  = static_cast<floating>(_pvBuses[i].getPowerReal());
		sum += abs(currentPower - powerShouldBe);
	}

	return abs(sum);
}

std::vector< complex<Calculator::floating> > Calculator::pointwiseMultiply(const std::vector< complex<floating> > &one, const std::vector< complex<floating> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<floating> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

std::vector< complex<Calculator::floating> > Calculator::pointwiseDivide(const std::vector< complex<floating> > &one, const std::vector< complex<floating> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<floating> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

std::vector< complex<Calculator::floating> > Calculator::add(const std::vector< complex<floating> > &one, const std::vector< complex<floating> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<floating> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

std::vector< complex<Calculator::floating> > Calculator::subtract(const std::vector< complex<floating> > &one, const std::vector< complex<floating> > &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector< complex<floating> > result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] - two[i];
	
	return result;
}

std::vector< complex<Calculator::floating> > Calculator::multiply(const std::vector< complex<floating> > &one, const complex<floating> &two)
{
	std::vector< complex<floating> > result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

std::vector< complex<Calculator::floating> > Calculator::divide(const complex<floating> &one, const std::vector< complex<floating> > &two)
{
	std::vector< complex<floating> > result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}

Calculator::floating Calculator::findMaximumMagnitude(const std::vector< std::complex<floating> > &values)
{
	assert(values.size() > 0);
	floating result = 0;

	for (size_t i = 0; i < values.size(); ++i)
	{
		floating magnitude = abs(values[i]);
		if (magnitude > result)
			result = magnitude;
	}

	return result;
}

complex<Calculator::floating> Calculator::converToComplexFloating(const complex<double> &value)
{
	floating real = static_cast<floating>(value.real());
	floating imaginary = static_cast<floating>(value.imag());
	return complex<floating>(real, imaginary);
}

std::vector< complex<Calculator::floating> > Calculator::ublasToStdVector(const Eigen::Matrix< std::complex<floating>, Eigen::Dynamic, 1> &values)
{
	std::vector< complex<floating> > result(values.size());

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

Eigen::Matrix< std::complex<Calculator::floating>, Eigen::Dynamic, 1> Calculator::stdToUblasVector(const std::vector< complex<floating> > &values)
{
	Eigen::Matrix< std::complex<floating>, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

std::vector< complex<Calculator::floating> > Calculator::conjugate(const std::vector< complex<Calculator::floating> > &values)
{
	std::vector< complex<Calculator::floating> > result(values.size());

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = conj(values[i]);

	return result;
}

double Calculator::getCoefficientReal(int step, int node) const
{
	return static_cast<double>(_coefficients[step][node].real());
}

double Calculator::getCoefficientImaginary(int step, int node) const
{
	return static_cast<double>(_coefficients[step][node].imag());
}

double Calculator::getInverseCoefficientReal(int step, int node) const
{
	return static_cast<double>(_inverseCoefficients[step][node].real());
}

double Calculator::getInverseCoefficientImaginary(int step, int node) const
{
	return static_cast<double>(_inverseCoefficients[step][node].imag());
}

int Calculator::getNodeCount() const
{
	return _nodeCount;
}