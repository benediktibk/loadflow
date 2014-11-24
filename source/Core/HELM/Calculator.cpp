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
Calculator<Floating, ComplexFloating>::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage) :
	_targetPrecision(targetPrecision),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_nominalVoltage(nominalVoltage),
	_factorization(0),
	_admittances(nodeCount, nodeCount),
	_totalAdmittanceRowSums(nodeCount),
	_constantCurrents(nodeCount),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount),
	_consoleOutput(0),
	_coefficientStorage(0),
	_embeddingModification(0)
{ 
	assert(numberOfCoefficients > 0);
	assert(nodeCount > 0);
	assert(pqBusCount >= 0);
	assert(pvBusCount >= 0);
	assert(nominalVoltage > 0);
	_continuations.reserve(nodeCount);
}

template<typename Floating, typename ComplexFloating>
Calculator<Floating, ComplexFloating>::~Calculator()
{
	freeMemory();
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittance(int row, int column, complex<double> value)
{
	_admittances.setValue(row, column, createComplexFloating(value));
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittanceRowSum(int row, complex<double> value)
{
	_totalAdmittanceRowSums[row] = createComplexFloating(value);
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
	_constantCurrents[node] = createComplexFloating(value);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculate()
{          
	freeMemory();
	//_admittances.compress();
	_coefficientStorage = new CoefficientStorage<ComplexFloating, Floating>(_numberOfCoefficients, _nodeCount, _pqBuses, _pvBuses, _admittances);
	_factorization = new Decomposition<ComplexFloating>(_admittances);

	for (size_t i = 0; i < _nodeCount; ++i)
		_continuations.push_back(new AnalyticContinuation<Floating, ComplexFloating>(*_coefficientStorage, i, _numberOfCoefficients));
	
	if (!calculateFirstCoefficient())
		return;

	calculateSecondCoefficient();
	map<double, int> totalErrors;
	std::vector< std::vector< complex<double> > > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficientStorage->getCoefficientCount() < _numberOfCoefficients)
	{
		calculateNextCoefficient();

		try
		{
			calculateVoltagesFromCoefficients();
		}
		catch (overflow_error e)
		{
			writeLine("had to stop earlier because of numerical issues");
			break;
		}

		double totalError = calculateTotalRelativeError();
		totalErrors.insert(pair<double, int>(totalError, partialResults.size()));
		partialResults.push_back(_voltages);

		if (totalError < _targetPrecision)
		{
			writeLine("finished earlier because the total error is already small enough");
			break;
		}
	}

	if (!totalErrors.empty())
	{
		int bestResultIndex = totalErrors.begin()->second;
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
ComplexFloating Calculator<Floating, ComplexFloating>::createComplexFloating(complex<double> const &value) const
{
	return ComplexFloating(createFloating(value.real()), createFloating(value.imag()));
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::writeLine(const char *description, vector<ComplexFloating> const& values)
{
	assert(!values.empty());
	stringstream stream;
	stream << description << endl;

	stream << values.front();
	for (vector<ComplexFloating>::const_iterator i = values.begin() + 1; i != values.end(); ++i)
		stream << " - " << *i;

	writeLine(stream.str().c_str());
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::writeLine(const char *description, Eigen::SparseMatrix<ComplexFloating> const& matrix)
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
	return _factorization->solveEquationSystem(rightHandSide);
}

template<typename Floating, typename ComplexFloating>
bool Calculator<Floating, ComplexFloating>::calculateFirstCoefficient()
{
	vector<ComplexFloating> coefficients = calculateFirstCoefficientInternal();
	bool modificationNecessary = isPQCoefficientZero(coefficients);

	if (!modificationNecessary)
	{		
		_coefficientStorage->addCoefficients(coefficients);
		return true;
	}
	
	_embeddingModification = ComplexFloating(createFloating(1));
	coefficients = calculateFirstCoefficientInternal();
	modificationNecessary = isPQCoefficientZero(coefficients);

	if (modificationNecessary)
		return false;

	_coefficientStorage->addCoefficients(coefficients);
	return true;
}

template<typename Floating, typename ComplexFloating>
vector<ComplexFloating> Calculator<Floating, ComplexFloating>::calculateFirstCoefficientInternal()
{
	vector<ComplexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating const& constantCurrent = _constantCurrents[id];
		rightHandSide[id] = constantCurrent - (_totalAdmittanceRowSums[id] + _embeddingModification);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		int id = bus.getId();
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[id];
		ComplexFloating const& constantCurrent = _constantCurrents[id];
		rightHandSide[id] = admittanceRowSum + constantCurrent;
	}

	vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	return coefficients;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateSecondCoefficient()
{
	std::vector<ComplexFloating> rightHandSide(_nodeCount);

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		PQBus const& bus = _pqBuses[i];
		int id = bus.getId();
		ComplexFloating power = createComplexFloating(bus.getPower());
		ComplexFloating const& current = conj(power*_coefficientStorage->getLastInverseCoefficient(id));
		rightHandSide[id] = current + (_totalAdmittanceRowSums[id] + _embeddingModification);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		int id = bus.getId();
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[id];
		rightHandSide[id] = calculateRightHandSide(bus) - admittanceRowSum;
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
		ComplexFloating power = createComplexFloating(bus.getPower());
		rightHandSide[id] = conj(power*_coefficientStorage->getLastInverseCoefficient(id));
	}
		
	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		int id = bus.getId();
		rightHandSide[id] = calculateRightHandSide(bus);
	}
	
	std::vector<ComplexFloating> coefficients = solveAdmittanceEquationSystem(rightHandSide);
	assert(coefficients.size() == _nodeCount);
	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
ComplexFloating Calculator<Floating, ComplexFloating>::calculateRightHandSide(PVBus const& bus)
{
	int id = bus.getId();
	Floating realPower = createFloating(bus.getPowerReal());
	ComplexFloating const& previousCoefficient = _coefficientStorage->getLastCoefficient(id);
	ComplexFloating const& previousCombinedCoefficient = _coefficientStorage->getLastCombinedCoefficient(id);
	ComplexFloating const& previousSquaredCoefficient = _coefficientStorage->getLastSquaredCoefficient(id);
	ComplexFloating const& constantCurrent = _constantCurrents[id];
	Floating magnitudeSquare = createFloating(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
	return (previousCoefficient*ComplexFloating(realPower*createFloating(2)) - previousCombinedCoefficient + previousSquaredCoefficient*conj(constantCurrent))/ComplexFloating(magnitudeSquare);
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculatePowerError() const
{
	std::vector<ComplexFloating> currents = _admittances.multiply(Matrix<ComplexFloating>::stdComplexVectorToComplexFloatingVector(_voltages));
	std::vector<ComplexFloating> totalCurrents = Matrix<ComplexFloating>::subtract(currents, _constantCurrents);
	std::vector<ComplexFloating> powers = Matrix<ComplexFloating>::pointwiseMultiply(conjugate(totalCurrents), Matrix<ComplexFloating>::stdComplexVectorToComplexFloatingVector(_voltages));
	
	assert(_nodeCount == powers.size());
	double sum = 0;

	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		complex<double> currentPower = static_cast< complex<double> >(powers[_pqBuses[i].getId()]);
		complex<double> powerShouldBe = _pqBuses[i].getPower();
		complex<double> difference = currentPower - powerShouldBe;
		double realDifferenceRelative = powerShouldBe.real() != 0 ? difference.real()/powerShouldBe.real() : difference.real();
		double imaginaryDifferenceRelative = powerShouldBe.imag() != 0 ? difference.imag()/powerShouldBe.imag() : difference.imag();
		sum += abs(realDifferenceRelative) + abs(imaginaryDifferenceRelative);
	}

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		double currentPower = static_cast<double>(powers[_pvBuses[i].getId()].real());
		double powerShouldBe  = _pvBuses[i].getPowerReal();
		double difference = currentPower - powerShouldBe;
		double differenceRelative = powerShouldBe != 0 ? difference/powerShouldBe : difference;
		sum += abs(differenceRelative);
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculateVoltageError() const
{	
	double sum = 0;

	for (size_t i = 0; i < _pvBusCount; ++i)
	{
		PVBus const &bus = _pvBuses[i];
		int id = bus.getId();
		double currentMagnitude = abs(_voltages[id]);
		double magnitudeShouldBe = bus.getVoltageMagnitude();
		sum += abs((currentMagnitude - magnitudeShouldBe)/magnitudeShouldBe);
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculateTotalRelativeError() const
{	
	double powerError = calculatePowerError();
	double voltageError = calculateVoltageError();
	return powerError + voltageError;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::freeMemory()
{
	delete _coefficientStorage;
	_coefficientStorage = 0;
	delete _factorization;
	_factorization = 0;
	deleteContinuations();
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::deleteContinuations()
{
	for (size_t i = 0; i < _continuations.size(); ++i)
		delete _continuations[i];
	_continuations.clear();
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateVoltagesFromCoefficients()
{
	for (size_t i = 0; i < _nodeCount; ++i)
	{
		_continuations[i]->updateWithLastCoefficients();
		_voltages[i] = _continuations[i]->getResult();
	}
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
std::vector<ComplexFloating> Calculator<Floating, ComplexFloating>::conjugate(const std::vector<ComplexFloating> &values)
{
	std::vector<ComplexFloating> result(values.size());

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = conj(values[i]);

	return result;
}

template<typename Floating, typename ComplexFloating>
bool Calculator<Floating, ComplexFloating>::isPQCoefficientZero(vector<ComplexFloating> const& coefficients) const
{
	for (size_t i = 0; i < _pqBusCount; ++i)
	{
		PQBus const& bus = _pqBuses[i];
		int id = bus.getId();

		if (coefficients[id] == ComplexFloating())
			return true;
	}

	return false;
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