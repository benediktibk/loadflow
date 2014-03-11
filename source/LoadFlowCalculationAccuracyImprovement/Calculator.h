#pragma once

class Calculator
{
public:
	Calculator(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);

	void setAdmittanceReal(int row, int column, double value);
	void setAdmittanceImaginary(int row, int column, double value);
	void setPQBusPowerReal(int busId, int node, double value);
	void setPQBusPowerImaginary(int busId, int node, double value);
	void setPVBusPowerReal(int busId, int node, double value);
	void setPVBusVoltageMagnitude(int busId, int node, double value);
	void setConstantCurrentReal(int node, double value);
	void setConstantCurrentImaginary(int node, double value);
	void calculate();
	double getVoltageReal(int node) const;
	double getVoltageImaginary(int node) const;

private:
	const double _targetPrecision;
	const int _numberOfCoefficients;
	const int _nodeCount;
	const int _pqBusCount;
	const int _pvBusCount;
};

