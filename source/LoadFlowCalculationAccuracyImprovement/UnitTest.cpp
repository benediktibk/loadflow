#include "UnitTest.h"
#include "Complex.h"
#include "CoefficientStorage.h"
#include <complex>

using namespace std;

bool areEqual(Complex<double> const& one, Complex<double> const& two, double delta)
{
	return	std::abs(one.real() - two.real()) < delta &&
			std::abs(one.imag() - two.imag()) < delta;
}

bool areEqual(complex<long double> const& one, complex<long double> const& two, double delta)
{
	return	std::abs(one.real() - two.real()) < delta &&
			std::abs(one.imag() - two.imag()) < delta;
}

bool areEqual(Complex<MultiPrecision> const& one, Complex<MultiPrecision> const& two, double delta)
{
	return	std::abs(one.real() - two.real()) < MultiPrecision(delta) &&
			std::abs(one.imag() - two.imag()) < MultiPrecision(delta);
}

bool runTestsComplexDouble()
{
	Complex<double> one(2, 3);
	Complex<double> two(5, 7);

	Complex<double> add = one + two;
	Complex<double> subtract = one - two;
	Complex<double> multiply = one * two;
	Complex<double> divide = one / two;

	Complex<double> addExpected(7, 10);
	Complex<double> subtractExpected(-3, -4);
	Complex<double> multiplyExpected(-11, 29);
	Complex<double> divideExpected(0.418918918, 0.013513513);

	if (!areEqual(addExpected, add, 0.0001))
		return false;
	if (!areEqual(subtractExpected, subtract, 0.0001))
		return false;
	if (!areEqual(multiplyExpected, multiply, 0.0001))
		return false;
	if (!areEqual(divideExpected, divide, 0.0001))
		return false;

	return true;
}

bool runTestsComplexMultiPrecision()
{
	Complex<MultiPrecision> one(MultiPrecision(2), MultiPrecision(3));
	Complex<MultiPrecision> two(MultiPrecision(5), MultiPrecision(7));

	Complex<MultiPrecision> add = one + two;
	Complex<MultiPrecision> subtract = one - two;
	Complex<MultiPrecision> multiply = one * two;
	Complex<MultiPrecision> divide = one / two;

	Complex<MultiPrecision> addExpected(MultiPrecision(7), MultiPrecision(10));
	Complex<MultiPrecision> subtractExpected(MultiPrecision(-3), MultiPrecision(-4));
	Complex<MultiPrecision> multiplyExpected(MultiPrecision(-11), MultiPrecision(29));
	Complex<MultiPrecision> divideExpected(MultiPrecision(0.418918918), MultiPrecision(0.013513513));

	if (!areEqual(addExpected, add, 0.0001))
		return false;
	if (!areEqual(subtractExpected, subtract, 0.0001))
		return false;
	if (!areEqual(multiplyExpected, multiply, 0.0001))
		return false;
	if (!areEqual(divideExpected, divide, 0.0001))
		return false;
	if (one == two)
		return false;
	if (!(one != two))
		return false;

	return true;
}

bool runTestsMultiPrecision()
{
	MultiPrecision one(-2.3);
	MultiPrecision two(5);

	if (one == two)
		return false;
	if (!(one != two))
		return false;
	if (one > two)
		return false;
	if (!(one < two))
		return false;
	if (!(two > one))
		return false;
	if (two < one)
		return false;

	MultiPrecision copyOne(one);

	if (one != copyOne)
		return false;

	copyOne = two;

	if (copyOne != two)
		return false;

	return true;
}

bool runTestsCoefficientStoragePQ()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, complex<double>()));
	vector<PVBus> pvBuses;
	CoefficientStorage< complex<long double>, long double> storage(10, 1, pqBuses, pvBuses);
	vector< complex<long double> > coefficients;

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients.push_back(complex<long double>(2, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(2, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(0.5, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;

	coefficients[0] = complex<long double>(3, 0);
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(3, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(-0.75, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;

	coefficients[0] = complex<long double>(5, 0);
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(5, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(complex<long double>((-1.0)/8, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;

	if (!areEqual(complex<long double>(2, 0), storage.getCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(0.5, 0), storage.getInverseCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(3, 0), storage.getCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(-0.75, 0), storage.getInverseCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(5, 0), storage.getCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(complex<long double>((-1.0)/8, 0), storage.getInverseCoefficient(0, 2), 0.000001))
		return false;

	return true;
}

bool runTestsCoefficientStoragePV()
{
	vector<PQBus> pqBuses;
	vector<PVBus> pvBuses;
	pvBuses.push_back(PVBus(0, 0, 1));
	CoefficientStorage< complex<long double>, long double> storage(10, 1, pqBuses, pvBuses);
	vector< complex<long double> > coefficients;

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients.push_back(complex<long double>(2, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(4, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;

	coefficients[0] = complex<long double>(3, 0);
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(12, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;

	coefficients[0] = complex<long double>(5, 0);
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(29, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;

	if (!areEqual(complex<long double>(4, 0), storage.getSquaredCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(12, 0), storage.getSquaredCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(29, 0), storage.getSquaredCoefficient(0, 2), 0.000001))
		return false;

	return true;
}

bool runTestsCoefficientStorage()
{
	if (!runTestsCoefficientStoragePQ())
		return false;

	if (!runTestsCoefficientStoragePV())
		return false;

	return true;
}

bool runTests()
{
	if (!runTestsComplexDouble())
		return false;

	if (!runTestsComplexMultiPrecision())
		return false;

	if (!runTestsMultiPrecision())
		return false;

	if (!runTestsCoefficientStorage())
		return false;

	return true;
}