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
	/*auto candidates = upper.findNonZeroValuesInColumnWithSmallestElementCount(row, row);
	std::map<Floating, int> candidatesSorted;

	for each (auto candidate in candidates)
		candidatesSorted[std::abs2(candidate.second)] = candidate.first;

	assert(candidatesSorted.size() > 0);
	return candidatesSorted.rbegin()->second;*/
}