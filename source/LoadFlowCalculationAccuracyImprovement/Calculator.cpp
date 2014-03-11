#include "Calculator.h"

Calculator::Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount) :
	_targetPrecision(targetPrecision),
	_numberOfCoefficients(numberOfCoefficients),
	_nodeCount(nodeCount),
	_pqBusCount(pqBusCount),
	_pvBusCount(pvBusCount)
{ }

void Calculator::setAdmittanceReal(int row, int column, double value)
{

}

void Calculator::setAdmittanceImaginary(int row, int column, double value)
{

}

void Calculator::setPQBusPowerReal(int busId, int node, double value)
{

}

void Calculator::setPQBusPowerImaginary(int busId, int node, double value)
{

}

void Calculator::setPVBusPowerReal(int busId, int node, double value)
{

}

void Calculator::setPVBusVoltageMagnitude(int busId, int node, double value)
{

}

void Calculator::setConstantCurrentReal(int node, double value)
{

}

void Calculator::setConstantCurrentImaginary(int node, double value)
{

}

void Calculator::calculate()
{

}

double Calculator::getVoltageReal(int node) const
{
	return 0;
}

double Calculator::getVoltageImaginary(int node) const
{
	return 0;
}