#pragma once

#include "CoefficientStorage.h"
#include <complex>

template<typename Floating, typename ComplexFloating>
class AnalyticContinuation
{
public:
	AnalyticContinuation(CoefficientStorage<ComplexFloating, Floating> const& coefficients, int node, int maximumNumberOfCoefficients);

	void updateWithLastCoefficients();
	std::complex<double> const& getResult() const;

private:
	CoefficientStorage<ComplexFloating, Floating> const& _coefficients;
	const int _node;
	const int _maximumNumberOfCoefficients;
	std::complex<double> _result;
};

