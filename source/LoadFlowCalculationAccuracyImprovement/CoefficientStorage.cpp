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
		_squaredCoefficients.insert(pair<int, vector<ComplexType> >(pvBuses[i].getId(), vector<ComplexType>()));
		_pvBuses.push_back(pvBuses[i].getId());
	}
}

template<typename ComplexType, typename RealType>
void CoefficientStorage<ComplexType, RealType>::addCoefficients(std::vector<ComplexType> const &coefficients)
{
	_coefficients.push_back(coefficients);
	calculateNextInverseCoefficients();
	calculateNextSquaredCoefficients();
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