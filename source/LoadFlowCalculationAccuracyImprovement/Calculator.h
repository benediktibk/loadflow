#pragma once

#include <string>
#include <vector>
#include <complex>
#include <Eigen/Core>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>
#include <mpirxx.h>
#include "PQBus.h"
#include "PVBus.h"
#include "ConsoleOutput.h"
#include "MultiPrecision.h"

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
	double getCoefficientReal(int step, int node) const;
	double getCoefficientImaginary(int step, int node) const;
	double getInverseCoefficientReal(int step, int node) const;
	double getInverseCoefficientImaginary(int step, int node) const;
	int getNodeCount() const;
	void setConsoleOutput(ConsoleOutput function);

private:
	typedef long double floating;
	typedef std::complex<floating> complexFloating;

private:
	void writeLine(const char *description, const Eigen::SparseMatrix<complexFloating> &matrix);
	void writeLine(const char *text);
	std::vector<complexFloating> solveAdmittanceEquationSystem(const std::vector<complexFloating> &rightHandSide);
	std::vector<complexFloating> calculateAdmittanceRowSum();
	void calculateFirstCoefficient(const std::vector<complexFloating> &admittanceRowSum);
	void calculateSecondCoefficient(const std::vector<complexFloating> &admittanceRowSum);
	void calculateNextCoefficient();
	void calculateNextInverseCoefficient();
	void calculateVoltagesFromCoefficients();
	complexFloating calculateVoltageFromCoefficients(const std::vector<complexFloating> &coefficients);
	floating calculatePowerError() const;

private:
	static std::vector<complexFloating> pointwiseMultiply(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two);
	static std::vector<complexFloating> pointwiseDivide(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two);
	static std::vector<complexFloating> add(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two);
	static std::vector<complexFloating> subtract(const std::vector<complexFloating> &one, const std::vector<complexFloating> &two);
	static std::vector<complexFloating> multiply(const std::vector<complexFloating> &one, const complexFloating &two);
	static std::vector<complexFloating> divide(const complexFloating &one, const std::vector<complexFloating> &two);
	static floating findMaximumMagnitude(const std::vector<complexFloating> &values);
	static complexFloating converToComplexFloating(const std::complex<double> &value);
	static std::vector<complexFloating> ublasToStdVector(const Eigen::Matrix<complexFloating, Eigen::Dynamic, 1> &values);
	static Eigen::Matrix<complexFloating, Eigen::Dynamic, 1> stdToUblasVector(const std::vector<complexFloating> &values);
	static std::vector<complexFloating> conjugate(const std::vector<complexFloating> &values);
	static bool isValidFloating(floating value);

private:
	const floating _targetPrecision;
	const size_t _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	Eigen::SparseLU<Eigen::SparseMatrix<complexFloating>, Eigen::NaturalOrdering<int> > _factorization;
	Eigen::SparseMatrix<complexFloating, Eigen::ColMajor > _admittances;
	std::vector<complexFloating> _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector<complexFloating> _voltages;
	ConsoleOutput _consoleOutput;
	std::vector< std::vector<complexFloating> > _coefficients;
	std::vector< std::vector<complexFloating> > _inverseCoefficients;
	mpz_class _blub;
};

