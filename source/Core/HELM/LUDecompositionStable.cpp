#include "LUDecompositionStable.h"
#include "Complex.h"
#include "MultiPrecision.h"

template class LUDecompositionStable<long double, Complex<long double>>;
template class LUDecompositionStable<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
LUDecompositionStable<Floating, ComplexFloating>::LUDecompositionStable(SparseMatrix<Floating, ComplexFloating> const &systemMatrix) :
	LUDecomposition<Floating, ComplexFloating>(systemMatrix)
{ }

template<class Floating, class ComplexFloating>
int LUDecompositionStable<Floating, ComplexFloating>::findPivotIndex(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const
{
	return upper.findAbsoluteMaximumOfColumn(row, row);
}