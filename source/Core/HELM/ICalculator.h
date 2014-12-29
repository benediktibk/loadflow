#pragma once

#include "ConsoleOutput.h"
#include "Complex.h"

class ICalculator
{
public:
	virtual ~ICalculator() { };
	
	virtual void setAdmittance(int row, int column, Complex<long double> value) = 0;
	virtual void setAdmittanceRowSum(int row, Complex<long double> value) = 0;
	virtual void setPQBus(int busId, int node, Complex<long double> power) = 0;
	virtual void setPVBus(int busId, int node, double powerReal, double voltageMagnitude) = 0;
	virtual void setConstantCurrent(int node, Complex<long double> value) = 0;
	virtual void calculate() = 0;
	virtual double getVoltageReal(int node) const = 0;
	virtual double getVoltageImaginary(int node) const = 0;
	virtual double getCoefficientReal(int step, int node) const = 0;
	virtual double getCoefficientImaginary(int step, int node) const = 0;
	virtual double getInverseCoefficientReal(int step, int node) const = 0;
	virtual double getInverseCoefficientImaginary(int step, int node) const = 0;
	virtual int getNodeCount() const = 0;
	virtual double getProgress() = 0;
	virtual double getRelativePowerError() = 0;
	virtual int getMaximumPossibleCoefficientCount() = 0;
};