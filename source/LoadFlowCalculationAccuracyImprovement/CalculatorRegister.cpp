#include "CalculatorRegister.h"
#include "ConsoleOutput.h"
#include <limits>
#include <iostream>

using namespace std;

CalculatorRegister::~CalculatorRegister(void)
{
	lock_guard<mutex> lock(_mutex);

	for (map<int, ICalculator*>::iterator i = _calculators.begin(); i != _calculators.end(); ++i)
		delete i->second;
	_calculators.clear();
}

ICalculator * const CalculatorRegister::get(int id)
{
	lock_guard<mutex> lock(_mutex);

	return _calculators.at(id);
}

int CalculatorRegister::createCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount)
{
	lock_guard<mutex> lock(_mutex);

	cout << _calculators.size() << endl;

	for (int i = 0; i < numeric_limits<int>::max(); ++i)
		if (_calculators.count(i) == 0)
		{
			_calculators.insert(pair<int, ICalculator*>(i, new Calculator(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount)));
			return i;
		}

	return -1;
}

void CalculatorRegister::remove(int calculator)
{
	lock_guard<mutex> lock(_mutex);

	map<int, ICalculator*>::iterator iterator = _calculators.find(calculator);
	delete iterator->second;
	_calculators.erase(iterator);
}