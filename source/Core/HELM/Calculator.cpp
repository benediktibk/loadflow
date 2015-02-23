#include "Calculator.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include "BiCGSTAB.h"
#include "SOR.h"
#include "LUDecomposition.h"
#include "NumericalTraits.h"
#include <sstream>
#include <map>
#include <cmath>
#include <stdexcept>
#include <assert.h>

using namespace std;

template class Calculator<long double, Complex<long double> >;
template class Calculator<MultiPrecision, Complex<MultiPrecision> >;

template<typename Floating, typename ComplexFloating>
Calculator<Floating, ComplexFloating>::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, bool iterativeSolver) :
	_targetPrecision(targetPrecision),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount),
	_nominalVoltage(nominalVoltage),
	_iterativeSolver(iterativeSolver),
	_admittances(nodeCount, nodeCount),
	_solver(0),
	_totalAdmittanceRowSums(nodeCount),
	_constantCurrents(nodeCount),
	_pqBuses(pqBusCount, PQBus()),
	_pvBuses(pvBusCount, PVBus()),
	_voltages(nodeCount),
	_coefficientStorage(0),
	_embeddingModification(Floating(0), Floating(0)),
	_progress(0),
	_relativePowerError(1),
	_maximumPossibleCoefficientCount(-1)
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
void Calculator<Floating, ComplexFloating>::setAdmittance(int row, int column, Complex<long double> value)
{
	auto valueCasted = createComplexFloating(value);
	assert(isValueFinite(std::abs2(valueCasted)));
	_admittances.set(row, column, valueCasted);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setAdmittanceRowSum(int row, Complex<long double> value)
{
	auto valueCasted = createComplexFloating(value);
	assert(isValueFinite(std::abs2(valueCasted)));
	_totalAdmittanceRowSums[row] = valueCasted;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setPQBus(int busId, int node, Complex<long double> power)
{
	assert(isValueFinite(std::abs2(power)));
	_pqBuses[busId] = PQBus(node, power);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setPVBus(int busId, int node, double powerReal, double voltageMagnitude)
{
	assert(isValueFinite(powerReal));
	assert(isValueFinite(voltageMagnitude));
	_pvBuses[busId] = PVBus(node, powerReal, voltageMagnitude);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::setConstantCurrent(int node, Complex<long double> value)
{
	auto valueCasted = createComplexFloating(value);
	assert(isValueFinite(std::abs2(valueCasted)));
	_constantCurrents.set(node, valueCasted);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculate()
{          
	assert(_constantCurrents.isFinite());

	{
		lock_guard<mutex> lock(_progressMutex);
		_progress = 0;
		_relativePowerError = 1;
		_maximumPossibleCoefficientCount = -1;
	}

	for (auto i = 0; i < _admittances.getColumnCount(); ++i)
		if (std::abs(_admittances(i, i)) == Floating(0))
			throw invalid_argument("zero values in the main diagonal of the admittance matrix are not supported");

	freeMemory();
	_coefficientStorage = new CoefficientStorage<ComplexFloating, Floating>(_numberOfCoefficients, _nodeCount, _pqBuses, _pvBuses, _admittances);
	if (_iterativeSolver)
		_solver = new BiCGSTAB<Floating, ComplexFloating>(_admittances, Floating(_targetPrecision*1e-10));
	else
		_solver = new LUDecomposition<Floating, ComplexFloating>(_admittances);

	for (auto i = 0; i < _nodeCount; ++i)
		_continuations.push_back(new AnalyticContinuation<Floating, ComplexFloating>(*_coefficientStorage, i, _numberOfCoefficients));
	
	try
	{	
		calculateFirstCoefficient();
		updateProgress(1);
		calculateSecondCoefficient();
		updateProgress(1);
	} 
	catch(exception)
	{
		lock_guard<mutex> lock(_progressMutex);
		_maximumPossibleCoefficientCount = _coefficientStorage->getCoefficientCount();
		return;
	}

	map<double, int> totalErrors;
	vector< vector< Complex<long double> > > partialResults;
	partialResults.reserve(_numberOfCoefficients);
	
	while (_coefficientStorage->getCoefficientCount() < _numberOfCoefficients)
	{
		try
		{
			calculateNextCoefficient();
			calculateVoltagesFromCoefficients();
		}
		catch(exception)
		{
			lock_guard<mutex> lock(_progressMutex);
			_maximumPossibleCoefficientCount = _coefficientStorage->getCoefficientCount();
			break;
		}

		double totalError = calculateTotalRelativeError();
		totalErrors.insert(pair<double, int>(totalError, partialResults.size()));
		partialResults.push_back(_voltages);
		updateProgress(totalErrors.begin()->first);
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
ComplexFloating Calculator<Floating, ComplexFloating>::createComplexFloating(Complex<long double> const &value) const
{
	return ComplexFloating(createFloating(value.real()), createFloating(value.imag()));
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateFirstCoefficient()
{
	auto coefficients = calculateFirstCoefficientInternal();
	auto modificationNecessary = isPQCoefficientZero(coefficients);

	if (!modificationNecessary)
	{		
		_coefficientStorage->addCoefficients(coefficients);
		return;
	}
	
	_embeddingModification = ComplexFloating(createFloating(1));
	coefficients = calculateFirstCoefficientInternal();
	modificationNecessary = isPQCoefficientZero(coefficients);

	if (modificationNecessary)
		throw exception("one modification was not enough");

	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
Vector<Floating, ComplexFloating> Calculator<Floating, ComplexFloating>::calculateFirstCoefficientInternal()
{
	Vector<Floating, ComplexFloating> rightHandSide(_nodeCount);
	
	#pragma omp parallel for
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		auto id = bus.getId();
		ComplexFloating const& constantCurrent = _constantCurrents(id);
		auto value = constantCurrent - (_totalAdmittanceRowSums[id] + _embeddingModification);
		assert(isValueFinite(std::abs2(value)));
		rightHandSide.set(id, value);
	}
	
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		auto id = bus.getId();
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[id];
		ComplexFloating const& constantCurrent = _constantCurrents(id);
		auto value = admittanceRowSum + constantCurrent;
		assert(isValueFinite(std::abs2(value)));
		rightHandSide.set(id, value);
	}

	return _solver->solve(rightHandSide);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateSecondCoefficient()
{
	Vector<Floating, ComplexFloating> rightHandSide(_nodeCount);
	
	#pragma omp parallel for
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		PQBus const& bus = _pqBuses[i];
		auto id = bus.getId();
		auto power = createComplexFloating(bus.getPower());
		auto current = conj(operator*(power, _coefficientStorage->getLastInverseCoefficient(id)));
		auto value = operator+(current, operator+(_totalAdmittanceRowSums[id], _embeddingModification));
		assert(isValueFinite(std::abs2(value)));
		rightHandSide.set(id, value);
	}
	
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		auto id = bus.getId();
		ComplexFloating const& admittanceRowSum = _totalAdmittanceRowSums[id];
		auto value = calculateRightHandSide(bus) - admittanceRowSum;
		assert(isValueFinite(std::abs2(value)));	
		rightHandSide.set(id, value);
	}
	
	auto coefficients = _solver->solve(rightHandSide);
	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateNextCoefficient()
{
	Vector<Floating, ComplexFloating> rightHandSide(_nodeCount);
			
	#pragma omp parallel for
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		const PQBus &bus = _pqBuses[i];
		auto id = bus.getId();
		auto power = createComplexFloating(bus.getPower());
		auto value = conj(operator*(power, _coefficientStorage->getLastInverseCoefficient(id)));
		assert(isValueFinite(std::abs2(value)));
		rightHandSide.set(id, value);
	}
		
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = _pvBuses[i];
		auto id = bus.getId();
		auto value = calculateRightHandSide(bus);
		assert(isValueFinite(std::abs2(value)));
		rightHandSide.set(id, value);
	}
	
	auto coefficients = _solver->solve(rightHandSide);
	_coefficientStorage->addCoefficients(coefficients);
}

template<typename Floating, typename ComplexFloating>
ComplexFloating Calculator<Floating, ComplexFloating>::calculateRightHandSide(PVBus const& bus)
{
	auto id = bus.getId();
	auto realPower = createFloating(bus.getPowerReal());
	ComplexFloating const& previousCoefficient = _coefficientStorage->getLastCoefficient(id);
	ComplexFloating const& previousCombinedCoefficient = _coefficientStorage->getLastCombinedCoefficient(id);
	ComplexFloating const& previousSquaredCoefficient = _coefficientStorage->getLastSquaredCoefficient(id);
	ComplexFloating const& constantCurrent = _constantCurrents(id);
	auto magnitudeSquare = createFloating(bus.getVoltageMagnitude()*bus.getVoltageMagnitude());
	return (previousCoefficient*ComplexFloating(realPower*createFloating(2)) - previousCombinedCoefficient + previousSquaredCoefficient*conj(constantCurrent))/ComplexFloating(magnitudeSquare);
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculatePowerError() const
{
	Vector<Floating, ComplexFloating> currents(_nodeCount);
	Vector<Floating, ComplexFloating> voltages(_nodeCount);
	getVoltagesAsVectorComplexFloating(voltages);
	_admittances.multiply(currents, voltages);
	currents.subtract(currents, _constantCurrents);
	currents.conjugate();
	Vector<Floating, ComplexFloating> powers(_nodeCount);
	powers.pointwiseMultiply(currents, voltages);	
	double sum = 0;
	
	#pragma omp parallel for reduction(+:sum)
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		auto currentPower = powers(_pqBuses[i].getId());
		auto currentPowerCasted = Complex<long double>(real(currentPower), imag(currentPower));
		auto powerShouldBe = _pqBuses[i].getPower();
		auto difference = currentPowerCasted - powerShouldBe;
		auto realDifferenceRelative = real(powerShouldBe) != 0 ? real(difference)/real(powerShouldBe) : real(difference);
		auto imaginaryDifferenceRelative = imag(powerShouldBe) != 0 ? imag(difference)/imag(powerShouldBe) : imag(difference);
		sum += abs(realDifferenceRelative) + abs(imaginaryDifferenceRelative);
	}
	
	#pragma omp parallel for reduction(+:sum)
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		auto currentPower = static_cast<long double>(real(powers(_pvBuses[i].getId())));
		auto powerShouldBe  = _pvBuses[i].getPowerReal();
		auto difference = currentPower - powerShouldBe;
		auto differenceRelative = powerShouldBe != 0 ? difference/powerShouldBe : difference;
		sum += abs(differenceRelative);
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculateVoltageError() const
{	
	double sum = 0;
	
	#pragma omp parallel for reduction(+:sum)
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		PVBus const &bus = _pvBuses[i];
		auto id = bus.getId();
		auto currentMagnitude = abs(_voltages[id]);
		auto magnitudeShouldBe = bus.getVoltageMagnitude();
		sum += abs((currentMagnitude - magnitudeShouldBe)/magnitudeShouldBe);
	}

	return sum;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::calculateTotalRelativeError() const
{	
	auto powerError = calculatePowerError();
	auto voltageError = calculateVoltageError();
	return powerError + voltageError;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::freeMemory()
{
	delete _coefficientStorage;
	_coefficientStorage = 0;
	delete _solver;
	_solver = 0;
	deleteContinuations();
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::deleteContinuations()
{
	for (auto i = 0; i < static_cast<int>(_continuations.size()); ++i)
		delete _continuations[i];
	_continuations.clear();
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::calculateVoltagesFromCoefficients()
{
	for (auto i = 0; i < _nodeCount; ++i)
	{
		_continuations[i]->updateWithLastCoefficients();
		_voltages[i] = _continuations[i]->getResult();
	}
}

template<typename Floating, typename ComplexFloating>
Floating Calculator<Floating, ComplexFloating>::findMaximumMagnitude(const vector<ComplexFloating> &values)
{
	assert(values.size() > 0);
	Floating result(0);

	for (auto i = 0; i < static_cast<int>(values.size()); ++i)
	{
		Floating magnitude = abs(values[i]);
		if (magnitude > result)
			result = magnitude;
	}

	return result;
}

template<typename Floating, typename ComplexFloating>
bool Calculator<Floating, ComplexFloating>::isPQCoefficientZero(Vector<Floating, ComplexFloating> const& coefficients) const
{
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		PQBus const& bus = _pqBuses[i];
		auto id = bus.getId();

		if (coefficients(id) == ComplexFloating())
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

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getProgress()
{
	lock_guard<mutex> lock(_progressMutex);
	return _progress;
}

template<typename Floating, typename ComplexFloating>
double Calculator<Floating, ComplexFloating>::getRelativePowerError()
{
	lock_guard<mutex> lock(_progressMutex);
	return _relativePowerError;
}

template<typename Floating, typename ComplexFloating>
int Calculator<Floating, ComplexFloating>::getMaximumPossibleCoefficientCount()
{
	lock_guard<mutex> lock(_progressMutex);
	return _maximumPossibleCoefficientCount;
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::getVoltagesAsVectorComplexFloating(Vector<Floating, ComplexFloating> &result) const
{
	int count = _voltages.size();

	#pragma omp parallel for
	for (auto i = 0; i < count; ++i)
	{
		Complex<long double> const &voltage = _voltages[i];
		result.set(i, ComplexFloating(Floating(real(voltage)), Floating(imag(voltage))));
	}
}

template<typename Floating, typename ComplexFloating>
void Calculator<Floating, ComplexFloating>::updateProgress(double relativePowerError)
{
	lock_guard<mutex> lock(_progressMutex);
	_relativePowerError = relativePowerError;	
	_progress = static_cast<double>(_coefficientStorage->getCoefficientCount()) / _numberOfCoefficients;
}