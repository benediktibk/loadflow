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

	auto nextRowPosition = _rowPointers[row + 1];
	_columns.insert(_columns.begin() + nextRowPosition, column);
	_values.insert(_values.begin() + nextRowPosition, value);
	
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
	assert(row >= 0);
	assert(row < getRowCount());
	return SparseMatrixRowIterator<ComplexFloating>(_values, _rowPointers, _columns, row);
}

template<class Floating, class ComplexFloating>
ComplexFloating const& SparseMatrix<Floating, ComplexFloating>::operator()(int row, int column) const
{
	assert(row < getRowCount());
	assert(column < getColumnCount());
	assert(row >= 0);
	assert(column >= 0);

	int position;
	if (findPosition(row, column, position))
		return _values[position];

	return _zero;
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::findPosition(int row, int column, int &position) const
{
	auto rowPosition = _rowPointers[row];

	for (auto i = rowPosition; i < _rowPointers[row + 1]; ++i)
	{
		auto currentColumn = _columns[i];

		if (currentColumn != column)
			continue;

		position = i;
		return true;
	}

	position = _values.size() + 1;
	return false;
}

