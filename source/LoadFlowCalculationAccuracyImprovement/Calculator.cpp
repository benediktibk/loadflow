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
	_totalAdmittanceRowSums(nodeCount),
	_constantCurrents(nodeCount),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount),
	_consoleOutput(0),
	_absolutePowerSum(0),
	_coefficientStorage(0),
	_embeddingModification(0)
{ 
	assert(numberOfCoefficients > 0);
	assert(nodeCount > 0);
	assert(pqBusCount >= 0);
	assert(pvBusCount >= 0);
}

template<typename Floating, typename ComplexFloating>
Calculator<Floating, ComplexFloating>::~Calculator()
{
	delete _coefficientStorage;
	_coefficientStorage = 0;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittance(int row, int column, complex<double> value)
{
	_admittances.insert(row, column) = ComplexFloating(value);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittanceRowSum(int row, complex<double> value)
{
	_totalAdmittanceRowSums[row] = ComplexFloating(value);
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
	delete _coefficientStorage;
	_coefficientStorage = new CoefficientStorage<ComplexFloating, Floating>(_numberOfCoefficients, _nodeCount, _pqBuses, _pvBuses, _admittances);
	calculateAbsolutePowerSum();
	_factorization.analyzePattern(_admittances);
	_factorization.factorize(_admittances);
	vector<ComplexFloating> partialAdmittanceRowSums = calculateAdmittanceRowSum();

	if (!calculateFirstCoefficient(partialAdmittanceRowSums))
		return;

	calculateSecondCoefficient(partialAdmittanceRowSums);
	map<double, int> powerErrors;
	std::vector< std::vector< complex<double> > > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficientStorage->getCoefficientCount() < _numberOfCoefficients)
	{
		calculateNextCoefficient();

		try
		{
			_voltages = _coefficientStorage->calculateVoltagesFromCoefficients();
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
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> rightHandSideConverted = stdToEigenVector(rightHandSide);
	return eigenToStdVector(_factorization.solve(rightHandSideConverted));
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
bool Calculator<Floating, ComplexFloating>::calculateFirstCoefficient(vector<ComplexFloating> const& admittanceRowSum)
{
	std::vector<ComplexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		rightHandSide[bus.getId()] = (admittanceRowSum[bus.getId()] + _embeddingModification)*ComplexFloating(Floating(-1));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		int id = bus.getId();
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[id];
		ComplexFloating const& constantCurrent = _constantCurrents[id];
		rightHandSide[id] = admittanceRowSum + _embeddingModification + constantCurrent;
	}

	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);

	for (size_t i = 0; i < coefficients.size(); ++i)
		if (coefficients[i] == ComplexFloating(Floating(0), Floating(0)))
			return false;

	_coefficientStorage->addCoefficients(coefficients);
	return true;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateSecondCoefficient(vector<ComplexFloating> const& admittanceRowSums)
{
	std::vector<ComplexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		PQBus const& bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating const& ownCurrent = static_cast<ComplexFloating>(bus.getPower())*_coefficientStorage->getLastInverseCoefficient(id);
		ComplexFloating const& constantCurrent = _constantCurrents[id];
		ComplexFloating const& totalCurrent = conj(ownCurrent) + constantCurrent;
		rightHandSide[id] = admittanceRowSums[id] + _embeddingModification + totalCurrent;
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[bus.getId()];
		rightHandSide[bus.getId()] = calculateRightHandSide(bus) - (admittanceRowSum + _embeddingModification);
	}
	
	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateNextCoefficient()
{
	std::vector<ComplexFloating> rightHandSide(_nodeCount);
			
	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating power(bus.getPower());
		rightHandSide[id] = conj(power*_coefficientStorage->getLastInverseCoefficient(id));
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		rightHandSide[bus.getId()] = calculateRightHandSide(bus);
	}
	
	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
ComplexFloating Calculator<Floating, ComplexFloating>::calculateRightHandSide(PVBus const& bus)
{
	int id = bus.getId();
	Floating realPower = static_cast<Floating>(bus.getPowerReal());
	ComplexFloating const& previousCoefficient = _coefficientStorage->getLastCoefficient(id);
	ComplexFloating const& previousCombinedCoefficient = _coefficientStorage->getLastCombinedCoefficient(id);
	ComplexFloating const& previousSquaredCoefficient = _coefficientStorage->getLastSquaredCoefficient(id);
	ComplexFloating const& constantCurrent = _constantCurrents[id];
	Floating magnitudeSquare = static_cast<Floating>(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
	return (previousCoefficient*ComplexFloating(realPower*Floating(2)) - previousCombinedCoefficient + previousSquaredCoefficient*conj(constantCurrent))/ComplexFloating(magnitudeSquare);
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculatePowerError() const
{
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> voltages = stdToEigenVector(_voltages);
	Eigen::Matrix< ComplexFloating, Eigen::Dynamic, 1> currents = _admittances * voltages;
	std::vector<ComplexFloating> currentsConverted = eigenToStdVector(currents);
	std::vector<ComplexFloating> totalCurrents = subtract(currentsConverted, _constantCurrents);
	std::vector<ComplexFloating> powers = pointwiseMultiply(conjugate(totalCurrents), stdComplexVectorToComplexFloatingVector(_voltages));
	
	assert(_nodeCount == powers.size());
	double sum = 0;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		complex<double> currentPower = static_cast< complex<double> >(powers[_pqBuses[i].getId()]);
		complex<double> powerShouldBe = _pqBuses[i].getPower();
		sum += abs(currentPower - powerShouldBe);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		double currentPower = static_cast<double>(powers[_pvBuses[i].getId()].real());
		double powerShouldBe  = _pvBuses[i].getPowerReal();
		sum += abs(currentPower - powerShouldBe);
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateAbsolutePowerSum()
{
	complex<double> sum;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		complex<double> power = _pqBuses[i].getPower();
		sum += complex<double>(abs(power.real()), abs(power.imag()));
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		double power  = _pvBuses[i].getPowerReal();
		sum += complex<double>(abs(power), 0);
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
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::eigenToStdVector(const Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> &values)
{
	std::vector<ComplexFloating> result(values.size());

	for (int i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> Calculator<Floating, ComplexFloating>::stdToEigenVector(const std::vector<ComplexFloating> &values)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename Floating, typename ComplexFloating>
Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> Calculator<Floating, ComplexFloating>::stdToEigenVector(const std::vector< complex<double> > &values)
{
	Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = static_cast<ComplexFloating>(values[i]);

	return result;
}

template<typename Floating, typename ComplexFloating>
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::stdComplexVectorToComplexFloatingVector(const std::vector< complex<double> > &values)
{
	std::vector<ComplexFloating> result(values.size());
	
	for (size_t i = 0; i < values.size(); ++i)
		result[i] = static_cast<ComplexFloating>(values[i]);

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
	assert(_coefficientStorage != 0);
	return static_cast<double>(_coefficientStorage->getCoefficient(node, step).real());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getCoefficientImaginary(int step, int node) const
{
	assert(_coefficientStorage != 0);
	return static_cast<double>(_coefficientStorage->getCoefficient(node, step).imag());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getInverseCoefficientReal(int step, int node) const
{
	assert(_coefficientStorage != 0);
	return static_cast<double>(_coefficientStorage->getInverseCoefficient(node, step).real());
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getInverseCoefficientImaginary(int step, int node) const
{
	assert(_coefficientStorage != 0);
	return static_cast<double>(_coefficientStorage->getInverseCoefficient(node, step).imag());
}

template<typename Floating, typename ComplexFloating>
int Calculator<Floating, ComplexFloating>::getNodeCount() const
{
	return _nodeCount;
}