#include "AnalyticContinuation.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <vector>
#include <assert.h>
#include <algorithm>

using namespace std;

template class AnalyticContinuation< long double, Complex<long double> >;
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
	assert(maximumNumberOfCoefficients > 0);
	assert(node >= 0);
}

template<typename Floating, typename ComplexFloating>
void AnalyticContinuation<Floating, ComplexFloating>::updateWithLastCoefficients()
{	
	assert(_alreadyProcessed < _maximumNumberOfCoefficients);
	
	while (_alreadyProcessed < _coefficients.getCoefficientCount())
		updateWithLastCoefficientsOnce();
}

template<typename Floating, typename ComplexFloating>
void AnalyticContinuation<Floating, ComplexFloating>::updateWithLastCoefficientsOnce()
{
	ComplexFloating const& newCoefficient = _coefficients.getCoefficient(_node, _alreadyProcessed);
	_next[0] = _current[0] + newCoefficient;

	if (_alreadyProcessed > 0)
	{
		_next[1] = calculateImprovement(_current[0], _next[0], ComplexFloating());

		for (auto i = 2; i <= _alreadyProcessed; ++i)
			_next[i] = calculateImprovement(_current[i - 1], _next[i - 1], _current[i - 2]);
	}

	swap(_next, _current);
	++_alreadyProcessed;
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
Complex<long double> AnalyticContinuation<Floating, ComplexFloating>::getResult() const
{	
	assert(_alreadyProcessed > 0);
	ComplexFloating const& result =  _alreadyProcessed % 2 == 0 ? _current[_alreadyProcessed - 2] : _current[_alreadyProcessed - 1];
	return Complex<long double>(static_cast<long double>(real(result)), static_cast<long double>(imag(result)));
}
