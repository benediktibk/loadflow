#include "LUDecompositionSparse.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "PivotFinderSmallestFillIn.h"

template class LUDecompositionSparse<long double, Complex<long double>>;
template class LUDecompositionSparse<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
LUDecompositionSparse<Floating, ComplexFloating>::LUDecompositionSparse(SparseMatrix<Floating, ComplexFloating> const &systemMatrix) :
	LUDecomposition<Floating, ComplexFloating>(systemMatrix, new PivotFinderSmallestFillIn<Floating, ComplexFloating>())
{ }
