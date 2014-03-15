#pragma once

#include <string>
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
	void setAdmittanceQ(int row, int column, std::complex<double> value);
	void setAdmittanceR(int row, int column, std::complex<double> value);
	void setPQBus(int busId, int node, std::complex<double> power);
	void setPVBus(int busId, int node, double powerReal, double voltageMagnitude);
	void setConstantCurrent(int node, std::complex<double> value);
	void calculate();
	double getVoltageReal(int node) const;
	double getVoltageImaginary(int node) const;
	const std::complex<double>& getCoefficient(int step, int node) const;
	const std::complex<double>& getInverseCoefficient(int step, int node) const;
	int getNodeCount() const;
	void setConsoleOutput(ConsoleOutput function);

private:
	void writeLine(const char *text, size_t argument);
	void writeLine(const boost::numeric::ublas::matrix< std::complex<double> > &matrix);
	void writeLine(const std::string &text);
	void writeLine(const char *text);
	std::vector< std::complex<double> > solveAdmittanceEquationSystem(const std::vector< std::complex<double> > &rightHandSide);
	std::vector< std::complex<double> > calculateAdmittanceRowSum() const;
	void calculateFirstCoefficient(const std::vector< std::complex<double> > &admittanceRowSum);
	void calculateSecondCoefficient(const std::vector< std::complex<double> > &admittanceRowSum);
	void calculateNextCoefficient();
	void calculateNextInverseCoefficient();
	void calculateVoltagesFromCoefficients();
	std::complex<double> calculateVoltageFromCoefficients(const std::vector< std::complex<double> > &coefficients);

private:
	std::vector< std::complex<double> > pointwiseMultiply(const std::vector< std::complex<double> > &one, const std::vector< std::complex<double> > &two);
	std::vector< std::complex<double> > pointwiseDivide(const std::vector< std::complex<double> > &one, const std::vector< std::complex<double> > &two);
	std::vector< std::complex<double> > add(const std::vector< std::complex<double> > &one, const std::vector< std::complex<double> > &two);
	std::vector< std::complex<double> > multiply(const std::vector< std::complex<double> > &one, const std::complex<double> &two);
	std::vector< std::complex<double> > divide(const std::complex<double> &one, const std::vector< std::complex<double> > &two);

private:
	const double _targetPrecision;
	const size_t _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	boost::numeric::ublas::matrix< std::complex<double> > _admittances;
	boost::numeric::ublas::matrix< std::complex<double> > _admittancesQ;
	boost::numeric::ublas::matrix< std::complex<double> > _admittancesR;
	std::vector< std::complex<double> > _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<double> > _voltages;
	ConsoleOutput _consoleOutput;
	std::vector< std::vector< std::complex<double> > > _coefficients;
	std::vector< std::vector< std::complex<double> > > _inverseCoefficients;
};

