#pragma once

#include <complex>
#include "ConsoleOutput.h"

class ICalculator
{
public:
	virtual ~ICalculator() { };
	
	virtual void setAdmittance(int row, int column, std::complex<double> value) = 0;
	virtual void setAdmittanceRowSum(int row, std::complex<double> value) = 0;
	virtual void setPQBus(int busId, int node, std::complex<double> power) = 0;
	virtual void setPVBus(int busId, int node, double powerReal, double voltageMagnitude) = 0;
	virtual void setConstantCurrent(int node, std::complex<double> value) = 0;
	virtual void calculate() = 0;
	virtual double getVoltageReal(int node) const = 0;
	virtual double getVoltageImaginary(int node) const = 0;
	virtual double getCoefficientReal(int step, int node) const = 0;
	virtual double getCoefficientImaginary(int step, int node) const = 0;
	virtual double getInverseCoefficientReal(int step, int node) const = 0;
	virtual double getInverseCoefficientImaginary(int step, int node) const = 0;
	virtual int getNodeCount() const = 0;
	virtual void setConsoleOutput(ConsoleOutput function) = 0;
};