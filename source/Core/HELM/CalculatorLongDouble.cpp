#include "CalculatorLongDouble.h"

CalculatorLongDouble::CalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage) :
	Calculator< long double, Complex<long double> >(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage)
{ }

long double CalculatorLongDouble::createFloating(double value) const
{
	return static_cast<long double>(value);
}