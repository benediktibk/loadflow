#include "SparseMatrix.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "SparseMatrixRowIterator.h"
#include <assert.h>
#include <algorithm>
#include <functional>

template class SparseMatrix<long double, Complex<long double> >;
template class SparseMatrix<MultiPrecision, Complex<MultiPrecision> >;

template<class Floating, class ComplexFloating>
SparseMatrix<Floating, ComplexFloating>::SparseMatrix(int rows, int columns) :
	_rowCount(rows),
	_columnCount(columns),
	_zero(Floating(0), Floating(0))
{
	assert(getRowCount() > 0);
	assert(getColumnCount() > 0);

	_rowPointers.resize(getRowCount() + 1, 0);
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::getRowCount() const
{
	return _rowCount;
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::getColumnCount() const
{
	return _columnCount;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::set(int row, int column, ComplexFloating const &value)
{
	int position;

	if (findPosition(row, column, position))
	{
		_values[position] = value;
		return;
	}

	_columns.insert(_columns.begin() + position, column);
	_values.insert(_values.begin() + position, value);
	
	#pragma omp parallel for
	for (auto i = row + 1; i < static_cast<int>(_rowPointers.size()); ++i)
		_rowPointers[i] += 1;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::multiply(Vector<Floating, ComplexFloating> &destination, Vector<Floating, ComplexFloating> const &source) const
{
	assert(destination.getCount() == getRowCount());
	assert(source.getCount() == getColumnCount());

	#pragma omp parallel
	{
		std::vector<Floating> summandsReal;
		std::vector<Floating> summandsImaginary;

		#pragma omp for
		for (auto i = 0; i < _rowCount; ++i)
		{
			auto rowPointer = _rowPointers[i];
			auto nextRowPointer = _rowPointers[i + 1];
			const auto count = nextRowPointer - rowPointer;
			summandsReal.clear();
			summandsImaginary.clear();
			summandsReal.reserve(count);
			summandsImaginary.reserve(count);

			for (auto j = rowPointer; j < nextRowPointer; ++j)
			{
				auto column = _columns[j];
				ComplexFloating const &value = _values[j];
				auto summand = value*source(column);
				summandsReal.push_back(std::real(summand));
				summandsImaginary.push_back(std::imag(summand));
			}

			std::sort(summandsReal.begin(), summandsReal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
			std::sort(summandsImaginary.begin(), summandsImaginary.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
			ComplexFloating result;

			for (auto j = 0; j < count; ++j)
				result += ComplexFloating(summandsReal[j], summandsImaginary[j]);

			destination.set(i, result);
		}
	}
}

template<class Floating, class ComplexFloating>
SparseMatrixRowIterator<ComplexFloating> SparseMatrix<Floating, ComplexFloating>::getRowIterator(int row) const
{
	assert(isValidRowIndex(row));
	return SparseMatrixRowIterator<ComplexFloating>(_values, _rowPointers, _columns, row);
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::findAbsoluteMaximumOfColumn(int column) const
{
	assert(isValidColumnIndex(column));
	auto maximumRow = 0;
	auto maximumValue = std::abs2(operator()(0, column));

	for (auto row = 1; row < _rowCount; ++row)
	{
		auto value = std::abs2(operator()(row, column));
		
		if (value > maximumValue)
		{
			maximumRow = row;
			maximumValue = value;
		}
	}

	return maximumRow;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::changeRows(int one, int two)
{
	assert(isValidRowIndex(one));
	assert(isValidRowIndex(two));
	assert(one != two);

	if (one > two)
		std::swap(one, two);

	std::vector<int> positions;
	positions.push_back(_rowPointers[one]);
	positions.push_back(_rowPointers[one + 1]);
	positions.push_back(_rowPointers[two]);
	positions.push_back(_rowPointers[two + 1]);
	
	_tempInt.clear();
	_tempComplexFloating.clear();
	_tempInt.reserve(_columns.size());
	_tempComplexFloating.reserve(_values.size());

	_tempInt.insert(_tempInt.end(), _columns.begin(), _columns.begin() + positions[0]);
	_tempComplexFloating.insert(_tempComplexFloating.end(), _values.begin(), _values.begin() + positions[0]);
	_tempInt.insert(_tempInt.end(), _columns.begin() + positions[2], _columns.begin() + positions[3]);
	_tempComplexFloating.insert(_tempComplexFloating.end(), _values.begin() + positions[2], _values.begin() + positions[3]);
	_tempInt.insert(_tempInt.end(), _columns.begin() + positions[1], _columns.begin() + positions[2]);
	_tempComplexFloating.insert(_tempComplexFloating.end(), _values.begin() + positions[1], _values.begin() + positions[2]);
	_tempInt.insert(_tempInt.end(), _columns.begin() + positions[0], _columns.begin() + positions[1]);
	_tempComplexFloating.insert(_tempComplexFloating.end(), _values.begin() + positions[0], _values.begin() + positions[1]);
	_tempInt.insert(_tempInt.end(), _columns.begin() + positions[3], _columns.end());
	_tempComplexFloating.insert(_tempComplexFloating.end(), _values.begin() + positions[3], _values.end());

	_tempInt.swap(_columns);
	_tempComplexFloating.swap(_values);
	
	auto rwoOneLength = positions[1] - positions[0];
	auto rwoTwoLength = positions[3] - positions[2];
	auto lengthDifference = rwoTwoLength - rwoOneLength;

	for (auto i = one + 1; i <= two; ++i)
		_rowPointers[i] += lengthDifference;
}

template<class Floating, class ComplexFloating>
ComplexFloating const& SparseMatrix<Floating, ComplexFloating>::operator()(int row, int column) const
{
	int position;
	if (findPosition(row, column, position))
		return _values[position];

	return _zero;
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::findPosition(int row, int column, int &position) const
{
	assert(isValidRowIndex(row));
	assert(isValidColumnIndex(column));

	auto start = _rowPointers[row];
	auto end = _rowPointers[row + 1];

	if (end == start)
	{
		position = end;
		return false;
	}

	auto rangeBecameSmaller = true;
	auto previousRangeSize = end - start;

	while(rangeBecameSmaller)
	{
		auto middle = (start + end)/2;
		auto middleColumn = _columns[middle];

		if (middleColumn < column)
			start = middle;
		else
			end = middle;

		auto rangeSize = end - start;

		rangeBecameSmaller = rangeSize < previousRangeSize;
		previousRangeSize = rangeSize;
	}

	position = end;
	return position < _rowPointers[row + 1] && column == _columns[position];
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::isValidRowIndex(int row) const
{
	return row >= 0 && row < getRowCount();
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::isValidColumnIndex(int column) const
{
	return column >= 0 && column < getColumnCount();
}

