#include "CalculatorRegister.h"
#include "ConsoleOutput.h"
#include <limits>
#include <iostream>

using namespace std;

CalculatorRegister::~CalculatorRegister(void)
{
	for (map<int, Calculator*>::iterator i = _calculators.begin(); i != _calculators.end(); ++i)
		delete i->second;
	_calculators.clear();
}

Calculator * const CalculatorRegister::get(int id) const
{
	return _calculators.at(id);
}

int CalculatorRegister::create(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount)
{
	cout << _calculators.size() << endl;

	for (int i = 0; i < numeric_limits<int>::max(); ++i)
		if (_calculators.count(i) == 0)
		{
			_calculators.insert(pair<int, Calculator*>(i, new Calculator(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount)));
			return i;
		}

	return -1;
}

void CalculatorRegister::remove(int calculator)
{
	map<int, Calculator*>::iterator iterator = _calculators.find(calculator);
	delete iterator->second;
	_calculators.erase(iterator);
}