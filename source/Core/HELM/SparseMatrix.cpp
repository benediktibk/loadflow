#include "SparseMatrix.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "SparseMatrixRowIterator.h"
#include <assert.h>
#include <complex>

template class SparseMatrix<long double>;
template class SparseMatrix< std::complex<long double> >;
template class SparseMatrix< Complex<MultiPrecision> >;

template<class T>
SparseMatrix<T>::SparseMatrix(int rows, int columns) :
	_rowCount(rows),
	_columnCount(columns),
	_zero(0)
{
	assert(getRowCount() > 0);
	assert(getColumnCount() > 0);

	_rowPointers.resize(getRowCount() + 1, 0);
}

template<class T>
int SparseMatrix<T>::getRowCount() const
{
	return _rowCount;
}

template<class T>
int SparseMatrix<T>::getColumnCount() const
{
	return _columnCount;
}

template<class T>
void SparseMatrix<T>::set(int row, int column, T const &value)
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

	for (auto i = row + 1; i < _rowPointers.size(); ++i)
		_rowPointers[i] += 1;
}

template<class T>
void SparseMatrix<T>::multiply(Vector<T> &destination, Vector<T> const &source) const
{
	assert(destination.getCount() == getRowCount());
	assert(source.getCount() == getColumnCount());

	for (auto i = 0; i < _rowCount; ++i)
	{
		auto rowPointer = _rowPointers[i];
		auto nextRowPointer = _rowPointers[i + 1];
		T result(0);

		for (auto j = rowPointer; j < nextRowPointer; ++j)
		{
			auto column = _columns[j];
			T const &value = _values[j];
			result += value*source(column);
		}

		destination.set(i, result);
	}
}

template<class T>
SparseMatrixRowIterator<T> SparseMatrix<T>::getRowIterator(int row) const
{
	assert(row >= 0);
	assert(row < getRowCount());
	return SparseMatrixRowIterator<T>(_values, _rowPointers, _columns, row);
}

template<class T>
T const& SparseMatrix<T>::operator()(int row, int column) const
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

template<class T>
bool SparseMatrix<T>::findPosition(int row, int column, int &position) const
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

