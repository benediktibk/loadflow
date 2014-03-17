#pragma once

#include "ICalculator.h"
#include "Calculator.h"
#include <map>
#include <mutex>

class CalculatorRegister
{
public:
	~CalculatorRegister();

	ICalculator& get(int id);
	int createCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);
	int createCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);
	void remove(int id);

private:
	int findEmptyId() const;
	
private:
	std::map<int, ICalculator*> _calculators;
	std::mutex _mutex;
};

