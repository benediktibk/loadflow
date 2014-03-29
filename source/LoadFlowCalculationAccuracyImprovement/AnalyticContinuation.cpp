#include "AnalyticContinuation.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <vector>
#include <assert.h>

using namespace std;

template class AnalyticContinuation< long double, complex<long double> >;
template class AnalyticContinuation< MultiPrecision, Complex<MultiPrecision> >;

template<typename Floating, typename ComplexFloating>
AnalyticContinuation<Floating, ComplexFloating>::AnalyticContinuation(CoefficientStorage<ComplexFloating, Floating> const& coefficients, int node, int maximumNumberOfCoefficients) :
	_coefficients(coefficients),
	_node(node),
	_maximumNumberOfCoefficients(maximumNumberOfCoefficients),
	_current(_maximumNumberOfCoefficients),
	_next(_maximumNumberOfCoefficients),
	_alreadyProcessed(0)
{
	assert(_maximumNumberOfCoefficients > 0);
	assert(_node >= 0);
}

template<typename Floating, typename ComplexFloating>
void AnalyticContinuation<Floating, ComplexFloating>::updateWithLastCoefficients()
{	
	size_t coefficientCount = _coefficients.getCoefficientCount();
	vector<ComplexFloating> previousEpsilon(coefficientCount + 1);
	vector<ComplexFloating> currentEpsilon(coefficientCount);

	ComplexFloating sum;
	for (size_t i = 0; i < coefficientCount; ++i)
	{
		sum += _coefficients.getCoefficient(_node, i);
		currentEpsilon[i] = sum;
	}

	while(currentEpsilon.size() > 1)
	{
		vector<ComplexFloating> nextEpsilon(currentEpsilon.size() - 1);

		for (size_t j = 0; j <= currentEpsilon.size() - 2; ++j)
			nextEpsilon[j] = calculateImprovement(currentEpsilon[j], currentEpsilon[j + 1], previousEpsilon[j + 1]);

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	ComplexFloating const& result =  coefficientCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
	_result = static_cast< complex<double> >(result);
}

template<typename Floating, typename ComplexFloating>
ComplexFloating AnalyticContinuation<Floating, ComplexFloating>::calculateImprovement(ComplexFloating const& leftElement, ComplexFloating const& leftDownElement, ComplexFloating const& leftLeftDownElement)
{
	ComplexFloating difference = leftDownElement - leftElement;
	if (abs(difference) == Floating(0))
		throw overflow_error("numeric error, would have to divide by zero");
	return leftLeftDownElement + ComplexFloating(Floating(1))/difference;
}

template<typename Floating, typename ComplexFloating>
std::complex<double> const& AnalyticContinuation<Floating, ComplexFloating>::getResult() const
{
	return _result;
}
