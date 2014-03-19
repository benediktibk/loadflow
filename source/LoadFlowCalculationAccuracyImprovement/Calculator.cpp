#include "Calculator.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <sstream>
#include <map>
#include <cmath>
#include <stdexcept>

using namespace std;

template class Calculator<long double, complex<long double> >;
template class Calculator<MultiPrecision, Complex<MultiPrecision> >;

template<typename Floating, typename ComplexFloating>
Calculator<Floating, ComplexFloating>::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(targetPrecision),
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
	_absolutePowerSum(0)
{ 
	assert(numberOfCoefficients > 0);
	assert(nodeCount > 0);
	assert(pqBusCount >= 0);
	assert(pvBusCount >= 0);
	_coefficients.reserve(_numberOfCoefficients);
	_inverseCoefficients.reserve(_numberOfCoefficients);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittance(int row, int column, complex<double> value)
{
	_admittances.insert(row, column) = converToComplexFloating(value);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setPQBus(int busId, int node, complex<double> power)
{
	_pqBuses[busId] = PQBus(node, power);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setPVBus(int busId, int node, double powerReal, double voltageMagnitude)
{
	_pvBuses[busId] = PVBus(node, powerReal, voltageMagnitude);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setConstantCurrent(int node, complex<double> value)
{
	_constantCurrents[node] = static_cast<ComplexFloating>(value);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculate()
{          
	calculateAbsolutePowerSum();
	_factorization.analyzePattern(_admittances);
	_factorization.factorize(_admittances);
	_coefficients.clear();
	_inverseCoefficients.clear();
	std::vector<ComplexFloating> admittanceRowSums = calculateAdmittanceRowSum();
	calculateFirstCoefficient(admittanceRowSums);
	_inverseCoefficients.push_back(divide(ComplexFloating(1), _coefficients.front()));
	calculateSecondCoefficient(admittanceRowSums);
	calculateNextInverseCoefficient();
	map<double, int> powerErrors;
	std::vector< std::vector<ComplexFloating> > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficients.size() < _numberOfCoefficients)
	{
		calculateNextCoefficient();
		calculateNextInverseCoefficient();

		try
		{
			calculateVoltagesFromCoefficients();
		}
		catch (overflow_error e)
		{
			writeLine("had to stop earlier because of numerical issues");
			break;
		}

		double powerError = calculatePowerError();

		powerErrors.insert(pair<double, int>(powerError, partialResults.size()));
		partialResults.push_back(_voltages);

		if (_absolutePowerSum != 0 && powerError/_absolutePowerSum < _targetPrecision)
		{
			writeLine("finished earlier because the power error is already small enough");
			break;
		}
	}

	if (!powerErrors.empty())
	{
		int bestResultIndex = powerErrors.begin()->second;
		_voltages = partialResults[bestResultIndex];
	}
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getVoltageReal(int node) const
{
	return static_cast<double>(_voltages[node].real());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getVoltageImaginary(int node) const
{
	return static_cast<double>(_voltages[node].imag());
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setConsoleOutput(ConsoleOutput function)
{
	_consoleOutput = function;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::writeLine(const char *description, const Eigen::SparseMatrix<ComplexFloating> &matrix)
{
	stringstream stream;
	stream << description << endl;
	stream << matrix.toDense();
	writeLine(stream.str().c_str());
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::writeLine(const char *text)
{
	if (_consoleOutput != 0)
		_consoleOutput(text);
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::solveAdmittanceEquationSystem(const std::vector<ComplexFloating> &rightHandSide)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> rightHandSideConverted = stdToUblasVector(rightHandSide);
	return ublasToStdVector(_factorization.solve(rightHandSideConverted));
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::calculateAdmittanceRowSum()
{
	std::vector<ComplexFloating> result(_nodeCount);

	for (size_t row = 0; row < _nodeCount; ++row)
		for (size_t column = 0; column < _nodeCount; ++column)
			result[row] += _admittances.coeffRef(row, column);

	return result;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateFirstCoefficient(const std::vector<ComplexFloating> &admittanceRowSums)
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

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateSecondCoefficient(const std::vector<ComplexFloating> &admittanceRowSums)
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

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateNextCoefficient()
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

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateNextInverseCoefficient()
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

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		std::vector<ComplexFloating> coefficients(_coefficients.size());

		for (size_t j = 0; j < _coefficients.size(); ++j)
			coefficients[j] = _coefficients[j][i];

		_voltages[i] = calculateVoltageFromCoefficients(coefficients);
	}
}

template<typename Floating, typename ComplexFloating>
ComplexFloating Calculator<Floating, ComplexFloating>::calculateVoltageFromCoefficients(const std::vector<ComplexFloating> &coefficients)
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
			if (abs(previousDifference) == Floating(0))
				throw overflow_error("numeric error, would have to divide by zero");
			nextEpsilon[j] = previousEpsilon[j + 1] + ComplexFloating(Floating(1))/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	return initialCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculatePowerError() const
{
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> voltages = stdToUblasVector(_voltages);
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> currents = _admittances * voltages;
	std::vector<ComplexFloating> currentsConverted = ublasToStdVector(currents);
	std::vector<ComplexFloating> totalCurrents = subtract(currentsConverted, _constantCurrents);
	std::vector<ComplexFloating> powers = pointwiseMultiply(conjugate(totalCurrents), _voltages);
	
	assert(_nodeCount == powers.size());
	double sum = 0;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		ComplexFloating currentPower = powers[_pqBuses[i].getId()];
		ComplexFloating powerShouldBe = static_cast<ComplexFloating>(_pqBuses[i].getPower());
		sum += static_cast<double>(abs(currentPower - powerShouldBe));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		Floating currentPower = powers[_pvBuses[i].getId()].real();
		Floating powerShouldBe  = static_cast<Floating>(_pvBuses[i].getPowerReal());
		sum += static_cast<double>(abs(currentPower - powerShouldBe));
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateAbsolutePowerSum()
{
	ComplexFloating sum;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		ComplexFloating power = static_cast<ComplexFloating>(_pqBuses[i].getPower());
		sum += ComplexFloating(abs(power.real()), abs(power.imag()));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		Floating power  = static_cast<Floating>(_pvBuses[i].getPowerReal());
		sum += ComplexFloating(abs(power), Floating(0));
	}

	_absolutePowerSum = abs(sum);
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::pointwiseMultiply(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::pointwiseDivide(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::add(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::subtract(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<ComplexFloating> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] - two[i];
	
	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::multiply(const std::vector<ComplexFloating> &one, const ComplexFloating &two)
{
	std::vector<ComplexFloating> result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::divide(const ComplexFloating &one, const std::vector<ComplexFloating> &two)
{
	std::vector<ComplexFloating> result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
Floating Calculator<Floating, ComplexFloating>::findMaximumMagnitude(const std::vector<ComplexFloating> &values)
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

template<typename Floating, typename ComplexFloating>
ComplexFloating Calculator<Floating, ComplexFloating>::converToComplexFloating(const complex<double> &value)
{
	Floating real = static_cast<Floating>(value.real());
	Floating imaginary = static_cast<Floating>(value.imag());
	return ComplexFloating(real, imaginary);
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::ublasToStdVector(const Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> &values)
{
	std::vector<ComplexFloating> result(values.size());

	for (int i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> Calculator<Floating, ComplexFloating>::stdToUblasVector(const std::vector<ComplexFloating> &values)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::conjugate(const std::vector<ComplexFloating> &values)
{
	std::vector<ComplexFloating> result(values.size());

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = conj(values[i]);

	return result;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getCoefficientReal(int step, int node) const
{
	return static_cast<double>(_coefficients[step][node].real());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getCoefficientImaginary(int step, int node) const
{
	return static_cast<double>(_coefficients[step][node].imag());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getInverseCoefficientReal(int step, int node) const
{
	return static_cast<double>(_inverseCoefficients[step][node].real());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getInverseCoefficientImaginary(int step, int node) const
{
	return static_cast<double>(_inverseCoefficients[step][node].imag());
}

template<typename Floating, typename ComplexFloating>
int Calculator<Floating, ComplexFloating>::getNodeCount() const
{
	return _nodeCount;
}