#include "SparseMatrix.h"
#include <assert.h>

template class SparseMatrix<long double>;

template<class T>
SparseMatrix<T>::SparseMatrix(size_t rows, size_t columns) :
	_rowCount(rows),
	_columnCount(columns)
{
	assert(getRowCount() > 0);
	assert(getColumnCount() > 0);

	_rowPointers.resize(getRowCount() + 1, 0);
}

template<class T>
size_t SparseMatrix<T>::getRowCount() const
{
	return _rowCount;
}

template<class T>
size_t SparseMatrix<T>::getColumnCount() const
{
	return _columnCount;
}

template<class T>
void SparseMatrix<T>::set(size_t row, size_t column, T const &value)
{
	size_t position;

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

	for (size_t i = 0; i < _rowCount; ++i)
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
T const& SparseMatrix<T>::operator()(size_t row, size_t column) const
{
	assert(row < getRowCount());
	assert(column < getColumnCount());

	size_t position;
	T result(0);

	if (findPosition(row, column, position))
		result = _values[position];

	return result;
}

template<class T>
bool SparseMatrix<T>::findPosition(size_t row, size_t column, size_t &position) const
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

