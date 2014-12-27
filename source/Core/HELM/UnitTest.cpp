#include "UnitTest.h"
#include "Complex.h"
#include "CoefficientStorage.h"
#include "AnalyticContinuation.h"
#include "LinearEquationSystemSolver.h"
#include "Vector.h"
#include "SparseMatrix.h"
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

	return true;
}

bool runTestsCoefficientStoragePQ()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, complex<double>()));
	vector<PVBus> pvBuses;
	Matrix< complex<long double> > admittances(1, 1);
	CoefficientStorage< complex<long double>, long double> storage(10, 1, pqBuses, pvBuses, admittances);
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
	Matrix< complex<long double> > admittances(1, 1);
	CoefficientStorage< complex<long double>, long double> storage(10, 1, pqBuses, pvBuses, admittances);
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

bool runTestsCoefficientStorageMixed()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(1, complex<double>()));
	vector<PVBus> pvBuses;
	pvBuses.push_back(PVBus(0, 0, 0.7));
	Matrix< complex<long double> > admittances(2, 2);
	admittances.setValue(0, 0, complex<double>(100, 100));
	admittances.setValue(0, 1, complex<double>(-10, 1));
	admittances.setValue(1, 0, complex<double>(-10, 0));
	admittances.setValue(1, 1, complex<double>(20, 0));
	CoefficientStorage< complex<long double>, long double> storage(10, 2, pqBuses, pvBuses, admittances);
	vector< complex<long double> > coefficients(2);

	if (0 != storage.getCoefficientCount())
		return false;

	coefficients[0] = complex<long double>(2, 0);
	coefficients[1] = complex<long double>(0, 3);
	storage.addCoefficients(coefficients);
	coefficients[0] = complex<long double>(3, 0);
	coefficients[1] = complex<long double>(0, 7);
	storage.addCoefficients(coefficients);
	coefficients[0] = complex<long double>(5, 0);
	coefficients[1] = complex<long double>(1, 1);
	storage.addCoefficients(coefficients);

	if (!areEqual(complex<long double>(2, 0), storage.getCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(3, 0), storage.getCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(5, 0), storage.getCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(complex<long double>(0, 3), storage.getCoefficient(1, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(0, 7), storage.getCoefficient(1, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(1, 1), storage.getCoefficient(1, 2), 0.000001))
		return false;
	
	if (!areEqual(complex<long double>(4, 0), storage.getSquaredCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(12, 0), storage.getSquaredCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(29, 0), storage.getSquaredCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(complex<long double>(86, 22), storage.getCombinedCoefficient(0, 0), 0.000001))
		return false;
	if (!areEqual(complex<long double>(83, 493), storage.getCombinedCoefficient(0, 1), 0.000001))
		return false;
	if (!areEqual(complex<long double>(30, 1501), storage.getCombinedCoefficient(0, 2), 0.000001))
		return false;
	if (!areEqual(complex<long double>(30, 1501), storage.getLastCombinedCoefficient(0), 0.000001))
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
	pqBuses.push_back(PQBus(0, complex<double>()));
	vector<PVBus> pvBuses;
	Matrix< complex<long double> > admittances(1, 1);
	CoefficientStorage< complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, complex<long double> > continuation(coefficientStorage, 0, 6);
	vector< complex<long double> > coefficients(1);

	coefficients[0] = 0;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.5;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.5, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.0625;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.57142857, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.01660156;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.58510638, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.00473809;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.58574349, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.00137754;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.58578573, 0), continuation.getResult(), 0.00001))
		return false;

	return true;
}

bool runTestsAnalyticContinuationBunchAtOnce()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, complex<double>()));
	vector<PVBus> pvBuses;
	Matrix< complex<long double> > admittances(1, 1);
	CoefficientStorage< complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, complex<long double> > continuation(coefficientStorage, 0, 6);
	vector< complex<long double> > coefficients(1);

	coefficients[0] = 0;
	coefficientStorage.addCoefficients(coefficients);
	coefficients[0] = 0.5;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.5, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.0625;
	coefficientStorage.addCoefficients(coefficients);
	coefficients[0] = 0.01660156;
	coefficientStorage.addCoefficients(coefficients);
	coefficients[0] = 0.00473809;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.58574349, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients[0] = 0.00137754;
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(complex<double>(0.58578573, 0), continuation.getResult(), 0.00001))
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
	Matrix< complex<long double> > A(3, 3);
	A.setValue(0, 0, complex<long double>(1, 2));
	A.setValue(0, 1, complex<long double>(3, 4));
	A.setValue(0, 2, complex<long double>(5, 6));
	A.setValue(1, 1, complex<long double>(7, 8));
	A.setValue(1, 2, complex<long double>(9, 10));
	A.setValue(2, 0, complex<long double>(11, 12));
	A.setValue(2, 2, complex<long double>(13, 14));
	vector< complex<long double> > x;
	x.push_back(complex<long double>(15, 16));
	x.push_back(complex<long double>(17, 18));
	x.push_back(complex<long double>(19, 20));
	vector< complex<long double> > b = A.multiply(x);
	LinearEquationSystemSolver< complex<long double>, long double > solver(A, 1e-10);

	vector< complex<long double> > result = solver.solve(b);

	if (result.size() != 3)
		return false;
	if (!areEqual(x[0], result[0], 0.000001))
		return false;
	if (!areEqual(x[1], result[1], 0.000001))
		return false;
	if (!areEqual(x[2], result[2], 0.000001))
		return false;

	return true;
}

bool runTestsVectorConstructor()
{
	Vector<long double> a(3);

	if (a.getCount() != 3)
		return false;

	if (a(0) != 0)
		return false;

	if (a(1) != 0)
		return false;

	if (a(2) != 0)
		return false;

	return true;
}

bool runTestsVectorCopyConstructor()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);
	Vector<long double> b(a);

	if (b.getCount() != 3)
		return false;

	if (b(0) != 1)
		return false;

	if (b(1) != 2)
		return false;

	if (b(2) != 3)
		return false;

	return true;
}

bool runTestsVectorAssignment()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);
	Vector<long double> b(3);
	b = a;

	if (b.getCount() != 3)
		return false;

	if (b(0) != 1)
		return false;

	if (b(1) != 2)
		return false;

	if (b(2) != 3)
		return false;

	return true;
}

bool runTestsVectorDotProduct()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);
	Vector<long double> b(3);
	b.set(0, 4);
	b.set(1, 5);
	b.set(2, 6);

	auto result = a.dot(b);

	return result == 1*4 + 2*5 + 3*6;
}

bool runTestsVectorSquaredNorm()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);

	auto result = a.squaredNorm();

	return result == 1*1 + 2*2 + 3*3;
}

bool runTestsVectorWeightedSum()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);
	Vector<long double> b(3);
	b.set(0, 4);
	b.set(1, 5);
	b.set(2, 6);
	Vector<long double> result(3);

	result.weightedSum(a, 5, b);

	if (result(0) != 21)
		return false;

	if (result(1) != 27)
		return false;

	if (result(2) != 33)
		return false;

	return true;
}

bool runTestsVectorAddWeightedSum()
{
	Vector<long double> a(3);
	a.set(0, 1);
	a.set(1, 2);
	a.set(2, 3);
	Vector<long double> b(3);
	b.set(0, 4);
	b.set(1, 5);
	b.set(2, 6);
	Vector<long double> result(3);
	result.set(0, 7);
	result.set(1, 8);
	result.set(2, 9);

	result.addWeightedSum(10, a, 5, b);

	if (result(0) != 7 + 10*1 + 5*4)
		return false;

	if (result(1) != 8 + 10*2 + 5*5)
		return false;

	if (result(2) != 9 + 10*3 + 5*6)
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

	return true;
}

bool runTestsSparseMatrixConstructor()
{
	SparseMatrix<long double> matrix(4, 5);

	if (matrix.getRowCount() != 4)
		return false;

	if (matrix.getColumnCount() != 5)
		return false;

	auto value = matrix(0, 0);
	if (value != 0)
		return false;

	value = matrix(1, 2);
	if (value != 0)
		return false;

	value = matrix(3, 4);
	if (value != 0)
		return false;

	return true;
}

bool runTestsSparseMatrixSet()
{
	SparseMatrix<long double> matrix(3, 3);

	matrix.set(0, 0, 4);
	matrix.set(2, 2, 5);
	matrix.set(2, 1, 6);
	matrix.set(2, 2, 7);
	matrix.set(1, 0, 8);
	matrix.set(1, 1, 9);
	matrix.set(1, 2, 10);
	matrix.set(1, 1, 11);

	if (matrix(0, 0) != 4)
		return false;

	if (matrix(1, 0) != 8)
		return false;

	if (matrix(1, 1) != 11)
		return false;

	if (matrix(1, 2) != 10)
		return false;

	if (matrix(2, 1) != 6)
		return false;

	if (matrix(2, 2) != 7)
		return false;

	return true;
}

bool runTestsSparseMatrix()
{
	if (!runTestsSparseMatrixConstructor())
		return false;
	
	if (!runTestsSparseMatrixSet())
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

	if (!runTestsCoefficientStorage())
		return false;

	if (!runTestsAnalyticContinuation())
		return false;

	if (!runTestsLinearEquationSystem())
		return false;

	if (!runTestsVector())
		return false;

	if (!runTestsSparseMatrix())
		return false;

	return true;
}