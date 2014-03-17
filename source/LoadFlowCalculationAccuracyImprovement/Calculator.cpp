#include "Calculator.h"
#include <sstream>
#include <map>
#include <cmath>

using namespace std;

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(static_cast<floating>(targetPrecision)),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_admittances(nodeCount, nodeCount),
	_constantCurrents(nodeCount),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount),
	_consoleOutput(0),
	_blub(12)
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
	_constantCurrents[node] = static_cast<complexFloating>(value);
}

void Calculator::calculate()
{            
	stringstream stream;
	stream << "blub is " << _blub << endl;
	writeLine(stream.str().c_str());

	_factorization.analyzePattern(_admittances);
	_factorization.factorize(_admittances);
	_coefficients.clear();
	_inverseCoefficients.clear();
	std::vector<complexFloating> admittanceRowSums = calculateAdmittanceRowSum();
	calculateFirstCoefficient(admittanceRowSums);
	_inverseCoefficients.push_back(divide(complexFloating(1), _coefficients.front()));
	calculateSecondCoefficient(admittanceRowSums);
	calculateNextInverseCoefficient();
	map<floating, int> powerErrors;
	std::vector< std::vector<complexFloating> > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficients.size() < _numberOfCoefficients)
	{
		calculateNextCoefficient();
		calculateNextInverseCoefficient();

		calculateVoltagesFromCoefficients();
		floating powerError = calculatePowerError();

		powerErrors.insert(pair<floating, int>(powerError, partialResults.size()));
		partialResults.push_back(_voltages);
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

void Calculator::writeLine(const char *description, const Eigen::SparseMatrix<complexFloating> &matrix)
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

std::vector<Calculator::complexFloating> Calculator::solveAdmittanceEquationSystem(const std::vector<complexFloating> &rightHandSide)
{
	Eigen::Matrix<complexFloating, Eigen::Dynamic, 1> rightHandSideConverted = stdToUblasVector(rightHandSide);
	return ublasToStdVector(_factorization.solve(rightHandSideConverted));
}

std::vector<Calculator::complexFloating> Calculator::calculateAdmittanceRowSum()
{
	std::vector<complexFloating> result(_nodeCount);

	for (size_t row = 0; row < _nodeCount; ++row)
		for (size_t column = 0; column < _nodeCount; ++column)
			result[row] += _admittances.coeffRef(row, column);

	return result;
}

void Calculator::calculateFirstCoefficient(const std::vector<complexFloating> &admittanceRowSums)
{
	assert(_coefficients.size() == 0);

	std::vector<complexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()]*complexFloating(floating(-1));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()] + _constantCurrents[bus.getId()];
	}


	std::vector<complexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateSecondCoefficient(const std::vector<complexFloating> &admittanceRowSums)
{
	assert(_coefficients.size() == 1);
	assert(_inverseCoefficients.size() == 1);

	const std::vector<complexFloating> &previousCoefficients = _coefficients.back();
	const std::vector<complexFloating> &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector<complexFloating> rightHandSide(_nodeCount);
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		complexFloating ownCurrent = static_cast<complexFloating>(bus.getPower())*previousInverseCoefficients[id];
		complexFloating constantCurrent = _constantCurrents[id];
		complexFloating totalCurrent = conj(ownCurrent) + constantCurrent;
		rightHandSide[id] = admittanceRowSums[id] + totalCurrent;
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		floating realPower = static_cast<floating>(bus.getPowerReal());
		complexFloating previousCoefficient = previousCoefficients[id];
		complexFloating previousInverseCoefficient = previousInverseCoefficients[id];
		complexFloating admittanceRowSum = admittanceRowSums[id];
		floating magnitudeSquare = static_cast<floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (previousCoefficient*complexFloating(realPower*floating(2)) - previousInverseCoefficient)/complexFloating(magnitudeSquare) - admittanceRowSum;
	}
	
	std::vector<complexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextCoefficient()
{
	assert(_coefficients.size() > 1);
	assert(_inverseCoefficients.size() > 1);

	const std::vector<complexFloating> &previousCoefficients = _coefficients.back();
	const std::vector<complexFloating> &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector<complexFloating> rightHandSide(_nodeCount);
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		complexFloating power = converToComplexFloating(bus.getPower());
		rightHandSide[id] = conj(power*previousInverseCoefficients[id]);
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		floating realPower = static_cast<floating>(bus.getPowerReal());
		complexFloating previousCoefficient = previousCoefficients[id];
		complexFloating previousInverseCoefficient = previousInverseCoefficients[id];
		floating magnitudeSquare = static_cast<floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (previousCoefficient*complexFloating(realPower*floating(2)) - previousInverseCoefficient)/complexFloating(magnitudeSquare);
	}
	
	std::vector<complexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextInverseCoefficient()
{
	assert(_inverseCoefficients.size() > 0);
	assert(_inverseCoefficients.size() + 1 == _coefficients.size());

	size_t n = _coefficients.size() - 1;
	std::vector<complexFloating> result(_nodeCount);

	for (size_t i = 0; i < n; ++i)
	{
		const std::vector<complexFloating> &inverseCoefficient = _inverseCoefficients[i];
		const std::vector<complexFloating> &coefficient = _coefficients[n - i];
		
		assert(inverseCoefficient.size() == _nodeCount);
		assert(coefficient.size() == _nodeCount);

		std::vector<complexFloating> summand = pointwiseMultiply(coefficient, inverseCoefficient);
		result = add(result, summand);
	}

	result = pointwiseDivide(result, _coefficients[0]);
	result = multiply(result, complexFloating(floating(-1)));
	assert(result.size() == _nodeCount);
	_inverseCoefficients.push_back(result);
}

void Calculator::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		std::vector<complexFloating> coefficients(_coefficients.size());

		for (size_t j = 0; j < _coefficients.size(); ++j)
			coefficients[j] = _coefficients[j][i];

		_voltages[i] = calculateVoltageFromCoefficients(coefficients);
	}
}

Calculator::complexFloating Calculator::calculateVoltageFromCoefficients(const std::vector<complexFloating> &coefficients)
{
	std::vector<complexFloating> previousEpsilon(coefficients.size() + 1);
	std::vector<complexFloating> currentEpsilon(coefficients.size());

	complexFloating sum;
	for (size_t i = 0; i < coefficients.size(); ++i)
	{
		sum += coefficients[i];
		currentEpsilon[i] = sum;
	}

	size_t initialCount = coefficients.size();

	while(currentEpsilon.size() > 1)
	{
		std::vector<complexFloating> nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
        {
            complexFloating previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			nextEpsilon[j] = previousEpsilon[j + 1] + complexFloating(floating(1))/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	return initialCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
}

Calculator::floating Calculator::calculatePowerError() const
{
	Eigen::Matrix< complexFloating, Eigen::Dynamic, 1> voltages = stdToUblasVector(_voltages);
	Eigen::Matrix< complexFloating, Eigen::Dynamic, 1> currents = _admittances * voltages;
	std::vector<complexFloating> currentsConverted = ublasToStdVector(currents);
	std::vector<complexFloating> totalCurrents = subtract(currentsConverted, _constantCurrents);
	std::vector<complexFloating> powers = pointwiseMultiply(conjugate(totalCurrents), _voltages);
	
	assert(_nodeCount == powers.size());
	floating sum(0);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		complexFloating currentPower = powers[_pqBuses[i].getId()];
		complexFloating powerShouldBe = static_cast<complexFloating>(_pqBuses[i].getPower());
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

std::vector<Calculator::complexFloating> Calculator::pointwiseMultiply(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<complexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

std::vector<Calculator::complexFloating> Calculator::pointwiseDivide(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<complexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

std::vector<Calculator::complexFloating> Calculator::add(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<complexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

std::vector<Calculator::complexFloating> Calculator::subtract(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<complexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] - two[i];
	
	return result;
}

std::vector<Calculator::complexFloating> Calculator::multiply(const std::vector<complexFloating> &one, const complexFloating &two)
{
	std::vector<complexFloating> result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

std::vector<Calculator::complexFloating> Calculator::divide(const complexFloating &one, const std::vector<complexFloating> &two)
{
	std::vector<complexFloating> result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}

Calculator::floating Calculator::findMaximumMagnitude(const std::vector<complexFloating> &values)
{
	assert(values.size() > 0);
	floating result(0);

	for (size_t i = 0; i < values.size(); ++i)
	{
		floating magnitude = abs(values[i]);
		if (magnitude > result)
			result = magnitude;
	}

	return result;
}

Calculator::complexFloating Calculator::converToComplexFloating(const complex<double> &value)
{
	floating real = static_cast<floating>(value.real());
	floating imaginary = static_cast<floating>(value.imag());
	return complexFloating(real, imaginary);
}

std::vector<Calculator::complexFloating> Calculator::ublasToStdVector(const Eigen::Matrix<complexFloating, Eigen::Dynamic, 1> &values)
{
	std::vector<complexFloating> result(values.size());

	for (int i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

Eigen::Matrix<Calculator::complexFloating, Eigen::Dynamic, 1> Calculator::stdToUblasVector(const std::vector<complexFloating> &values)
{
	Eigen::Matrix<complexFloating, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

std::vector<Calculator::complexFloating> Calculator::conjugate(const std::vector<complexFloating> &values)
{
	std::vector<Calculator::complexFloating> result(values.size());

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