#pragma once

#include "CoefficientStorage.h"

template<typename Floating, typename ComplexFloating>
class AnalyticContinuation
{
public:
	AnalyticContinuation(CoefficientStorage<ComplexFloating, Floating> const& coefficients, int node, int maximumNumberOfCoefficients);

	void updateWithLastCoefficients();
	Complex<long double> getResult() const;

private:
	void updateWithLastCoefficientsOnce();

private:
	static ComplexFloating calculateImprovement(ComplexFloating const& leftElement, ComplexFloating const& leftDownElement, ComplexFloating const& leftLeftDownElement);

private:
	CoefficientStorage<ComplexFloating, Floating> const& _coefficients;
	const int _node;
	const int _maximumNumberOfCoefficients;
	std::vector<ComplexFloating> _current;
	std::vector<ComplexFloating> _next;
	int _alreadyProcessed;
};

