#pragma once

#include "Calculator.h"
#include "MultiPrecision.h"
#include "Complex.h"

class CalculatorMulti :
	public Calculator< MultiPrecision, Complex<MultiPrecision> >
{
public:
	CalculatorMulti(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision);
	
protected:
	virtual MultiPrecision createFloating(double value) const;

private:
	const int _bitPrecision;
};

