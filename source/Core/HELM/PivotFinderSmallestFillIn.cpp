#include "PivotFinderSmallestFillIn.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <map>
#include <assert.h>

template class PivotFinderSmallestFillIn<long double, Complex<long double>>;
template class PivotFinderSmallestFillIn<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
int PivotFinderSmallestFillIn<Floating, ComplexFloating>::operator()(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const
{
	return row;
}