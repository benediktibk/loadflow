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
	static ComplexFloating calculateImprovement(ComplexFloating const& leftElement, ComplexFloating const& leftDownElement, ComplexFloating const& leftLeftDownElement);

private:
	CoefficientStorage<ComplexFloating, Floating> const& _coefficients;
	const int _node;
	const int _maximumNumberOfCoefficients;
	std::vector<ComplexFloating> _current;
	std::vector<ComplexFloating> _next;
	int _alreadyProcessed;
	std::complex<double> _result;
};

