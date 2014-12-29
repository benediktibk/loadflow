#pragma once

#include <string>
#include <vector>
#include <mutex>
#include "PQBus.h"
#include "PVBus.h"
#include "ConsoleOutput.h"
#include "ICalculator.h"
#include "CoefficientStorage.h"
#include "AnalyticContinuation.h"
#include "SparseMatrix.h"
#include "LinearEquationSystemSolver.h"

template<typename Floating, typename ComplexFloating>
class Calculator : public ICalculator
{
public:
	Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage);
	virtual ~Calculator();

	virtual void setAdmittance(int row, int column, Complex<long double> value);
	virtual void setAdmittanceRowSum(int row, Complex<long double> value);
	virtual void setPQBus(int busId, int node, Complex<long double> power);
	virtual void setPVBus(int busId, int node, double powerReal, double voltageMagnitude);
	virtual void setConstantCurrent(int node, Complex<long double> value);
	virtual void calculate();
	virtual double getVoltageReal(int node) const;
	virtual double getVoltageImaginary(int node) const;
	virtual double getCoefficientReal(int step, int node) const;
	virtual double getCoefficientImaginary(int step, int node) const;
	virtual double getInverseCoefficientReal(int step, int node) const;
	virtual double getInverseCoefficientImaginary(int step, int node) const;
	virtual int getNodeCount() const;
	virtual double getProgress();
	virtual double getRelativePowerError();

protected:
	virtual Floating createFloating(double value) const = 0;
	ComplexFloating createComplexFloating(Complex<long double> const &value) const;

private:
	bool calculateFirstCoefficient();
	Vector<Floating, ComplexFloating> calculateFirstCoefficientInternal();
	bool isPQCoefficientZero(Vector<Floating, ComplexFloating> const& coefficients) const;
	void calculateSecondCoefficient();
	ComplexFloating calculateRightHandSide(PVBus const& bus);
	void calculateNextCoefficient();
	double calculatePowerError() const;
	double calculateVoltageError() const;
	double calculateTotalRelativeError() const;
	void freeMemory();
	void deleteContinuations();
	void calculateVoltagesFromCoefficients();
	void getVoltagesAsVectorComplexFloating(Vector<Floating, ComplexFloating> &result) const;

private:
	static Floating findMaximumMagnitude(const std::vector<ComplexFloating> &values);

private:
	const double _targetPrecision;
	const int _numberOfCoefficients;
	const int _nodeCount;
	const int _pqBusCount;
	const int _pvBusCount;
	const double _nominalVoltage;
	SparseMatrix<Floating, ComplexFloating> _admittances;
	LinearEquationSystemSolver<ComplexFloating, Floating> *_solver;
	std::vector<ComplexFloating> _totalAdmittanceRowSums;
	Vector<Floating, ComplexFloating> _constantCurrents;
	std::vector<PQBus> _pqBuses;
	std::vector<PVBus> _pvBuses;
	std::vector< Complex<long double> > _voltages;
	CoefficientStorage<ComplexFloating, Floating> *_coefficientStorage;
	std::vector<AnalyticContinuation<Floating, ComplexFloating>*> _continuations;
	ComplexFloating _embeddingModification;
	std::mutex _progressMutex;
	double _progress;
	double _relativePowerError;
};

