#pragma once

#include <string>
#include <vector>
#include <complex>
#include <mutex>
#include "PQBus.h"
#include "PVBus.h"
#include "ConsoleOutput.h"
#include "ICalculator.h"
#include "CoefficientStorage.h"
#include "AnalyticContinuation.h"
#include "Matrix.h"

template<typename Floating, typename ComplexFloating>
class Calculator : public ICalculator
{
public:
	Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage);
	virtual ~Calculator();

	virtual void setAdmittance(int row, int column, std::complex<double> value);
	virtual void setAdmittanceRowSum(int row, std::complex<double> value);
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
	virtual double getProgress();
	virtual void setConsoleOutput(ConsoleOutput function);

protected:
	virtual Floating createFloating(double value) const = 0;
	ComplexFloating createComplexFloating(std::complex<double> const &value) const;

private:
	void writeLine(const char *description, std::vector<ComplexFloating> const& values);
	void writeLine(const char *description, Eigen::SparseMatrix<ComplexFloating> const& matrix);
	void writeLine(const char *text);
	std::vector<ComplexFloating> solveAdmittanceEquationSystem(const std::vector<ComplexFloating> &rightHandSide);
	bool calculateFirstCoefficient();
	std::vector<ComplexFloating> calculateFirstCoefficientInternal();
	bool isPQCoefficientZero(std::vector<ComplexFloating> const& coefficients) const;
	void calculateSecondCoefficient();
	ComplexFloating calculateRightHandSide(PVBus const& bus);
	void calculateNextCoefficient();
	double calculatePowerError() const;
	double calculateVoltageError() const;
	double calculateTotalRelativeError() const;
	void freeMemory();
	void deleteContinuations();
	void calculateVoltagesFromCoefficients();

private:
	static Floating findMaximumMagnitude(const std::vector<ComplexFloating> &values);
	static std::vector<ComplexFloating> conjugate(const std::vector<ComplexFloating> &values);

private:
	const double _targetPrecision;
	const size_t _numberOfCoefficients;
	const size_t _nodeCount;
	const size_t _pqBusCount;
	const size_t _pvBusCount;
	const double _nominalVoltage;
	Eigen::BiCGSTAB<Eigen::SparseMatrix<ComplexFloating>, Eigen::DiagonalPreconditioner<ComplexFloating> > *_solver;
	Matrix<ComplexFloating> _admittances;
	std::vector<ComplexFloating> _totalAdmittanceRowSums;
	std::vector<ComplexFloating> _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< std::complex<double> > _voltages;
	ConsoleOutput _consoleOutput;
	CoefficientStorage<ComplexFloating, Floating> *_coefficientStorage;
	std::vector<AnalyticContinuation<Floating, ComplexFloating>*> _continuations;
	ComplexFloating _embeddingModification;
	std::mutex _progressMutex;
	double _progress;
};

