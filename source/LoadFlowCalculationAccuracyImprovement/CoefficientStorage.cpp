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
CoefficientStorage<ComplexType, RealType>::CoefficientStorage(int maximumNumberOfCoefficients, int nodeCount, vector<PQBus> const& pqBuses, vector<PVBus> const &pvBuses) :
	_nodeCount(nodeCount)
{
	_coefficients.reserve(maximumNumberOfCoefficients);

	_pqBuses.reserve(pqBuses.size());
	for (size_t i = 0; i < pqBuses.size(); ++i)
	{
		_inverseCoefficients.insert(pair<int, vector<ComplexType> >(pqBuses[i].getId(), vector<ComplexType>()));
		_pqBuses.push_back(pqBuses[i].getId());
	}
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::addCoefficients(std::vector<ComplexType> const &coefficients)
{
	_coefficients.push_back(coefficients);
	calculateNextInverseCoefficients();
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getCoefficient(int node, int step) const
{
	return _coefficients[step][node];
}

template<typename ComplexType, typename RealType>
ComplexType const& CoefficientStorage<ComplexType, RealType>::getLastCoefficient(int node) const
{
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
	return getInverseCoefficient(node, _coefficients.size() - 1);
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
	size_t coefficientCount = _coefficients.size();
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
void CoefficientStorage<ComplexType, RealType>::calculateNextInverseCoefficients()
{
	if (_coefficients.size() == 0)
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

	int n = _coefficients.size() - 2;
	for (int i = 0; i <= n; ++i)
	{
		ComplexType const& coefficient = getCoefficient(node, i);
		ComplexType const& inverseCoefficient = getInverseCoefficient(node, n - i);
		result += coefficient*inverseCoefficient;
	}

	ComplexType const& firstCoefficient = getCoefficient(node, 0);
	result = result/firstCoefficient*ComplexType(-1);
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
void CoefficientStorage<ComplexType, RealType>::insertInverseCoefficient(int node, ComplexType const& value)
{
	assert(_inverseCoefficients.count(node) == 1);
	_inverseCoefficients[node].push_back(value);
}