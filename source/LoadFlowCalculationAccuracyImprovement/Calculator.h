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
	double getCoefficientReal(int step, int node) const;
	double getCoefficientImaginary(int step, int node) const;
	double getInverseCoefficientReal(int step, int node) const;
	double getInverseCoefficientImaginary(int step, int node) const;
	int getNodeCount() const;
	void setConsoleOutput(ConsoleOutput function);

private:
	typedef long double floating;

private:
	void writeLine(const char *text, size_t argument);
	void writeLine(const boost::numeric::ublas::matrix< std::complex<double> > &matrix);
	void writeLine(const std::string &text);
	void writeLine(const char *text);
	std::vector< std::complex<floating> > solveAdmittanceEquationSystem(const std::vector< std::complex<floating> > &rightHandSide);
	std::vector< std::complex<floating> > calculateAdmittanceRowSum() const;
	void calculateFirstCoefficient(const std::vector< std::complex<floating> > &admittanceRowSum);
	void calculateSecondCoefficient(const std::vector< std::complex<floating> > &admittanceRowSum);
	void calculateNextCoefficient();
	void calculateNextInverseCoefficient();
	void calculateVoltagesFromCoefficients();
	std::complex<floating> calculateVoltageFromCoefficients(const std::vector< std::complex<floating> > &coefficients);

private:
	static std::vector< std::complex<floating> > pointwiseMultiply(const std::vector< std::complex<floating> > &one, const std::vector< std::complex<floating> > &two);
	static std::vector< std::complex<floating> > pointwiseDivide(const std::vector< std::complex<floating> > &one, const std::vector< std::complex<floating> > &two);
	static std::vector< std::complex<floating> > add(const std::vector< std::complex<floating> > &one, const std::vector< std::complex<floating> > &two);
	static std::vector< std::complex<floating> > subtract(const std::vector< std::complex<floating> > &one, const std::vector< std::complex<floating> > &two);
	static std::vector< std::complex<floating> > multiply(const std::vector< std::complex<floating> > &one, const std::complex<floating> &two);
	static std::vector< std::complex<floating> > divide(const std::complex<floating> &one, const std::vector< std::complex<floating> > &two);
	static floating findMaximumMagnitude(const std::vector< std::complex<floating> > &values);
	static std::complex<floating> converToComplexFloating(const std::complex<double> &value);

private:
	const floating _targetPrecision;
	const size_t _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	boost::numeric::ublas::matrix< std::complex<floating> > _admittances;
	boost::numeric::ublas::matrix< std::complex<floating> > _admittancesQ;
	boost::numeric::ublas::matrix< std::complex<floating> > _admittancesR;
	std::vector< std::complex<floating> > _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<floating> > _voltages;
	ConsoleOutput _consoleOutput;
	std::vector< std::vector< std::complex<floating> > > _coefficients;
	std::vector< std::vector< std::complex<floating> > > _inverseCoefficients;
};

