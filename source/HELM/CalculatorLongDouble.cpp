#include "CalculatorLongDouble.h"

CalculatorLongDouble::CalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, bool calculatePartialResults) :
	Calculator< long double, std::complex<long double> >(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, calculatePartialResults)
{ }

long double CalculatorLongDouble::createFloating(double value) const
{
	return static_cast<long double>(value);
}