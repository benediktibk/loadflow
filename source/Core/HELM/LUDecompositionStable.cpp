#include "LUDecompositionStable.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "PivotFinderBiggestElement.h"

template class LUDecompositionStable<long double, Complex<long double>>;
template class LUDecompositionStable<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
LUDecompositionStable<Floating, ComplexFloating>::LUDecompositionStable(SparseMatrix<Floating, ComplexFloating> const &systemMatrix) :
	LUDecomposition<Floating, ComplexFloating>(systemMatrix, new PivotFinderBiggestElement<Floating, ComplexFloating>())
{ }