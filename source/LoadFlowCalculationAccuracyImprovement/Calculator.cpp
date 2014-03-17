#include "Calculator.h"
#include <sstream>
#include <map>
#include <cmath>

using namespace std;

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(static_cast<Floating>(targetPrecision)),
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
	_constantCurrents[node] = static_cast<ComplexFloating>(value);
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
	std::vector<ComplexFloating> admittanceRowSums = calculateAdmittanceRowSum();
	calculateFirstCoefficient(admittanceRowSums);
	_inverseCoefficients.push_back(divide(ComplexFloating(1), _coefficients.front()));
	calculateSecondCoefficient(admittanceRowSums);
	calculateNextInverseCoefficient();
	map<Floating, int> powerErrors;
	std::vector< std::vector<ComplexFloating> > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficients.size() < _numberOfCoefficients)
	{
		calculateNextCoefficient();
		calculateNextInverseCoefficient();

		calculateVoltagesFromCoefficients();
		Floating powerError = calculatePowerError();

		powerErrors.insert(pair<Floating, int>(powerError, partialResults.size()));
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

void Calculator::writeLine(const char *description, const Eigen::SparseMatrix<ComplexFloating> &matrix)
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

std::vector<Calculator::ComplexFloating> Calculator::solveAdmittanceEquationSystem(const std::vector<ComplexFloating> &rightHandSide)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> rightHandSideConverted = stdToUblasVector(rightHandSide);
	return ublasToStdVector(_factorization.solve(rightHandSideConverted));
}

std::vector<Calculator::ComplexFloating> Calculator::calculateAdmittanceRowSum()
{
	std::vector<ComplexFloating> result(_nodeCount);

	for (size_t row = 0; row < _nodeCount; ++row)
		for (size_t column = 0; column < _nodeCount; ++column)
			result[row] += _admittances.coeffRef(row, column);

	return result;
}

void Calculator::calculateFirstCoefficient(const std::vector<ComplexFloating> &admittanceRowSums)
{
	assert(_coefficients.size() == 0);

	std::vector<ComplexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()]*ComplexFloating(Floating(-1));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		rightHandSide[bus.getId()] = admittanceRowSums[bus.getId()] + _constantCurrents[bus.getId()];
	}


	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateSecondCoefficient(const std::vector<ComplexFloating> &admittanceRowSums)
{
	assert(_coefficients.size() == 1);
	assert(_inverseCoefficients.size() == 1);

	const std::vector<ComplexFloating> &previousCoefficients = _coefficients.back();
	const std::vector<ComplexFloating> &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector<ComplexFloating> rightHandSide(_nodeCount);
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating ownCurrent = static_cast<ComplexFloating>(bus.getPower())*previousInverseCoefficients[id];
		ComplexFloating constantCurrent = _constantCurrents[id];
		ComplexFloating totalCurrent = conj(ownCurrent) + constantCurrent;
		rightHandSide[id] = admittanceRowSums[id] + totalCurrent;
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		Floating realPower = static_cast<Floating>(bus.getPowerReal());
		ComplexFloating previousCoefficient = previousCoefficients[id];
		ComplexFloating previousInverseCoefficient = previousInverseCoefficients[id];
		ComplexFloating admittanceRowSum = admittanceRowSums[id];
		Floating magnitudeSquare = static_cast<Floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (previousCoefficient*ComplexFloating(realPower*Floating(2)) - previousInverseCoefficient)/ComplexFloating(magnitudeSquare) - admittanceRowSum;
	}
	
	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextCoefficient()
{
	assert(_coefficients.size() > 1);
	assert(_inverseCoefficients.size() > 1);

	const std::vector<ComplexFloating> &previousCoefficients = _coefficients.back();
	const std::vector<ComplexFloating> &previousInverseCoefficients = _inverseCoefficients.back();
	std::vector<ComplexFloating> rightHandSide(_nodeCount);
			
	assert(previousInverseCoefficients.size() == _nodeCount);
	assert(previousCoefficients.size() == _nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating power = converToComplexFloating(bus.getPower());
		rightHandSide[id] = conj(power*previousInverseCoefficients[id]);
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		const PVBus &bus = _pvBuses[i];
		int id = bus.getId();
		Floating realPower = static_cast<Floating>(bus.getPowerReal());
		ComplexFloating previousCoefficient = previousCoefficients[id];
		ComplexFloating previousInverseCoefficient = previousInverseCoefficients[id];
		Floating magnitudeSquare = static_cast<Floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
		rightHandSide[id] = (previousCoefficient*ComplexFloating(realPower*Floating(2)) - previousInverseCoefficient)/ComplexFloating(magnitudeSquare);
	}
	
	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficients.push_back(coefficients);
}

void Calculator::calculateNextInverseCoefficient()
{
	assert(_inverseCoefficients.size() > 0);
	assert(_inverseCoefficients.size() + 1 == _coefficients.size());

	size_t n = _coefficients.size() - 1;
	std::vector<ComplexFloating> result(_nodeCount);

	for (size_t i = 0; i < n; ++i)
	{
		const std::vector<ComplexFloating> &inverseCoefficient = _inverseCoefficients[i];
		const std::vector<ComplexFloating> &coefficient = _coefficients[n - i];
		
		assert(inverseCoefficient.size() == _nodeCount);
		assert(coefficient.size() == _nodeCount);

		std::vector<ComplexFloating> summand = pointwiseMultiply(coefficient, inverseCoefficient);
		result = add(result, summand);
	}

	result = pointwiseDivide(result, _coefficients[0]);
	result = multiply(result, ComplexFloating(Floating(-1)));
	assert(result.size() == _nodeCount);
	_inverseCoefficients.push_back(result);
}

void Calculator::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		std::vector<ComplexFloating> coefficients(_coefficients.size());

		for (size_t j = 0; j < _coefficients.size(); ++j)
			coefficients[j] = _coefficients[j][i];

		_voltages[i] = calculateVoltageFromCoefficients(coefficients);
	}
}

Calculator::ComplexFloating Calculator::calculateVoltageFromCoefficients(const std::vector<ComplexFloating> &coefficients)
{
	std::vector<ComplexFloating> previousEpsilon(coefficients.size() + 1);
	std::vector<ComplexFloating> currentEpsilon(coefficients.size());

	ComplexFloating sum;
	for (size_t i = 0; i < coefficients.size(); ++i)
	{
		sum += coefficients[i];
		currentEpsilon[i] = sum;
	}

	size_t initialCount = coefficients.size();

	while(currentEpsilon.size() > 1)
	{
		std::vector<ComplexFloating> nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
        {
            ComplexFloating previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			nextEpsilon[j] = previousEpsilon[j + 1] + ComplexFloating(Floating(1))/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	return initialCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
}

Calculator::Floating Calculator::calculatePowerError() const
{
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> voltages = stdToUblasVector(_voltages);
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> currents = _admittances * voltages;
	std::vector<ComplexFloating> currentsConverted = ublasToStdVector(currents);
	std::vector<ComplexFloating> totalCurrents = subtract(currentsConverted, _constantCurrents);
	std::vector<ComplexFloating> powers = pointwiseMultiply(conjugate(totalCurrents), _voltages);
	
	assert(_nodeCount == powers.size());
	Floating sum(0);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		ComplexFloating currentPower = powers[_pqBuses[i].getId()];
		ComplexFloating powerShouldBe = static_cast<ComplexFloating>(_pqBuses[i].getPower());
		sum += abs(currentPower - powerShouldBe);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		Floating currentPower = powers[_pvBuses[i].getId()].real();
		Floating powerShouldBe  = static_cast<Floating>(_pvBuses[i].getPowerReal());
		sum += abs(currentPower - powerShouldBe);
	}

	return abs(sum);
}

std::vector<Calculator::ComplexFloating> Calculator::pointwiseMultiply(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::pointwiseDivide(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::add(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::subtract(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] - two[i];
	
	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::multiply(const std::vector<ComplexFloating> &one, const ComplexFloating &two)
{
	std::vector<ComplexFloating> result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::divide(const ComplexFloating &one, const std::vector<ComplexFloating> &two)
{
	std::vector<ComplexFloating> result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}

Calculator::Floating Calculator::findMaximumMagnitude(const std::vector<ComplexFloating> &values)
{
	assert(values.size() > 0);
	Floating result(0);

	for (size_t i = 0; i < values.size(); ++i)
	{
		Floating magnitude = abs(values[i]);
		if (magnitude > result)
			result = magnitude;
	}

	return result;
}

Calculator::ComplexFloating Calculator::converToComplexFloating(const complex<double> &value)
{
	Floating real = static_cast<Floating>(value.real());
	Floating imaginary = static_cast<Floating>(value.imag());
	return ComplexFloating(real, imaginary);
}

std::vector<Calculator::ComplexFloating> Calculator::ublasToStdVector(const Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> &values)
{
	std::vector<ComplexFloating> result(values.size());

	for (int i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

Eigen::Matrix<Calculator::ComplexFloating, Eigen::Dynamic, 1> Calculator::stdToUblasVector(const std::vector<ComplexFloating> &values)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

std::vector<Calculator::ComplexFloating> Calculator::conjugate(const std::vector<ComplexFloating> &values)
{
	std::vector<Calculator::ComplexFloating> result(values.size());

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