#include "CalculatorMulti.h"
#include <assert.h>

CalculatorMulti::CalculatorMulti(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision) :
	Calculator< MultiPrecision, Complex<MultiPrecision> >(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage),
	_bitPrecision(bitPrecision)
{
	assert(bitPrecision > 30);
}

MultiPrecision CalculatorMulti::createFloating(double value) const
{
	return MultiPrecision(value, _bitPrecision);
}
