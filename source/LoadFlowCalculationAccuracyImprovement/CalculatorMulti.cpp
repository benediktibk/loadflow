#include "CalculatorMulti.h"

CalculatorMulti::CalculatorMulti(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage) :
	Calculator< MultiPrecision, Complex<MultiPrecision> >(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage)
{ }

MultiPrecision CalculatorMulti::createFloating(double value) const
{
	return MultiPrecision(value);
}
