#include "CoefficientStorage.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include "NumericalTraits.h"
#include <assert.h>
#include <stdexcept>

using namespace std;

template class CoefficientStorage< Complex<long double>, long double >;
template class CoefficientStorage< Complex<MultiPrecision>, MultiPrecision >;

template<typename ComplexType, typename RealType>
CoefficientStorage<ComplexType, RealType>::CoefficientStorage(int maximumNumberOfCoefficients, int nodeCount, vector<PQBus> const& pqBuses, vector<PVBus> const &pvBuses, SparseMatrix<RealType, ComplexType> const& admittances) :
	_nodeCount(nodeCount),
	_pqBusCount(pqBuses.size()),
	_pvBusCount(pvBuses.size()),
	_admittances(admittances)
{
	assert(nodeCount ==_pqBusCount + _pvBusCount);
	_coefficients.reserve(maximumNumberOfCoefficients);

	_pqBuses.reserve(_pqBusCount);
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		auto id = pqBuses[i].getId();
		_inverseCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_pqBuses.push_back(id);
	}

	_pvBuses.reserve(_pvBusCount);
	for (auto i = 0; i < _pvBusCount; ++i)
	{
		PVBus const& bus = pvBuses[i];
		auto id = bus.getId();
		auto voltageMagnitude = bus.getVoltageMagnitude();
		_squaredCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_combinedCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_weightedCoefficients.insert(pair<int, vector<ComplexType> >(id, vector<ComplexType>()));
		_pvBuses.push_back(id);
		_pvBusVoltageMagnitudeSquares.insert(pair<int, RealType>(id, RealType(voltageMagnitude*voltageMagnitude)));
	}
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::addCoefficients(Vector<RealType, ComplexType> const &coefficients)
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
	return _coefficients[step](node);
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
int CoefficientStorage<ComplexType, RealType>::getCoefficientCount() const
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
	
	#pragma omp parallel for
	for (auto i = 0; i < _pqBusCount; ++i)
		calculateNextInverseCoefficient(_pqBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextInverseCoefficient(int node)
{
	ComplexType result(RealType(0));

	int n = _coefficients.size() - 1;
	for (auto i = 0; i < n; ++i)
	{
		ComplexType const& coefficient = getCoefficient(node, n - i);
		ComplexType const& inverseCoefficient = getInverseCoefficient(node, i);
		result += coefficient*inverseCoefficient;
	}

	ComplexType const& firstCoefficient = getCoefficient(node, 0);
	result = result/firstCoefficient*ComplexType(RealType(-1));
	insertInverseCoefficient(node, result);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateFirstInverseCoefficients()
{
	#pragma omp parallel for
	for (auto i = 0; i < _pqBusCount; ++i)
	{
		auto node = _pqBuses[i];
		ComplexType const& coefficient = getCoefficient(node, 0);
		auto inverseCoefficient = ComplexType(RealType(1))/coefficient;
		insertInverseCoefficient(node, inverseCoefficient);
	}
}
template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextSquaredCoefficients()
{
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
		calculateNextSquaredCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextSquaredCoefficient(int node)
{
	int n = _coefficients.size() - 1;
	ComplexType coefficient;

	for (auto j = 0; j <= n; ++j)
		coefficient += getCoefficient(node, j)*getCoefficient(node, n - j);

	insertSquaredCoefficient(node, coefficient);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextWeightedCoefficients()
{
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
		calculateNextWeightedCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextWeightedCoefficient(int node)
{
	ComplexType coefficient;
	
	for (auto i = _admittances.getRowIterator(node); i.isValid(); i.next())
	{
		auto column = i.getColumn();

		if (column == node)
			continue;

		coefficient += conj(i.getValue()*getLastCoefficient(column));
	}

	insertWeightedCoefficent(node, coefficient);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextCombinedCoefficients()
{
	#pragma omp parallel for
	for (auto i = 0; i < _pvBusCount; ++i)
		calculateNextCombinedCoefficient(_pvBuses[i]);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::calculateNextCombinedCoefficient(int node)
{
	ComplexType result;
	int n = _coefficients.size() - 1;

	for (auto i = 0; i <= n; ++i)
			result += getWeightedCoefficient(node, n - i)*getSquaredCoefficient(node, i);
	
	result += conj(_admittances(node, node))*getLastCoefficient(node)*ComplexType(_pvBusVoltageMagnitudeSquares[node]);
	insertCombinedCoefficient(node, result);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertInverseCoefficient(int node, ComplexType const& value)
{
	assert(_inverseCoefficients.count(node) == 1);
	assert(isValueFinite(std::abs2(value)));
	_inverseCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertSquaredCoefficient(int node, ComplexType const& value)
{
	assert(_squaredCoefficients.count(node) == 1);
	assert(isValueFinite(std::abs2(value)));
	_squaredCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertCombinedCoefficient(int node, ComplexType const& value)
{
	assert(_combinedCoefficients.count(node) == 1);
	assert(isValueFinite(std::abs2(value)));
	_combinedCoefficients[node].push_back(value);
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::insertWeightedCoefficent(int node, ComplexType const& value)
{
	assert(_weightedCoefficients.count(node) == 1);
	assert(isValueFinite(std::abs2(value)));
	_weightedCoefficients[node].push_back(value);
}