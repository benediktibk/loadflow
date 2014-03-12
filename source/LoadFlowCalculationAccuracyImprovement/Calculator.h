#pragma once

#include <vector>
#include <complex>
#include <boost\numeric\ublas\matrix.hpp>
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
	void InitializeAdmittanceFactorization();
	boost::numeric::ublas::vector< std::complex<double> > solveAdmittanceEquationSystem(const boost::numeric::ublas::vector< std::complex<double> > &rightHandSide);

private:
	const double _targetPrecision;
	const int _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	boost::numeric::ublas::matrix< std::complex<double> > _admittances;
	boost::numeric::ublas::matrix< std::complex<double> > _admittancesInverse;
	std::vector< std::complex<double> > _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<double> > _voltages;
	ConsoleOutput _consoleOutput;
};

