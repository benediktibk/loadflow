#include "CalculatorMulti.h"
#include <assert.h>

CalculatorMulti::CalculatorMulti(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision, bool calculatePartialResults) :
	Calculator< MultiPrecision, Complex<MultiPrecision> >(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, calculatePartialResults),
	_bitPrecision(bitPrecision)
{
	assert(bitPrecision > 0);
	// unfortunately necessary
	MultiPrecision::setDefaultPrecision(bitPrecision);
}

MultiPrecision CalculatorMulti::createFloating(double value) const
{
	return MultiPrecision(value, _bitPrecision);
}
