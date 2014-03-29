#pragma once

#include "CoefficientStorage.h"
#include <complex>

template<typename Floating, typename ComplexFloating>
class AnalyticContinuation
{
public:
	AnalyticContinuation(CoefficientStorage<ComplexFloating, Floating> const& coefficients, int node, int maximumNumberOfCoefficients);

	void updateWithLastCoefficients();
	std::complex<double> getResult() const;

private:
	void updateWithLastCoefficientsOnce();

private:
	static ComplexFloating calculateImprovement(ComplexFloating const& leftElement, ComplexFloating const& leftDownElement, ComplexFloating const& leftLeftDownElement);

private:
	CoefficientStorage<ComplexFloating, Floating> const& _coefficients;
	const size_t _node;
	const size_t _maximumNumberOfCoefficients;
	std::vector<ComplexFloating> _current;
	std::vector<ComplexFloating> _next;
	size_t _alreadyProcessed;
};

