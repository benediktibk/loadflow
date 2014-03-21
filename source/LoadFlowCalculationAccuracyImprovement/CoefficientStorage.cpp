#include "CoefficientStorage.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <complex>
#include <assert.h>

using namespace std;

template class CoefficientStorage< complex<long double> >;
template class CoefficientStorage< Complex<MultiPrecision> >;

template<class T>
CoefficientStorage<T>::CoefficientStorage(int maximumNumberOfCoefficients, vector<PQBus> const& pqBuses, vector<PVBus> const &pvBuses)
{
	_coefficients.reserve(maximumNumberOfCoefficients);

	_pqBuses.reserve(pqBuses.size());
	for (size_t i = 0; i < pqBuses.size(); ++i)
	{
		_inverseCoefficients.insert(pair<int, vector<T> >(pqBuses[i].getId(), vector<T>()));
		_pqBuses.push_back(pqBuses[i].getId());
	}
}

template<class T>
void CoefficientStorage<T>::addCoefficients(std::vector<T> const &coefficients)
{
	_coefficients.push_back(coefficients);
	calculateNextInverseCoefficients();
}

template<class T>
T const& CoefficientStorage<T>::getCoefficient(int node, int step) const
{
	return _coefficients[step][node];
}

template<class T>
T const& CoefficientStorage<T>::getLastCoefficient(int node) const
{
	return getCoefficient(node, _coefficients.size() - 1);
}

template<class T>
T const& CoefficientStorage<T>::getInverseCoefficient(int node, int step) const
{
	assert(_inverseCoefficients.count(node) == 1);
	return _inverseCoefficients.at(node)[step];
}

template<class T>
T const& CoefficientStorage<T>::getLastInverseCoefficient(int node) const
{
	return getInverseCoefficient(node, _coefficients.size() - 1);
}

template<class T>
void CoefficientStorage<T>::calculateNextInverseCoefficients()
{
	if (_coefficients.size() == 0)
	{
		calculateFirstInverseCoefficients();
		return;
	}

	for (size_t i = 0; i < _pqBuses.size(); ++i)
		calculateNextInverseCoefficient(_pqBuses[i]);
}

template<class T>
void CoefficientStorage<T>::calculateNextInverseCoefficient(int node)
{
	T result(0);

	int n = _coefficients.size() - 2;
	for (int i = 0; i <= n; ++i)
	{
		T const& coefficient = getCoefficient(node, i);
		T const& inverseCoefficient = getInverseCoefficient(node, n - i);
		result += coefficient*inverseCoefficient;
	}

	T const& firstCoefficient = getCoefficient(node, 0);
	result = result/firstCoefficient*T(-1);
	insertInverseCoefficient(node, result);
}

template<class T>
void CoefficientStorage<T>::calculateFirstInverseCoefficients()
{
	for (size_t i = 0; i < _pqBuses.size(); ++i)
	{
		int node = _pqBuses[i];
		T const& coefficient = getCoefficient(node, 0);
		T inverseCoefficient = T(1)/coefficient;
		insertInverseCoefficient(node, inverseCoefficient);
	}
}

template<class T>
void CoefficientStorage<T>::insertInverseCoefficient(int node, T const& value)
{
	assert(_inverseCoefficients.count(node) == 1);
	_inverseCoefficients[node].push_back(value);
}