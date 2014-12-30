#include "UnitTest.h"
#include "Complex.h"
#include "CoefficientStorage.h"
#include "AnalyticContinuation.h"
#include "LinearEquationSystemSolver.h"
#include "Vector.h"
#include "SparseMatrix.h"
#include "MultiPrecision.h"

using namespace std;

bool areEqual(Complex<double> const& one, Complex<double> const& two, double delta)
{
	return	std::abs(one.real() - two.real()) < delta &&
			std::abs(one.imag() - two.imag()) < delta;
}

bool areEqual(Complex<long double> const& one, Complex<long double> const& two, double delta)
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
	Complex<double> zero;

	Complex<double> add = one + two;
	Complex<double> subtract = one - two;
	Complex<double> multiply = one * two;
	Complex<double> divide = one / two;

	Complex<double> addExpected(7, 10);
	Complex<double> subtractExpected(-3, -4);
	Complex<double> multiplyExpected(-11, 29);
	Complex<double> divideExpected(0.418918918, 0.013513513);

	if (zero.real() != 0)
		return false;
	if (zero.imag() != 0)
		return false;
	if (!areEqual(addExpected, add, 0.0001))
		return false;
	if (!areEqual(subtractExpected, subtract, 0.0001))
		return false;
	if (!areEqual(multiplyExpected, multiply, 0.0001))
		return false;
	if (!areEqual(divideExpected, divide, 0.0001))
		return false;

	if (Complex<double>(0, 1.0/3) == Complex<double>())
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

	Complex<MultiPrecision> onlyImaginaryValue(MultiPrecision(0), MultiPrecision(1.0/3));
	Complex<MultiPrecision> zero;

	if (onlyImaginaryValue == zero)
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

	MultiPrecision oneThird(1.0/3);
	MultiPrecision zero;

	if (oneThird == zero)
		return false;

	MultiPrecision::setDefaultPrecision(123);
	one = MultiPrecision(4);

	if (static_cast<double>(one) != 4)
		return false;

	return true;
}

bool runTestsCoefficientStoragePQ()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, Complex<long double>()));
	vector<PVBus> pvBuses;
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double> storage(10, 1, pqBuses, pvBuses, admittances);
	Vector<long double, Complex<long double> > coefficients(1);

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients.set(0, Complex<long double>(2, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(2, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(0.5, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;
	
	coefficients.set(0, Complex<long double>(3, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(3, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(-0.75, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;
	
	coefficients.set(0, Complex<long double>(5, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(5, 0), storage.getLastCoefficient(0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>((-1.0)/8, 0), storage.getLastInverseCoefficient(0), 0.000001))
		return false;

	if (!areEqual(Complex<long double>(2, 0), storage.getCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(0.5, 0), storage.getInverseCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(3, 0), storage.getCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(-0.75, 0), storage.getInverseCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(5, 0), storage.getCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(Complex<long double>((-1.0)/8, 0), storage.getInverseCoefficient(0, 2), 0.000001))
		return false;

	return true;
}

bool runTestsCoefficientStoragePV()
{
	vector<PQBus> pqBuses;
	vector<PVBus> pvBuses;
	pvBuses.push_back(PVBus(0, 0, 1));
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double> storage(10, 1, pqBuses, pvBuses, admittances);
	Vector<long double, Complex<long double> > coefficients(1);

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients.set(0, Complex<long double>(2, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(4, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;
	
	coefficients.set(0, Complex<long double>(3, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(12, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;
	
	coefficients.set(0, Complex<long double>(5, 0));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(29, 0), storage.getLastSquaredCoefficient(0), 0.000001))
		return false;

	if (!areEqual(Complex<long double>(4, 0), storage.getSquaredCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(12, 0), storage.getSquaredCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(29, 0), storage.getSquaredCoefficient(0, 2), 0.000001))
		return false;

	return true;
}

bool runTestsCoefficientStorageMixed()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(1, Complex<long double>()));
	vector<PVBus> pvBuses;
	pvBuses.push_back(PVBus(0, 0, 0.7));
	SparseMatrix<long double, Complex<long double> > admittances(2, 2);
	admittances.set(0, 0, Complex<long double>(100, 100));
	admittances.set(0, 1, Complex<long double>(-10, 1));
	admittances.set(1, 0, Complex<long double>(-10, 0));
	admittances.set(1, 1, Complex<long double>(20, 0));
	CoefficientStorage< Complex<long double>, long double> storage(10, 2, pqBuses, pvBuses, admittances);
	Vector<long double, Complex<long double> > coefficients(2);

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients.set(0, Complex<long double>(2, 0));
	coefficients.set(1, Complex<long double>(0, 3));
	storage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(3, 0));
	coefficients.set(1, Complex<long double>(0, 7));
	storage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(5, 0));
	coefficients.set(1, Complex<long double>(1, 1));
	storage.addCoefficients(coefficients);

	if (!areEqual(Complex<long double>(2, 0), storage.getCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(3, 0), storage.getCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(5, 0), storage.getCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(0, 3), storage.getCoefficient(1, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(0, 7), storage.getCoefficient(1, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(1, 1), storage.getCoefficient(1, 2), 0.000001))
		return false;
	
	if (!areEqual(Complex<long double>(4, 0), storage.getSquaredCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(12, 0), storage.getSquaredCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(29, 0), storage.getSquaredCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(86, 22), storage.getCombinedCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(83, 493), storage.getCombinedCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(30, 1501), storage.getCombinedCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(Complex<long double>(30, 1501), storage.getLastCombinedCoefficient(0), 0.000001))
		return false;

	return true;
}

bool runTestsCoefficientStorage()
{
	if (!runTestsCoefficientStoragePQ())
		return false;

	if (!runTestsCoefficientStoragePV())
		return false;

	if (!runTestsCoefficientStorageMixed())
		return false;

	return true;
}

bool runTestsAnalyticContinuationStepByStep()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, Complex<long double>()));
	vector<PVBus> pvBuses;
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, Complex<long double> > continuation(coefficientStorage, 0, 6);
	Vector<long double, Complex<long double> > coefficients(1);

	coefficients.set(0, Complex<long double>(0, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0, 0), continuation.getResult(), 0.00001))
		return false;
	
	coefficients.set(0, Complex<long double>(0.5, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.5, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.0625, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.57142857, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.01660156, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.58510638, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.00473809, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.58574349, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.00137754, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.58578573, 0), continuation.getResult(), 0.00001))
		return false;

	return true;
}

bool runTestsAnalyticContinuationBunchAtOnce()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, Complex<long double>()));
	vector<PVBus> pvBuses;
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, Complex<long double> > continuation(coefficientStorage, 0, 6);
	Vector<long double, Complex<long double> > coefficients(1);

	coefficients.set(0, Complex<long double>(0, 0));
	coefficientStorage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(0.5, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.5, 0), continuation.getResult(), 0.00001))
		return false;
	
	coefficients.set(0, Complex<long double>(0.0625, 0));
	coefficientStorage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(0.01660156, 0));
	coefficientStorage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(0.00473809, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.58574349, 0), continuation.getResult(), 0.00001))
		return false;
	
	coefficients.set(0, Complex<long double>(0.00137754, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.58578573, 0), continuation.getResult(), 0.00001))
		return false;

	return true;
}

bool runTestsAnalyticContinuation()
{
	if (!runTestsAnalyticContinuationStepByStep())
		return false;

	if (!runTestsAnalyticContinuationBunchAtOnce())
		return false;

	return true;
}

bool runTestsLinearEquationSystem()
{
	SparseMatrix<long double, Complex<long double> > A(3, 3);
	A.set(0, 0, Complex<long double>(1, 2));
	A.set(0, 1, Complex<long double>(3, 4));
	A.set(0, 2, Complex<long double>(5, 6));
	A.set(1, 1, Complex<long double>(7, 8));
	A.set(1, 2, Complex<long double>(9, 10));
	A.set(2, 0, Complex<long double>(11, 12));
	A.set(2, 2, Complex<long double>(13, 14));
	Vector<long double, Complex<long double> > x(3);
	x.set(0, Complex<long double>(15, 16));
	x.set(1, Complex<long double>(17, 18));
	x.set(2, Complex<long double>(19, 20));
	Vector<long double, Complex<long double> > b(3);
	A.multiply(b, x);
	LinearEquationSystemSolver< Complex<long double>, long double > solver(A, 1e-10);

	auto result = solver.solve(b);

	if (result.getCount() != 3)
		return false;
	if (!areEqual(x(0), result(0), 0.000001))
		return false;
	if (!areEqual(x(1), result(1), 0.000001))
		return false;
	if (!areEqual(x(2), result(2), 0.000001))
		return false;

	return true;
}

bool runTestsVectorConstructor()
{
	Vector<long double, Complex<long double> > a(3);

	if (a.getCount() != 3)
		return false;

	if (a(0) != Complex<long double>(0, 0))
		return false;

	if (a(1) != Complex<long double>(0, 0))
		return false;

	if (a(2) != Complex<long double>(0, 0))
		return false;

	return true;
}

bool runTestsVectorCopyConstructor()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(a);

	if (b.getCount() != 3)
		return false;

	if (b(0) != Complex<long double>(1, 0))
		return false;

	if (b(1) != Complex<long double>(2, 0))
		return false;

	if (b(2) != Complex<long double>(3, 0))
		return false;

	return true;
}

bool runTestsVectorAssignment()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b = a;

	if (b.getCount() != 3)
		return false;

	if (b(0) != Complex<long double>(1, 0))
		return false;

	if (b(1) != Complex<long double>(2, 0))
		return false;

	if (b(2) != Complex<long double>(3, 0))
		return false;

	return true;
}

bool runTestsVectorDotProduct()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b.set(0, Complex<long double>(4, 0));
	b.set(1, Complex<long double>(5, 0));
	b.set(2, Complex<long double>(6, 0));

	auto result = a.dot(b);

	return result == Complex<long double>(1*4 + 2*5 + 3*6, 0);
}

bool runTestsVectorSquaredNorm()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));

	auto result = a.squaredNorm();

	return result == Complex<long double>(1*1 + 2*2 + 3*3, 0);
}

bool runTestsVectorWeightedSum()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b.set(0, Complex<long double>(4, 0));
	b.set(1, Complex<long double>(5, 0));
	b.set(2, Complex<long double>(6, 0));
	Vector<long double, Complex<long double> > result(3);
	result.set(0, Complex<long double>(7, 0));
	result.set(1, Complex<long double>(8, 0));
	result.set(2, Complex<long double>(9, 0));

	result.weightedSum(a, Complex<long double>(5, 0), b);

	if (result(0) != Complex<long double>(21, 0))
		return false;

	if (result(1) != Complex<long double>(27, 0))
		return false;

	if (result(2) != Complex<long double>(33, 0))
		return false;

	return true;
}

bool runTestsVectorAddWeightedSum()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b.set(0, Complex<long double>(4, 0));
	b.set(1, Complex<long double>(5, 0));
	b.set(2, Complex<long double>(6, 0));
	Vector<long double, Complex<long double> > result(3);
	result.set(0, Complex<long double>(7, 0));
	result.set(1, Complex<long double>(8, 0));
	result.set(2, Complex<long double>(9, 0));

	result.addWeightedSum(Complex<long double>(10, 0), a, Complex<long double>(5, 0), b);

	if (result(0) != Complex<long double>(7 + 10*1 + 5*4, 0))
		return false;

	if (result(1) != Complex<long double>(8 + 10*2 + 5*5, 0))
		return false;

	if (result(2) != Complex<long double>(9 + 10*3 + 5*6, 0))
		return false;

	return true;
}

bool runTestsVectorPointwiseMultiply()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b.set(0, Complex<long double>(4, 0));
	b.set(1, Complex<long double>(5, 0));
	b.set(2, Complex<long double>(6, 0));
	Vector<long double, Complex<long double> > result(3);
	result.set(0, Complex<long double>(7, 0));
	result.set(1, Complex<long double>(8, 0));
	result.set(2, Complex<long double>(9, 0));

	result.pointwiseMultiply(a, b);

	if (result(0) != Complex<long double>(4, 0))
		return false;

	if (result(1) != Complex<long double>(10, 0))
		return false;

	if (result(2) != Complex<long double>(18, 0))
		return false;

	return true;
}

bool runTestsVectorSubtract()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > b(3);
	b.set(0, Complex<long double>(6, 0));
	b.set(1, Complex<long double>(5, 0));
	b.set(2, Complex<long double>(4, 0));
	Vector<long double, Complex<long double> > result(3);
	result.set(0, Complex<long double>(7, 0));
	result.set(1, Complex<long double>(8, 0));
	result.set(2, Complex<long double>(9, 0));

	result.subtract(a, b);

	if (result(0) != Complex<long double>(-5, 0))
		return false;

	if (result(1) != Complex<long double>(-3, 0))
		return false;

	if (result(2) != Complex<long double>(-1, 0))
		return false;

	return true;
}

bool runTestsVectorConjugate()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 2));
	a.set(1, Complex<long double>(-3, -4));
	a.set(2, Complex<long double>(5, 6));

	a.conjugate();

	if (a(0) != Complex<long double>(1, -2))
		return false;

	if (a(1) != Complex<long double>(-3, 4))
		return false;

	if (a(2) != Complex<long double>(5, -6))
		return false;

	return true;
}

bool runTestsVectorMultiPrecision()
{
	MultiPrecision::setDefaultPrecision(100);
	Vector<MultiPrecision, Complex<MultiPrecision> > a(1);
	
	a.set(0, Complex<MultiPrecision>(MultiPrecision(5), MultiPrecision(6)));

	if (static_cast<double>(a(0).real()) != 5)
		return false;

	if (static_cast<double>(a(0).imag()) != 6)
		return false;

	return true;
}

bool runTestsVector()
{
	if (!runTestsVectorConstructor())
		return false;

	if (!runTestsVectorCopyConstructor())
		return false;

	if (!runTestsVectorAssignment())
		return false;

	if (!runTestsVectorDotProduct())
		return false;

	if (!runTestsVectorSquaredNorm())
		return false;

	if (!runTestsVectorWeightedSum())
		return false;

	if (!runTestsVectorAddWeightedSum())
		return false;

	if (!runTestsVectorPointwiseMultiply())
		return false;

	if (!runTestsVectorSubtract())
		return false;

	if (!runTestsVectorConjugate())
		return false;

	if (!runTestsVectorMultiPrecision())
		return false;

	return true;
}

bool runTestsSparseMatrixConstructor()
{
	SparseMatrix<long double, Complex<long double> > matrix(4, 5);

	if (matrix.getRowCount() != 4)
		return false;

	if (matrix.getColumnCount() != 5)
		return false;

	auto value = matrix(0, 0);
	if (value != Complex<long double>(0, 0))
		return false;

	value = matrix(1, 2);
	if (value != Complex<long double>(0, 0))
		return false;

	value = matrix(3, 4);
	if (value != Complex<long double>(0, 0))
		return false;

	return true;
}

bool runTestsSparseMatrixGet()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 3);

	for (auto row = 0; row < 3; ++row)
		for (auto column = 0; column < 3; ++column)
			if (matrix(row, column) != Complex<long double>(0, 0))
				return false;

	return true;
}

bool runTestsSparseMatrixSet()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 3);

	matrix.set(0, 0, Complex<long double>(4, 0));
	matrix.set(2, 2, Complex<long double>(5, 0));
	matrix.set(2, 1, Complex<long double>(6, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(1, 0, Complex<long double>(8, 0));
	matrix.set(1, 1, Complex<long double>(9, 0));
	matrix.set(1, 2, Complex<long double>(10, 0));
	matrix.set(1, 1, Complex<long double>(11, 0));

	if (matrix(0, 0) != Complex<long double>(4, 0))
		return false;

	if (matrix(1, 0) != Complex<long double>(8, 0))
		return false;

	if (matrix(1, 1) != Complex<long double>(11, 0))
		return false;

	if (matrix(1, 2) != Complex<long double>(10, 0))
		return false;

	if (matrix(2, 1) != Complex<long double>(6, 0))
		return false;

	if (matrix(2, 2) != Complex<long double>(7, 0))
		return false;

	return true;
}

bool runTestsSparseMatrixMultiply()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	Vector<long double, Complex<long double> > result(3);
	Vector<long double, Complex<long double> > source(4);
	source.set(0, Complex<long double>(1, 0));
	source.set(1, Complex<long double>(2, 0));
	source.set(2, Complex<long double>(3, 0));
	source.set(3, Complex<long double>(4, 0));

	matrix.multiply(result, source);

	if (result(0) != Complex<long double>(2*2 + 3*3 + 4*4, 0))
		return false;

	if (result(1) != Complex<long double>(60*4 + 5*1, 0))
		return false;

	if (result(2) != Complex<long double>(7*3 + 80*1, 0))
		return false;

	return true;
}

bool runTestsSparseMatrixRowIteration()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	vector< Complex<long double> > values;
	vector<int> columns;
	vector<int> nonZeroCounts;

	for (auto row = 0; row < 3; ++row)
	{
		auto iterator = matrix.getRowIterator(row);
		nonZeroCounts.push_back(iterator.getNonZeroCount());

		for (auto i = iterator; i.isValid(); i.next())
		{
			values.push_back(i.getValue());
			columns.push_back(i.getColumn());
		}
	}

	vector< Complex<long double> > valuesShouldBe;
	vector<int> columnsShouldBe;
	vector<int> nonZeroCountsShouldBe;
	valuesShouldBe.push_back(Complex<long double>(2, 0));
	valuesShouldBe.push_back(Complex<long double>(3, 0));
	valuesShouldBe.push_back(Complex<long double>(4, 0));
	valuesShouldBe.push_back(Complex<long double>(5, 0));
	valuesShouldBe.push_back(Complex<long double>(60, 0));
	valuesShouldBe.push_back(Complex<long double>(80, 0));
	valuesShouldBe.push_back(Complex<long double>(7, 0));
	columnsShouldBe.push_back(1);
	columnsShouldBe.push_back(2);
	columnsShouldBe.push_back(3);
	columnsShouldBe.push_back(0);
	columnsShouldBe.push_back(3);
	columnsShouldBe.push_back(0);
	columnsShouldBe.push_back(2);
	nonZeroCountsShouldBe.push_back(3);
	nonZeroCountsShouldBe.push_back(2);
	nonZeroCountsShouldBe.push_back(2);

	if (columnsShouldBe != columns)
		return false;

	if (valuesShouldBe != values)
		return false;

	if (nonZeroCountsShouldBe != nonZeroCounts)
		return false;

	return true;
}

bool runTestsSparseMatrix()
{
	if (!runTestsSparseMatrixConstructor())
		return false;

	if (!runTestsSparseMatrixGet())
		return false;
	
	if (!runTestsSparseMatrixSet())
		return false;

	if (!runTestsSparseMatrixMultiply())
		return false;

	if (!runTestsSparseMatrixRowIteration())
		return false;

	return true;
}

bool runTests()
{
	if (!runTestsMultiPrecision())
		return false;

	if (!runTestsComplexDouble())
		return false;

	if (!runTestsComplexMultiPrecision())
		return false;

	if (!runTestsVector())
		return false;

	if (!runTestsSparseMatrix())
		return false;

	if (!runTestsCoefficientStorage())
		return false;

	if (!runTestsAnalyticContinuation())
		return false;

	if (!runTestsLinearEquationSystem())
		return false;

	return true;
}