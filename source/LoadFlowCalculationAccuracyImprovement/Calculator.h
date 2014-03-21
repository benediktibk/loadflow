#pragma once

#include <string>
#include <vector>
#include <complex>
#include <Eigen/Core>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>
#include "PQBus.h"
#include "PVBus.h"
#include "ConsoleOutput.h"
#include "ICalculator.h"
#include "CoefficientStorage.h"

template<typename Floating, typename ComplexFloating>
class Calculator : public ICalculator
{
public:
	Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);
	virtual ~Calculator();

	virtual void setAdmittance(int row, int column, std::complex<double> value);
	virtual void setPQBus(int busId, int node, std::complex<double> power);
	virtual void setPVBus(int busId, int node, double powerReal, double voltageMagnitude);
	virtual void setConstantCurrent(int node, std::complex<double> value);
	virtual void calculate();
	virtual double getVoltageReal(int node) const;
	virtual double getVoltageImaginary(int node) const;
	virtual double getCoefficientReal(int step, int node) const;
	virtual double getCoefficientImaginary(int step, int node) const;
	virtual double getInverseCoefficientReal(int step, int node) const;
	virtual double getInverseCoefficientImaginary(int step, int node) const;
	virtual int getNodeCount() const;
	virtual void setConsoleOutput(ConsoleOutput function);

private:
	void writeLine(const char *description, const Eigen::SparseMatrix<ComplexFloating> &matrix);
	void writeLine(const char *text);
	std::vector<ComplexFloating> solveAdmittanceEquationSystem(const std::vector<ComplexFloating> &rightHandSide);
	std::vector<ComplexFloating> calculateAdmittanceRowSum();
	void calculateFirstCoefficient(const std::vector<ComplexFloating> &admittanceRowSum);
	void calculateSecondCoefficient(const std::vector<ComplexFloating> &admittanceRowSum);
	void calculateNextCoefficient();
	double calculatePowerError() const;
	void calculateAbsolutePowerSum();

private:
	static std::vector<ComplexFloating> pointwiseMultiply(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two);
	static std::vector<ComplexFloating> pointwiseDivide(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two);
	static std::vector<ComplexFloating> add(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two);
	static std::vector<ComplexFloating> subtract(const std::vector<ComplexFloating> &one, const std::vector<ComplexFloating> &two);
	static std::vector<ComplexFloating> multiply(const std::vector<ComplexFloating> &one, const ComplexFloating &two);
	static std::vector<ComplexFloating> divide(const ComplexFloating &one, const std::vector<ComplexFloating> &two);
	static Floating findMaximumMagnitude(const std::vector<ComplexFloating> &values);
	static std::vector<ComplexFloating> eigenToStdVector(const Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> &values);
	static Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> stdToEigenVector(const std::vector<ComplexFloating> &values);
	static Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> stdToEigenVector(const std::vector< std::complex<double> > &values);
	static std::vector<ComplexFloating> stdComplexVectorToComplexFloatingVector(const std::vector< std::complex<double> > &values);
	static std::vector<ComplexFloating> conjugate(const std::vector<ComplexFloating> &values);

private:
	const double _targetPrecision;
	const size_t _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	Eigen::SparseLU<Eigen::SparseMatrix<ComplexFloating>, Eigen::NaturalOrdering<int> > _factorization;
	Eigen::SparseMatrix<ComplexFloating, Eigen::ColMajor > _admittances;
	std::vector<ComplexFloating> _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<double> > _voltages;
	ConsoleOutput _consoleOutput;
	double _absolutePowerSum;
	CoefficientStorage<ComplexFloating, Floating> *_coefficientStorage;
};

