#include "LUDecomposition.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <assert.h>

template class LUDecomposition<long double, Complex<long double>>;
template class LUDecomposition<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
LUDecomposition<Floating, ComplexFloating>::LUDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix, IPivotFinder<Floating, ComplexFloating> *pivotFinder) :
	_dimension(systemMatrix.getRowCount()),
	_left(_dimension, _dimension),
	_upper(_dimension, _dimension),
	_permutation(_dimension, _dimension),
	_permutationBandwidthReduction(_dimension, _dimension),
	_permutationBandwidthReductionInverse(_dimension, _dimension),
	_pivotFinder(pivotFinder)
{
	assert(systemMatrix.getRowCount() == systemMatrix.getColumnCount());
	assert(pivotFinder != 0);
	calculateDecomposition(systemMatrix);
}

template<class Floating, class ComplexFloating>
LUDecomposition<Floating, ComplexFloating>::~LUDecomposition()
{
	delete _pivotFinder;
	_pivotFinder = 0;
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> LUDecomposition<Floating, ComplexFloating>::solve(const Vector<Floating, ComplexFloating> &b) const
{
	assert(_dimension == b.getCount());
	Vector<Floating, ComplexFloating> bPermutated(_dimension);
	Vector<Floating, ComplexFloating> xPermutated(_dimension);
	Vector<Floating, ComplexFloating> residual(_dimension);
	Vector<Floating, ComplexFloating> xImproved(_dimension);
	_permutationBandwidthReduction.multiply(bPermutated, b);
	auto bSquaredNorm = bPermutated.squaredNorm();
	auto x = solveInternal(bPermutated);
	auto lastError = calculateError(x, bPermutated, bSquaredNorm, residual);
	auto improved = true;
	auto maximumIterations = 10;
	auto iteration = 0;

	// iterative refinement
	while(improved && iteration < maximumIterations)
	{
		auto improvement = solveInternal(residual);
		xImproved.add(x, improvement);
		auto error = calculateError(xImproved, bPermutated, bSquaredNorm, residual);

		if (error < lastError)
		{
			x = xImproved;
			lastError = error;
		}
		else
			improved = false;

		++iteration;
	}

	_permutationBandwidthReductionInverse.multiply(xPermutated, x);
	return xPermutated;
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> LUDecomposition<Floating, ComplexFloating>::solveInternal(const Vector<Floating, ComplexFloating> &b) const
{
	auto y = forwardSubstitution(b);
	return backwardSubstitution(y);
}

template<class Floating, class ComplexFloating>
Floating LUDecomposition<Floating, ComplexFloating>::calculateError(const Vector<Floating, ComplexFloating> &x, const Vector<Floating, ComplexFloating> &b, ComplexFloating const &bSquaredNorm, Vector<Floating, ComplexFloating> &residual) const
{
	Vector<Floating, ComplexFloating> tempOne(_dimension);
	Vector<Floating, ComplexFloating> tempTwo(_dimension);
	_upper.multiply(tempOne, x);
	_left.multiply(tempTwo, tempOne);
	residual.subtract(b, tempTwo);
	auto residualSquaredNorm = residual.squaredNorm();
	return std::sqrt(std::abs(residualSquaredNorm/bSquaredNorm));
}

template<class Floating, class ComplexFloating>
void LUDecomposition<Floating, ComplexFloating>::calculateDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix)
{
	_upper = systemMatrix;
	auto permutationOrder = _upper.reduceBandwidth();
	auto permutationOrderInverted = SparseMatrix<Floating, ComplexFloating>::invertPermutation(permutationOrder);
	_permutationBandwidthReduction = SparseMatrix<Floating, ComplexFloating>(permutationOrder);
	_permutationBandwidthReductionInverse = SparseMatrix<Floating, ComplexFloating>(permutationOrderInverted);
	auto one = ComplexFloating(Floating(1));
	auto zero = ComplexFloating(Floating(0));

	for (auto i = 0; i < _dimension; ++i)
		_permutation.set(i, i, one);

	for (auto i = 0; i < _dimension - 1; ++i)
	{
		auto pivotIndex = (*_pivotFinder)(_upper, i);
		auto pivotElement = _upper(pivotIndex, i);
		_upper.swapRows(i, pivotIndex);
		_left.swapRows(i, pivotIndex);
		_permutation.swapRows(i, pivotIndex);
		auto pivotRow = _upper.getRowValuesAndColumns(i, i + 1);

		for (auto j = i + 1; j < _dimension; ++j)
		{
			const ComplexFloating &currentValue = _upper(j, i);
			auto factor = currentValue/pivotElement;

			if (currentValue == zero)
				continue;
			
			auto factorNegative = factor*ComplexFloating(Floating(-1));
			_upper.addWeightedRowElements(j, factorNegative, pivotRow);
			_upper.set(j, i, zero);
			_left.set(j, i, factor);
		}
	}	

	for (auto i = 0; i < _dimension; ++i)
		_left.set(i, i, one);

	_upper.compress();
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> LUDecomposition<Floating, ComplexFloating>::forwardSubstitution(Vector<Floating, ComplexFloating> const &b) const
{
	Vector<Floating, ComplexFloating> y(_dimension);
	Vector<Floating, ComplexFloating> bPermutated(_dimension);
	_permutation.multiply(bPermutated, b);
	y.set(0, bPermutated(0));

	for (auto i = 1; i < _dimension; ++i)
	{
		auto rowSum = _left.multiplyRowWithEndColumn(i, y, i - 1);
		auto value = bPermutated(i) - rowSum;
		y.set(i, value);
	}
	
	return y;
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> LUDecomposition<Floating, ComplexFloating>::backwardSubstitution(Vector<Floating, ComplexFloating> const &y) const
{
	Vector<Floating, ComplexFloating> x(_dimension);
	x.set(_dimension - 1, y(_dimension - 1)/_upper(_dimension - 1, _dimension - 1));

	for (auto i = _dimension - 2; i >= 0; --i)
	{
		auto rowSum = _upper.multiplyRowWithStartColumn(i, x, i + 1);
		auto value = (y(i) - rowSum)/_upper(i, i);
		x.set(i, value);
	}
	
	return x;
}