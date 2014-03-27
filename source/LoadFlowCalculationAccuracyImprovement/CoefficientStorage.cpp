#include "CoefficientStorage.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <complex>
#include <assert.h>
#include <stdexcept>

using namespace std;

template class CoefficientStorage< complex<long double>, long double >;
template class CoefficientStorage< Complex<MultiPrecision>, MultiPrecision >;

template<typename ComplexType, typename RealType>
CoefficientStorage<ComplexType, RealType>::CoefficientStorage(int maximumNumberOfCoefficients, int nodeCount, vector<PQBus> const& pqBuses, vector<PVBus> const &pvBuses, Eigen::SparseMatrix<ComplexType, Eigen::ColMajor > const& admittances) :
	_nodeCount(nodeCount),
	_admittances(admittances)
{
	assert(_admittances.rows() == _admittances.cols());
	assert(_admittances.rows() == nodeCount);
	assert(nodeCount == pqBuses.size() + pvBuses.size());
	_coefficients.reserve(maximumNumberOfCoefficients);

	_pqBuses.reserve(pqBuses.size());
	for (size_t i = 0; i < pqBuses.size(); ++i)
	{
		_inverseCoefficients.insert(pair<int, vector<ComplexType> >(pqBuses[i].getId(), vector<ComplexType>()));
		_pqBuses.push_back(pqBuses[i].getId());
	}

	_pvBuses.reserve(pvBuses.size());
	for (size_t i = 0; i < pvBuses.size(); ++i)
	{
		PVBus const& bus = pvBuses[i];
		int id = bus.getId();
		double voltageMagnitude = bus.getVoltageMagnitude();
		_squaredCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_combinedCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_weightedCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_pvBuses.push_back(id);
		_pvBusVoltageMagnitudeSquares.insert(pair<int, RealType>(id, RealType(voltageMagnitude*voltageMagnitude)));
	}
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::addCoefficients(std::vector<ComplexType> const &coefficients)
{
	_coefficients.push_back(coefficients);
	calculateNextInverseCoefficients();
	calculateNextSquaredCoefficients();
	calculateNextWeightedCoefficients();
	calculateNextCombinedCoefficients();
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getCoefficient(int node, int step) const
{
	return _coefficients[step][node];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getLastCoefficient(int node) const
{
	assert(_coefficients.size() > 0);
	return getCoefficient(node, _coefficients.size() - 1);
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getInverseCoefficient(int node, int step) const
{
	assert(_inverseCoefficients.count(node) == 1);
	return _inverseCoefficients.at(node)[step];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getLastInverseCoefficient(int node) const
{
	assert(_coefficients.size() > 0);
	return getInverseCoefficient(node, _coefficients.size() - 1);
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getSquaredCoefficient(int node, int step) const
{
	assert(_squaredCoefficients.count(node) == 1);
	return _squaredCoefficients.at(node)[step];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getLastSquaredCoefficient(int node) const
{
	assert(_coefficients.size() > 0);
	return getSquaredCoefficient(node, _coefficients.size() - 1);
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getWeightedCoefficient(int node, int step) const
{
	assert(_weightedCoefficients.count(node) == 1);
	return _weightedCoefficients.at(node)[step];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getCombinedCoefficient(int node, int step) const
{
	assert(_squaredCoefficients.count(node) == 1);
	return _combinedCoefficients.at(node)[step];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getLastCombinedCoefficient(int node) const
{
	assert(_coefficients.size() > 0);
	return getCombinedCoefficient(node, _coefficients.size() - 1);
}

template<typename ComplexType, typename RealType>
vector< complex<double> > CoefficientStorage<ComplexType, RealType>::calculateVoltagesFromCoefficients()
{
	vector< complex<double> > voltages(_nodeCount);

	for (int i = 0; i < _nodeCount; ++i)
		voltages[i] = calculateVoltageFromCoefficients(i);

	return voltages;
}

template<typename ComplexType, typename RealType>
complex<double> CoefficientStorage<ComplexType, RealType>::calculateVoltageFromCoefficients(int node)
{
	size_t coefficientCount = getCoefficientCount();
	vector<ComplexType> previousEpsilon(coefficientCount + 1);
	vector<ComplexType> currentEpsilon(coefficientCount);

	ComplexType sum;
	for (size_t i = 0; i < coefficientCount; ++i)
	{
		sum += getCoefficient(node, i);
		currentEpsilon[i] = sum;
	}

	while(currentEpsilon.size() > 1)
	{
		vector<ComplexType> nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
        {
            ComplexType previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			if (abs(previousDifference) == RealType(0))
				throw overflow_error("numeric error, would have to divide by zero");
			nextEpsilon[j] = previousEpsilon[j + 1] + ComplexType(RealType(1), RealType(0))/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	ComplexType const& result =  coefficientCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
	return static_cast< complex<double> >(result);
}

template<typename ComplexType, typename RealType>
size_t CoefficientStorage<ComplexType, RealType>::getCoefficientCount() const
{
	return _coefficients.size();
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextInverseCoefficients()
{
	if (_coefficients.size() == 1)
	{
		calculateFirstInverseCoefficients();
		return;
	}

	for (size_t i = 0; i < _pqBuses.size(); ++i)
		calculateNextInverseCoefficient(_pqBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextInverseCoefficient(int node)
{
	ComplexType result(0);

	int n = _coefficients.size() - 1;
	for (int i = 0; i < n; ++i)
	{
		ComplexType const& coefficient = getCoefficient(node, n - i);
		ComplexType const& inverseCoefficient = getInverseCoefficient(node, i);
		result += coefficient*inverseCoefficient;
	}

	ComplexType const& firstCoefficient = getCoefficient(node, 0);
	result = result/firstCoefficient*ComplexType(RealType(-1), RealType(0));
	insertInverseCoefficient(node, result);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateFirstInverseCoefficients()
{
	for (size_t i = 0; i < _pqBuses.size(); ++i)
	{
		int node = _pqBuses[i];
		ComplexType const& coefficient = getCoefficient(node, 0);
		ComplexType inverseCoefficient = ComplexType(1)/coefficient;
		insertInverseCoefficient(node, inverseCoefficient);
	}
}
template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextSquaredCoefficients()
{
	for (size_t i = 0; i < _pvBuses.size(); ++i)
		calculateNextSquaredCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextSquaredCoefficient(int node)
{
	int n = _coefficients.size() - 1;
	ComplexType coefficient;

	for (int j = 0; j <= n; ++j)
		coefficient += getCoefficient(node, j)*getCoefficient(node, n - j);

	insertSquaredCoefficient(node, coefficient);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextWeightedCoefficients()
{
	for (size_t i = 0; i < _pvBuses.size(); ++i)
		calculateNextWeightedCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextWeightedCoefficient(int node)
{
	ComplexType coefficient;
	
	for (int i = 0; i < _nodeCount; ++i)
	{
		if (i == node)
			continue;

		ComplexType admittance = _admittances.coeff(node, i);

		if (admittance == ComplexType(RealType(0), RealType(0)))
			continue;

		coefficient += conj(admittance*getLastCoefficient(i));
	}

	insertWeightedCoefficent(node, coefficient);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextCombinedCoefficients()
{
	for (size_t i = 0; i < _pvBuses.size(); ++i)
		calculateNextCombinedCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextCombinedCoefficient(int node)
{
	ComplexType result;
	int n = _coefficients.size() - 1;

	for (int i = 0; i <= n; ++i)
			result += getWeightedCoefficient(node, n - i)*getSquaredCoefficient(node, i);
	
	result += conj(_admittances.coeff(node, node))*getLastCoefficient(node)*ComplexType(_pvBusVoltageMagnitudeSquares[node]);
	insertCombinedCoefficient(node, result);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertInverseCoefficient(int node, ComplexType const& value)
{
	assert(_inverseCoefficients.count(node) == 1);
	_inverseCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertSquaredCoefficient(int node, ComplexType const& value)
{
	assert(_squaredCoefficients.count(node) == 1);
	_squaredCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertCombinedCoefficient(int node, ComplexType const& value)
{
	assert(_combinedCoefficients.count(node) == 1);
	_combinedCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertWeightedCoefficent(int node, ComplexType const& value)
{
	assert(_weightedCoefficients.count(node) == 1);
	_weightedCoefficients[node].push_back(value);
}