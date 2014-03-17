#pragma once

#include "ICalculator.h"
#include "Calculator.h"
#include <map>
#include <mutex>

class CalculatorRegister
{
public:
	~CalculatorRegister();

	ICalculator * const get(int id);
	int createCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);
	void remove(int id);
	
private:
	std::map<int, ICalculator*> _calculators;
	std::mutex _mutex;
};

