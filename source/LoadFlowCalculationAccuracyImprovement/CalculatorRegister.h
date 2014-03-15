#pragma once

#include "Calculator.h"
#include <map>
#include <mutex>

class CalculatorRegister
{
public:
	~CalculatorRegister();

	Calculator * const get(int id);
	int create(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount);
	void remove(int id);
	
private:
	std::map<int, Calculator*> _calculators;
	std::mutex _mutex;
};

