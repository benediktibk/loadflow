#include "UnitTest.h"
#include "Complex.h"
#include "CoefficientStorage.h"
#include "AnalyticContinuation.h"
#include "ILinearEquationSystemSolver.h"
#include "BiCGSTAB.h"
#include "LUDecompositionStable.h"
#include "LUDecompositionSparse.h"
#include "SOR.h"
#include "Vector.h"
#include "SparseMatrix.h"
#include "MultiPrecision.h"
#include "NumericalTraits.h"
#include "Graph.h"
#include <sstream>
#include <fstream>
#include <algorithm>
#include <chrono>

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

bool areEqual(Vector<long double, Complex<long double>> const& one, Vector<long double, Complex<long double>> const& two, double delta)
{
	if (one.getCount() != two.getCount())
		return false;

	for (auto i = 0; i < one.getCount(); ++i)
		if (!areEqual(one(i), two(i), delta))
			return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsComplexDouble()
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

	Complex<double> original(1.23456, 0.00000006789);
	Complex<double> parsed;
	stringstream stream;
	stream << original;
	stream.seekg(ios_base::beg);
	stream >> parsed;

	if (original != parsed)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsComplexMultiPrecision()
{
	Complex<MultiPrecision> one(MultiPrecision(2), MultiPrecision(3));
	Complex<MultiPrecision> two(MultiPrecision(5), MultiPrecision(7));
	Complex<long double> zeroLongDouble(0, 0);
	Complex<MultiPrecision> zeroMulti(MultiPrecision(zeroLongDouble.real()), MultiPrecision(zeroLongDouble.imag()));

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

	if (!isValueFinite(std::abs2(zero)))
		return false;

	if (!isValueFinite(std::abs2(zeroMulti)))
		return false;

	if (zero != zeroMulti)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsMultiPrecision()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsCoefficientStoragePQ()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsCoefficientStoragePV()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsCoefficientStorageMixed()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsAnalyticContinuationStepByStep()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, Complex<long double>()));
	vector<PVBus> pvBuses;
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, Complex<long double> > continuation(coefficientStorage, 0, 6);
	Vector<long double, Complex<long double> > coefficients(1);
	
	coefficients.set(0, Complex<long double>(0.5, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	if (!areEqual(Complex<long double>(0.5, 0), continuation.getResult(), 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.0625, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	auto result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.56250000000000000, 0), result, 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.01660156, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.58510637834314194, 0), result, 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.00473809, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.58573197128947940, 0), result, 0.00001))
		return false;

	coefficients.set(0, Complex<long double>(0.00137754, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.58578574861861932, 0), result, 0.00001))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsAnalyticContinuationBunchAtOnce()
{
	vector<PQBus> pqBuses;
	pqBuses.push_back(PQBus(0, Complex<long double>()));
	vector<PVBus> pvBuses;
	SparseMatrix<long double, Complex<long double> > admittances(1, 1);
	CoefficientStorage< Complex<long double>, long double > coefficientStorage(6, 1, pqBuses, pvBuses, admittances);
	AnalyticContinuation< long double, Complex<long double> > continuation(coefficientStorage, 0, 6);
	Vector<long double, Complex<long double> > coefficients(1);

	coefficients.set(0, Complex<long double>(0.5, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	auto result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.5, 0), result, 0.00001))
		return false;
	
	coefficients.set(0, Complex<long double>(0.0625, 0));
	coefficientStorage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(0.01660156, 0));
	coefficientStorage.addCoefficients(coefficients);
	coefficients.set(0, Complex<long double>(0.00473809, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.58573197128947940, 0), result, 0.00001))
		return false;
	
	coefficients.set(0, Complex<long double>(0.00137754, 0));
	coefficientStorage.addCoefficients(coefficients);
	continuation.updateWithLastCoefficients();
	result = continuation.getResult();
	if (!areEqual(Complex<long double>(0.58578574861861932, 0), result, 0.00001))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemOne()
{
	SparseMatrix<long double, Complex<long double>> A(3, 3);
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
	BiCGSTAB<long double, Complex<long double>> iterativeSolver(A, 1e-10);
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);
	LUDecompositionSparse<long double, Complex<long double>> luSolverSparse(A);

	auto iterativeResult = iterativeSolver.solve(b);
	auto luResult = luSolver.solve(b);
	auto luResultSparse = luSolverSparse.solve(b);

	if (iterativeResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), iterativeResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), iterativeResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), iterativeResult(2), 0.000001))
		return false;

	if (luResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResult(2), 0.000001))
		return false;

	if (luResultSparse.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResultSparse(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResultSparse(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResultSparse(2), 0.000001))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemTwo()
{
	SparseMatrix<long double, Complex<long double>> A(3, 3);
	A.set(0, 0, Complex<long double>(1, 0));
	A.set(0, 1, Complex<long double>(2, 0));
	A.set(0, 2, Complex<long double>(3, 0));
	A.set(1, 0, Complex<long double>(4, 0));
	A.set(1, 1, Complex<long double>(5, 0));
	A.set(1, 2, Complex<long double>(6, 0));
	A.set(2, 0, Complex<long double>(7, 0));
	A.set(2, 1, Complex<long double>(8, 0));
	A.set(2, 2, Complex<long double>(10, 0));
	Vector<long double, Complex<long double> > x(3);
	x.set(0, Complex<long double>(10, 0));
	x.set(1, Complex<long double>(11, 0));
	x.set(2, Complex<long double>(12, 0));
	Vector<long double, Complex<long double> > b(3);
	A.multiply(b, x);
	BiCGSTAB<long double, Complex<long double>> iterativeSolver(A, 1e-10);
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);
	LUDecompositionSparse<long double, Complex<long double>> luSolverSparse(A);

	auto iterativeResult = iterativeSolver.solve(b);
	auto luResult = luSolver.solve(b);
	auto luResultSparse = luSolverSparse.solve(b);

	if (iterativeResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), iterativeResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), iterativeResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), iterativeResult(2), 0.000001))
		return false;

	if (luResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResult(2), 0.000001))
		return false;

	if (luResultSparse.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResultSparse(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResultSparse(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResultSparse(2), 0.000001))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemThree()
{
	SparseMatrix<long double, Complex<long double>> A(3, 3);
	A.set(0, 0, Complex<long double>(4, 0));
	A.set(0, 1, Complex<long double>(-1, 0));
	A.set(0, 2, Complex<long double>(-1, 0));
	A.set(1, 0, Complex<long double>(-2, 0));
	A.set(1, 1, Complex<long double>(6, 0));
	A.set(1, 2, Complex<long double>(1, 0));
	A.set(2, 0, Complex<long double>(-1, 0));
	A.set(2, 1, Complex<long double>(1, 0));
	A.set(2, 2, Complex<long double>(7, 0));
	Vector<long double, Complex<long double> > x(3);
	x.set(0, Complex<long double>(1, 0));
	x.set(1, Complex<long double>(2, 0));
	x.set(2, Complex<long double>(-1, 0));
	Vector<long double, Complex<long double> > b(3);
	A.multiply(b, x);
	BiCGSTAB<long double, Complex<long double>> iterativeSolver(A, 1e-10);
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);
	LUDecompositionSparse<long double, Complex<long double>> luSolverSparse(A);
	SOR<long double, Complex<long double>> sorSolver(A, 1e-10, 1, 100);
	SOR<long double, Complex<long double>> sorSolverOver(A, 1e-10, 1.2, 100);
	SOR<long double, Complex<long double>> sorSolverUnder(A, 1e-10, 0.8, 100);

	auto iterativeResult = iterativeSolver.solve(b);
	auto luResult = luSolver.solve(b);
	auto luResultSparse = luSolverSparse.solve(b);
	auto sorResult = sorSolver.solve(b);
	auto sorResultOver = sorSolverOver.solve(b);
	auto sorResultUnder = sorSolverUnder.solve(b);

	if (iterativeResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), iterativeResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), iterativeResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), iterativeResult(2), 0.000001))
		return false;

	if (luResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResult(2), 0.000001))
		return false;

	if (luResultSparse.getCount() != 3)
		return false;
	if (!areEqual(x(0), luResultSparse(0), 0.000001))
		return false;
	if (!areEqual(x(1), luResultSparse(1), 0.000001))
		return false;
	if (!areEqual(x(2), luResultSparse(2), 0.000001))
		return false;

	if (sorResult.getCount() != 3)
		return false;
	if (!areEqual(x(0), sorResult(0), 0.000001))
		return false;
	if (!areEqual(x(1), sorResult(1), 0.000001))
		return false;
	if (!areEqual(x(2), sorResult(2), 0.000001))
		return false;

	if (sorResultOver.getCount() != 3)
		return false;
	if (!areEqual(x(0), sorResultOver(0), 0.000001))
		return false;
	if (!areEqual(x(1), sorResultOver(1), 0.000001))
		return false;
	if (!areEqual(x(2), sorResultOver(2), 0.000001))
		return false;

	if (sorResultUnder.getCount() != 3)
		return false;
	if (!areEqual(x(0), sorResultUnder(0), 0.000001))
		return false;
	if (!areEqual(x(1), sorResultUnder(1), 0.000001))
		return false;
	if (!areEqual(x(2), sorResultUnder(2), 0.000001))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemFour()
{
	auto n = 15025;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	Vector<long double, Complex<long double>> x(n);
	Vector<long double, Complex<long double>> b(n);
	fstream file("testdata\\matrix.csv", ios_base::in);
	file >> A;
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);

	for (auto i = 0; i < n; ++i)
		x.set(i, Complex<long double>(i));

	A.multiply(b, x);
	auto result = luSolver.solve(b);

	return areEqual(x, result, 1e-5);
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemFive()
{
	auto n = 15025;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	Vector<long double, Complex<long double>> b(n);
	fstream matrixFile("testdata\\matrix.csv", ios_base::in);
	matrixFile >> A;
	fstream vectorFile("testdata\\vector_currentiteration.csv", ios_base::in);
	vectorFile >> b;
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);

	auto x = luSolver.solve(b);

	Vector<long double, Complex<long double>> bEstimate(n);
	Vector<long double, Complex<long double>> residual(n);
	A.multiply(bEstimate, x);
	residual.subtract(bEstimate, b);
	auto error = sqrt(abs(residual.squaredNorm()/b.squaredNorm()));
	return error < 1e-5;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemSix()
{
	auto n = 15025;
	auto m = 20;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	Vector<long double, Complex<long double>> x(m);
	Vector<long double, Complex<long double>> b(m);
	fstream file("testdata\\matrix.csv", ios_base::in);
	file >> A;
	auto AReduced = A.createReducedMatrix(m, m);
	LUDecompositionSparse<long double, Complex<long double>> luSolver(AReduced);

	for (auto i = 0; i < m; ++i)
		x.set(i, Complex<long double>(i + 1));

	AReduced.multiply(b, x);

	auto result = luSolver.solve(b);

	return areEqual(x, result, 1e-5);
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsLinearEquationSystemSeven()
{
	auto n = 15025;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	Vector<long double, Complex<long double>> b(n);
	fstream matrixFile("testdata\\matrix.csv", ios_base::in);
	matrixFile >> A;
	fstream vectorFile("testdata\\vector.csv", ios_base::in);
	vectorFile >> b;
	LUDecompositionStable<long double, Complex<long double>> luSolver(A);
	
	auto x = luSolver.solve(b);
	
	Vector<long double, Complex<long double>> bEstimate(n);
	Vector<long double, Complex<long double>> residual(n);
	A.multiply(bEstimate, x);
	residual.subtract(bEstimate, b);
	auto error = sqrt(abs(residual.squaredNorm()/b.squaredNorm()));
	return error < 1e-4;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorConstructor()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorCopyConstructor()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorAssignment()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorDotProduct()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorSquaredNorm()
{
	Vector<long double, Complex<long double> > a(3);
	a.set(0, Complex<long double>(1, 0));
	a.set(1, Complex<long double>(2, 0));
	a.set(2, Complex<long double>(3, 0));

	auto result = a.squaredNorm();

	return result == Complex<long double>(1*1 + 2*2 + 3*3, 0);
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorWeightedSum()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorAddWeightedSum()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorPointwiseMultiply()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorSubtract()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorStreaming()
{
	Vector<long double, Complex<long double> > original(3);
	original.set(0, Complex<long double>(1, 0));
	original.set(1, Complex<long double>(2, 0));
	original.set(2, Complex<long double>(3, 0));
	Vector<long double, Complex<long double> > parsed(3);	
	stringstream stream;
	stream << original;
	stream.seekg(ios_base::beg);
	stream >> parsed;

	return parsed == original;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorConjugate()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsVectorMultiPrecision()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixConstructor()
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

	auto n = 5;
	vector<int> permutation;
	permutation.push_back(4);
	permutation.push_back(2);
	permutation.push_back(0);
	permutation.push_back(3);
	permutation.push_back(1);
	Vector<long double, Complex<long double>> x(n);
	Vector<long double, Complex<long double>> y(n);

	for (auto i = 0; i < n; ++i)
		x.set(i, Complex<long double>(i + 1));

	SparseMatrix<long double, Complex<long double>> P(permutation);

	P.multiply(y, x);
	
	for (auto i = 0; i < n; ++i)
		if (y(i) != Complex<long double>(permutation[i] + 1))
			return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixGet()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 3);

	for (auto row = 0; row < 3; ++row)
		for (auto column = 0; column < 3; ++column)
			if (matrix(row, column) != Complex<long double>(0, 0))
				return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixSet()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixStreaming()
{
	SparseMatrix<long double, Complex<long double> > original(3, 4);
	original.set(0, 1, Complex<long double>(2, 0));
	original.set(0, 2, Complex<long double>(3, 0));
	original.set(0, 3, Complex<long double>(4, 0));
	original.set(1, 0, Complex<long double>(5, 0));
	original.set(1, 3, Complex<long double>(60, 0));
	original.set(2, 2, Complex<long double>(7, 0));
	original.set(2, 0, Complex<long double>(80, 0));
	SparseMatrix<long double, Complex<long double> > parsed(3, 4);
	stringstream stream;
	stream << original;
	stream.seekg(ios_base::beg);
	stream >> parsed;

	return parsed == original;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixTranspose()
{
	SparseMatrix<long double, Complex<long double> > matrix(4, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	
	matrix.transpose();

	if (matrix(1, 0) != Complex<long double>(2, 0))
		return false;
	if (matrix(2, 0) != Complex<long double>(3, 0))
		return false;
	if (matrix(3, 0) != Complex<long double>(4, 0))
		return false;
	if (matrix(0, 1) != Complex<long double>(5, 0))
		return false;
	if (matrix(3, 1) != Complex<long double>(60, 0))
		return false;
	if (matrix(2, 2) != Complex<long double>(7, 0))
		return false;
	if (matrix(0, 2) != Complex<long double>(80, 0))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixPermutateRows()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	vector<int> permutation;
	permutation.push_back(2);
	permutation.push_back(0);
	permutation.push_back(1);

	matrix.permutateRows(permutation);

	if (matrix(1, 1) != Complex<long double>(2, 0))
		return false;
	if (matrix(1, 2) != Complex<long double>(3, 0))
		return false;
	if (matrix(1, 3) != Complex<long double>(4, 0))
		return false;
	if (matrix(2, 0) != Complex<long double>(5, 0))
		return false;
	if (matrix(2, 3) != Complex<long double>(60, 0))
		return false;
	if (matrix(0, 2) != Complex<long double>(7, 0))
		return false;
	if (matrix(0, 0) != Complex<long double>(80, 0))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixPermutateColumns()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	vector<int> permutation;
	permutation.push_back(2);
	permutation.push_back(0);
	permutation.push_back(3);
	permutation.push_back(1);

	matrix.permutateColumns(permutation);

	if (matrix(0, 3) != Complex<long double>(2, 0))
		return false;
	if (matrix(0, 0) != Complex<long double>(3, 0))
		return false;
	if (matrix(0, 2) != Complex<long double>(4, 0))
		return false;
	if (matrix(1, 1) != Complex<long double>(5, 0))
		return false;
	if (matrix(1, 2) != Complex<long double>(60, 0))
		return false;
	if (matrix(2, 0) != Complex<long double>(7, 0))
		return false;
	if (matrix(2, 1) != Complex<long double>(80, 0))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixReduceBandwidth()
{
	auto n = 15025;
	auto m = 50;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	fstream file("testdata\\matrix.csv", ios_base::in);
	file >> A;
	auto AReduced = A.createReducedMatrix(m, m);
	
	AReduced.reduceBandwidth();

	auto result = AReduced.calculateBandwidth();
	if (result > 14)
		return false;

	A.reduceBandwidth();

	result = A.calculateBandwidth();
	if (result > 735)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixMultiply()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixRowIteration()
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

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixRowIterationWithStartColumn()
{
	SparseMatrix<long double, Complex<long double> > matrix(4, 5);
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

	for (auto row = 0; row < 4; ++row)
	{
		auto iterator = matrix.getRowIterator(row, row + 1);
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
	valuesShouldBe.push_back(Complex<long double>(60, 0));
	columnsShouldBe.push_back(1);
	columnsShouldBe.push_back(2);
	columnsShouldBe.push_back(3);
	columnsShouldBe.push_back(3);
	nonZeroCountsShouldBe.push_back(3);
	nonZeroCountsShouldBe.push_back(1);
	nonZeroCountsShouldBe.push_back(0);
	nonZeroCountsShouldBe.push_back(0);

	if (columnsShouldBe != columns)
		return false;

	if (valuesShouldBe != values)
		return false;

	if (nonZeroCountsShouldBe != nonZeroCounts)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixFindAbsoluteMaximumOfColumn()
{	
	SparseMatrix<long double, Complex<long double> > matrix(4, 4);
	matrix.set(0, 2, Complex<long double>(2134, 0));
	matrix.set(0, 3, Complex<long double>(43234, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(1, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	matrix.set(3, 1, Complex<long double>(3, 0));
	matrix.set(3, 3, Complex<long double>(4, 0));
	vector<int> maximumRows;

	for (auto column = 0; column < 4; ++column)
		maximumRows.push_back(matrix.findAbsoluteMaximumOfColumn(column, 1));

	vector<int> maximumRowsShouldBe;
	maximumRowsShouldBe.push_back(2);
	maximumRowsShouldBe.push_back(3);
	maximumRowsShouldBe.push_back(2);
	maximumRowsShouldBe.push_back(1);

	return maximumRowsShouldBe == maximumRows;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixCalculateBandwidth()
{
	auto n = 15025;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	fstream file("testdata\\matrix.csv", ios_base::in);
	file >> A;

	auto result = A.calculateBandwidth();

	return result == 15012;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixChangeRows()
{
	SparseMatrix<long double, Complex<long double> > matrix(4, 4);
	matrix.set(0, 1, Complex<long double>(7, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(3, 0, Complex<long double>(80, 0));

	matrix.swapRows(0, 1);
	matrix.swapRows(3, 1);
	matrix.swapRows(2, 3);

	vector< Complex<long double> > values;
	for (auto row = 0; row < 3; ++row)
		for (auto i = matrix.getRowIterator(row); i.isValid(); i.next())
			values.push_back(i.getValue());
	
	vector< Complex<long double> > valuesShouldBe;
	valuesShouldBe.push_back(Complex<long double>(5, 0));
	valuesShouldBe.push_back(Complex<long double>(60, 0));
	valuesShouldBe.push_back(Complex<long double>(80, 0));
	valuesShouldBe.push_back(Complex<long double>(7, 0));
	valuesShouldBe.push_back(Complex<long double>(3, 0));
	valuesShouldBe.push_back(Complex<long double>(4, 0));

	return valuesShouldBe == values;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixAssignment()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 3);
	SparseMatrix<long double, Complex<long double> > copy(3, 3);
	matrix.set(0, 0, Complex<long double>(4, 0));
	matrix.set(2, 2, Complex<long double>(5, 0));
	matrix.set(2, 1, Complex<long double>(6, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(1, 0, Complex<long double>(8, 0));
	matrix.set(1, 1, Complex<long double>(9, 0));
	matrix.set(1, 2, Complex<long double>(10, 0));
	matrix.set(1, 1, Complex<long double>(11, 0));

	copy = matrix;

	if (copy(0, 0) != Complex<long double>(4, 0))
		return false;

	if (copy(1, 0) != Complex<long double>(8, 0))
		return false;

	if (copy(1, 1) != Complex<long double>(11, 0))
		return false;

	if (copy(1, 2) != Complex<long double>(10, 0))
		return false;

	if (copy(2, 1) != Complex<long double>(6, 0))
		return false;

	if (copy(2, 2) != Complex<long double>(7, 0))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixGetRowValuesAndColumns()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 5);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	std::vector<std::pair<int, Complex<long double>>> result;

	for (auto i = 0; i < 3; ++i)
	{
		auto partialResult = matrix.getRowValuesAndColumns(i, 2);
		result.insert(result.end(), partialResult.begin(), partialResult.end());
	}
	
	std::vector<std::pair<int, Complex<long double>>> resultShouldBe;
	resultShouldBe.push_back(std::pair<int, Complex<long double>>(2, Complex<long double>(3, 0)));
	resultShouldBe.push_back(std::pair<int, Complex<long double>>(3, Complex<long double>(4, 0)));
	resultShouldBe.push_back(std::pair<int, Complex<long double>>(3, Complex<long double>(60, 0)));
	resultShouldBe.push_back(std::pair<int, Complex<long double>>(2, Complex<long double>(7, 0)));

	return result == resultShouldBe;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixCompress()
{
	SparseMatrix<long double, Complex<long double> > matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	matrix.set(1, 3, Complex<long double>());
	matrix.set(0, 2, Complex<long double>());

	matrix.compress();

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
	valuesShouldBe.push_back(Complex<long double>(4, 0));
	valuesShouldBe.push_back(Complex<long double>(5, 0));
	valuesShouldBe.push_back(Complex<long double>(80, 0));
	valuesShouldBe.push_back(Complex<long double>(7, 0));
	columnsShouldBe.push_back(1);
	columnsShouldBe.push_back(3);
	columnsShouldBe.push_back(0);
	columnsShouldBe.push_back(0);
	columnsShouldBe.push_back(2);
	nonZeroCountsShouldBe.push_back(2);
	nonZeroCountsShouldBe.push_back(1);
	nonZeroCountsShouldBe.push_back(2);

	if (columnsShouldBe != columns)
		return false;

	if (valuesShouldBe != values)
		return false;

	if (nonZeroCountsShouldBe != nonZeroCounts)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixAddWeightedRowElements()
{
	SparseMatrix<long double, Complex<long double>> matrix(3, 4);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 2, Complex<long double>(3, 0));
	matrix.set(0, 3, Complex<long double>(4, 0));
	matrix.set(1, 0, Complex<long double>(5, 0));
	matrix.set(1, 3, Complex<long double>(60, 0));
	matrix.set(2, 2, Complex<long double>(7, 0));
	matrix.set(2, 0, Complex<long double>(80, 0));
	vector<pair<int, Complex<long double>>> valuesAndColumns;
	valuesAndColumns.push_back(pair<int, Complex<long double>>(0, Complex<long double>(9, 0)));
	valuesAndColumns.push_back(pair<int, Complex<long double>>(1, Complex<long double>(10, 0)));
	valuesAndColumns.push_back(pair<int, Complex<long double>>(3, Complex<long double>(11, 0)));

	matrix.addWeightedRowElements(0, Complex<long double>(2, 0), valuesAndColumns);

	vector<Complex<long double>> values;
	for (auto row = 0; row < 3; ++row)
		for (auto i = matrix.getRowIterator(row); i.isValid(); i.next())
			values.push_back(i.getValue());

	vector<Complex<long double>> valuesShouldBe;
	valuesShouldBe.push_back(Complex<long double>(18, 0));
	valuesShouldBe.push_back(Complex<long double>(22, 0));
	valuesShouldBe.push_back(Complex<long double>(3, 0));
	valuesShouldBe.push_back(Complex<long double>(26, 0));
	valuesShouldBe.push_back(Complex<long double>(5, 0));
	valuesShouldBe.push_back(Complex<long double>(60, 0));
	valuesShouldBe.push_back(Complex<long double>(80, 0));
	valuesShouldBe.push_back(Complex<long double>(7, 0));

	if (valuesShouldBe != values)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsSparseMatrixMultiplyWithStartAndEndColumn()
{
	SparseMatrix<long double, Complex<long double>> matrix(1, 5);
	matrix.set(0, 1, Complex<long double>(2, 0));
	matrix.set(0, 3, Complex<long double>(3, 0));
	matrix.set(0, 4, Complex<long double>(4, 0));
	Vector<long double, Complex<long double>> vector(5);
	vector.set(0, Complex<long double>(10, 0));
	vector.set(1, Complex<long double>(20, 0));
	vector.set(2, Complex<long double>(30, 0));
	vector.set(3, Complex<long double>(40, 0));
	vector.set(4, Complex<long double>(50, 0));

	auto one = matrix.multiplyRowWithStartColumn(0, vector, 1);
	auto two = matrix.multiplyRowWithStartColumn(0, vector, 2);
	auto three = matrix.multiplyRowWithEndColumn(0, vector, 3);
	auto four = matrix.multiplyRowWithEndColumn(0, vector, 4);

	if (one != Complex<long double>(20*2 + 40*3 + 50*4, 0))
		return false;

	if (two != Complex<long double>(40*3 + 50*4, 0))
		return false;

	if (three != Complex<long double>(20*2 + 40*3, 0))
		return false;

	if (four != Complex<long double>(20*2 + 40*3 + 50*4, 0))
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsGraphCalculateReverseCuthillMcKee()
{
	Graph graphOne;
	Graph graphTwo;

	for (auto i = 1; i <= 8; ++i)
		graphOne.addNode(i);

	for (auto i = 1; i <= 10; ++i)
		graphTwo.addNode(i);

	graphOne.connect(1, 5);
	graphOne.connect(2, 3);
	graphOne.connect(2, 6);
	graphOne.connect(2, 8);
	graphOne.connect(3, 5);
	graphOne.connect(4, 7);
	graphOne.connect(6, 8);

	graphTwo.connect(1, 2);
	graphTwo.connect(1, 4);
	graphTwo.connect(1, 9);
	graphTwo.connect(2, 9);
	graphTwo.connect(2, 3);
	graphTwo.connect(3, 5);
	graphTwo.connect(3, 9);
	graphTwo.connect(4, 5);
	graphTwo.connect(4, 6);
	graphTwo.connect(4, 9);
	graphTwo.connect(4, 10);
	graphTwo.connect(5, 8);
	graphTwo.connect(5, 9);
	graphTwo.connect(5, 10);
	graphTwo.connect(6, 7);
	graphTwo.connect(6, 10);
	graphTwo.connect(7, 8);
	graphTwo.connect(7, 10);
	graphTwo.connect(8, 10);
	graphTwo.connect(9, 10);

	auto result = graphOne.calculateReverseCuthillMcKee();

	if (result[0] != 7)
		return false;
	if (result[1] != 4)
		return false;
	if (result[2] != 8)
		return false;
	if (result[3] != 6)
		return false;
	if (result[4] != 2)
		return false;
	if (result[5] != 3)
		return false;
	if (result[6] != 5)
		return false;
	if (result[7] != 1)
		return false;

	result = graphTwo.calculateReverseCuthillMcKee();

	if (result[9] != 1)
		return false;
	if (result[8] != 2)
		return false;
	if (result[7] != 4)
		return false;
	if (result[6] != 9)
		return false;
	if (result[5] != 3)
		return false;
	if (result[4] != 6)
		return false;
	if (result[3] != 5)
		return false;
	if (result[2] != 10)
		return false;
	if (result[1] != 7)
		return false;
	if (result[0] != 8)
		return false;

	result = graphTwo.calculateReverseCuthillMcKee(2);

	if (result[9] != 2)
		return false;
	if (result[8] != 1)
		return false;
	if (result[7] != 3)
		return false;
	if (result[6] != 9)
		return false;
	if (result[5] != 4)
		return false;
	if (result[4] != 5)
		return false;
	if (result[3] != 10)
		return false;
	if (result[2] != 6)
		return false;
	if (result[1] != 8)
		return false;
	if (result[0] != 7)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsGraphCreateLayeringFrom()
{
	Graph graph;

	for (auto i = 1; i <= 10; ++i)
		graph.addNode(i);

	graph.connect(1, 2);
	graph.connect(1, 4);
	graph.connect(1, 9);
	graph.connect(2, 9);
	graph.connect(2, 3);
	graph.connect(3, 5);
	graph.connect(3, 9);
	graph.connect(4, 5);
	graph.connect(4, 6);
	graph.connect(4, 9);
	graph.connect(4, 10);
	graph.connect(5, 8);
	graph.connect(5, 9);
	graph.connect(5, 10);
	graph.connect(6, 7);
	graph.connect(6, 10);
	graph.connect(7, 8);
	graph.connect(7, 10);
	graph.connect(8, 10);
	graph.connect(9, 10);

	auto layering = graph.createLayeringFrom(3);

	for (auto &layer : layering)
		sort(layer.begin(), layer.end());

	if (layering.size() != 4)
		return false;
	if (layering[0].size() != 1)
		return false;
	if (layering[0][0] != 3)
		return false;
	if (layering[1].size() != 3)
		return false;
	if (layering[1][0] != 2)
		return false;
	if (layering[1][1] != 5)
		return false;
	if (layering[1][2] != 9)
		return false;
	if (layering[2].size() != 4)
		return false;
	if (layering[2][0] != 1)
		return false;
	if (layering[2][1] != 4)
		return false;
	if (layering[2][2] != 8)
		return false;
	if (layering[2][3] != 10)
		return false;
	if (layering[3].size() != 2)
		return false;
	if (layering[3][0] != 6)
		return false;
	if (layering[3][1] != 7)
		return false;

	auto secondLevelDegree = graph.calculateSecondLevelDegree(8);

	if (secondLevelDegree != 7)
		return false;

	return true;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsGraphFindPseudoPeriphereNode()
{
	Graph graph;	

	for (auto i = 1; i <= 8; ++i)
		graph.addNode(i);

	graph.connect(1, 2);
	graph.connect(1, 3);
	graph.connect(1, 5);
	graph.connect(1, 6);
	graph.connect(2, 3);
	graph.connect(2, 4);
	graph.connect(2, 5);
	graph.connect(3, 6);
	graph.connect(5, 6);
	graph.connect(5, 8);
	graph.connect(6, 8);
	graph.connect(7, 8);

	auto result = graph.findPseudoPeriphereNode();

	return result == 4 || result == 7;
}

extern "C" __declspec(dllexport) bool __cdecl RunTestsGraphFindPseudoPeriphereNodeOfAdmittanceMatrix()
{
	auto n = 15025;
	SparseMatrix<long double, Complex<long double>> A(n, n);
	fstream file("testdata\\matrix.csv", ios_base::in);
	file >> A;
	auto graph = A.createGraph();
	size_t eccentricity = 0;

	auto result = graph->findPseudoPeriphereNode(eccentricity);

	return eccentricity >= 190;
}