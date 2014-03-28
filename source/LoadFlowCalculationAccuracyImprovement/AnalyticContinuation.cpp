#include "AnalyticContinuation.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include <vector>

using namespace std;

template class AnalyticContinuation< long double, complex<long double> >;
template class AnalyticContinuation< MultiPrecision, Complex<MultiPrecision> >;

template<typename Floating, typename ComplexFloating>
AnalyticContinuation<Floating, ComplexFloating>::AnalyticContinuation(CoefficientStorage<ComplexFloating, Floating> const& coefficients, int node, int maximumNumberOfCoefficients) :
	_coefficients(coefficients),
	_node(node),
	_maximumNumberOfCoefficients(maximumNumberOfCoefficients)
{ }

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
        {
            ComplexFloating previousDifference = currentEpsilon[j + 1] - currentEpsilon[j];
			if (abs(previousDifference) == Floating(0))
				throw overflow_error("numeric error, would have to divide by zero");
			nextEpsilon[j] = previousEpsilon[j + 1] + ComplexFloating(Floating(1))/previousDifference;
        }

		previousEpsilon = currentEpsilon;
		currentEpsilon = nextEpsilon;
	}

	ComplexFloating const& result =  coefficientCount % 2 == 0 ? previousEpsilon.back() : currentEpsilon.back();
	_result = static_cast< complex<double> >(result);
}

template<typename Floating, typename ComplexFloating>
std::complex<double> const& AnalyticContinuation<Floating, ComplexFloating>::getResult() const
{
	return _result;
}
