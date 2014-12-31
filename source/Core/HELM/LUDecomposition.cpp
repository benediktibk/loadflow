#include "LUDecomposition.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <assert.h>

template class LUDecomposition<long double, Complex<long double>>;
template class LUDecomposition<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
LUDecomposition<Floating, ComplexFloating>::LUDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix) :
	_dimension(systemMatrix.getRowCount()),
	_left(_dimension, _dimension),
	_upper(_dimension, _dimension),
	_permutation(_dimension, _dimension)
{
	assert(systemMatrix.getRowCount() == systemMatrix.getColumnCount());
	calculateDecomposition(systemMatrix);
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> LUDecomposition<Floating, ComplexFloating>::solve(const Vector<Floating, ComplexFloating> &b) const
{
	return b;
}


template<class Floating, class ComplexFloating>
void LUDecomposition<Floating, ComplexFloating>::calculateDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix)
{
	_upper = systemMatrix;
	auto one = ComplexFloating(Floating(1));
	auto zero = ComplexFloating(Floating(0));

	for (auto i = 0; i < _dimension; ++i)
	{
		_permutation.set(i, i, one);
		_left.set(i, i, one);
	}

	for (auto i = 0; i < _dimension - 1; ++i)
	{
		auto pivotIndex = _upper.findAbsoluteMaximumOfColumn(i, i);
		auto pivotElement = _upper(pivotIndex, i);
		_upper.swapRows(i, pivotIndex);
		_left.swapRows(i, pivotIndex);
		_permutation.swapRows(i, pivotIndex);
		auto pivotRow = _upper.getRowValuesAndColumns(i, i + 1);

		for (auto j = i + 1; j < _dimension; ++j)
		{
			const ComplexFloating &currentValue = _upper(j, i);
			auto factor = currentValue/pivotElement;
			_left.set(j, i, factor);

			if (currentValue == zero)
				continue;

			auto factorNegative = factor*ComplexFloating(Floating(-1));
			_upper.addWeightedRowElements(j, factorNegative, pivotRow);
			_upper.set(j, i, zero);
		}
	}

	_upper.compress();
}