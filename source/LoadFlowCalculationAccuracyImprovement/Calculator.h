#pragma once

#include <vector>
#include <complex>
#include <boost\numeric\ublas\matrix_sparse.hpp>
#include "PQBus.h"
#include "PVBus.h"
#include "ConsoleOutput.h"

class Calculator
{
public:
	Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);

	void setAdmittance(int row, int column, std::complex<double> value);
	void setPQBus(int busId, int node, std::complex<double> power);
	void setPVBus(int busId, int node, double powerReal, double voltageMagnitude);
	void setConstantCurrent(int node, std::complex<double> value);
	void calculate();
	double getVoltageReal(int node) const;
	double getVoltageImaginary(int node) const;
	void setConsoleOutput(ConsoleOutput function);

private:
	void writeLine(const char *text);

private:
	const double _targetPrecision;
	const int _numberOfCoefficients;
	const int _nodeCount;
	const int _pqBusCount;
	const int _pvBusCount;
	boost::numeric::ublas::mapped_matrix< std::complex<double> > _admittances;
	std::vector< std::complex<double> > _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<double> > _voltages;
	ConsoleOutput _consoleOutput;
};

