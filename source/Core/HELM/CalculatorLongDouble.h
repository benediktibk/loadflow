#pragma once

#include "Calculator.h"

class CalculatorLongDouble :
	public Calculator< long double, Complex<long double> >
{
public:
	CalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, bool iterativeSolver);
	
public:
	virtual long double createFloating(double value) const;
};

