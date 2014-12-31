#include "CalculatorRegister.h"
#include "ConsoleOutput.h"
#include "MultiPrecision.h"
#include "Complex.h"
#include "CalculatorLongDouble.h"
#include "CalculatorMulti.h"
#include <limits>

using namespace std;

CalculatorRegister::~CalculatorRegister(void)
{
	try
	{
		lock_guard<mutex> lock(_mutex);

		for (map<int, ICalculator*>::iterator i = _calculators.begin(); i != _calculators.end(); ++i)
			delete i->second;
	}
	catch(...)
	{ }

	_calculators.clear();
}

ICalculator& CalculatorRegister::get(int id)
{
	lock_guard<mutex> lock(_mutex);

	return *(_calculators.at(id));
}

int CalculatorRegister::createCalculatorLongDouble(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, bool iterativeSolver)
{
	lock_guard<mutex> lock(_mutex);

	int id = findEmptyId();

	if (id >= 0)
		_calculators.insert(pair<int, ICalculator*>(id, new CalculatorLongDouble(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, iterativeSolver)));

	return id;
}

int CalculatorRegister::createCalculatorMultiPrecision(double targetPrecision, int numberOfCoefficients, int nodeCount, int pqBusCount, int pvBusCount, double nominalVoltage, int bitPrecision, bool iterativeSolver)
{
	lock_guard<mutex> lock(_mutex);

	int id = findEmptyId();

	if (id >= 0)
		_calculators.insert(pair<int, ICalculator*>(id, new CalculatorMulti(targetPrecision, numberOfCoefficients, nodeCount, pqBusCount, pvBusCount, nominalVoltage, bitPrecision, iterativeSolver)));

	return id;
}

int CalculatorRegister::findEmptyId() const
{	
	for (int i = 0; i < numeric_limits<int>::max(); ++i)
		if (_calculators.count(i) == 0)
			return i;

	return -1;
}

void CalculatorRegister::remove(int calculator)
{
	lock_guard<mutex> lock(_mutex);

	map<int, ICalculator*>::iterator iterator = _calculators.find(calculator);
	delete iterator->second;
	_calculators.erase(iterator);
}