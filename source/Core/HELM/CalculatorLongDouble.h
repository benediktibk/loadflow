#pragma once

#include "Calculator.h"

class CalculatorLongDouble :
	public Calculator< long double, std::complex<long double> >
{
public:
	CalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage);
	
protected:
	virtual long double createFloating(double value) const;
};

