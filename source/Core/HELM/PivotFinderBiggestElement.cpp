#include "PivotFinderBiggestElement.h"
#include "Complex.h"
#include "MultiPrecision.h"

template class PivotFinderBiggestElement<long double, Complex<long double>>;
template class PivotFinderBiggestElement<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
int PivotFinderBiggestElement<Floating, ComplexFloating>::operator()(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const
{
	return upper.findAbsoluteMaximumOfColumn(row, row);
}