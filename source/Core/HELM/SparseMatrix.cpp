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
	auto rowPosition = _rowPointers[row];

	for (auto i = rowPosition; i < _rowPointers[row + 1]; ++i)
	{
		auto currentColumn = _columns[i];

		if (currentColumn == column)
		{
			_values[i] = value;
			return;
		}
	}

	auto nextRowPosition = _rowPointers[row + 1];
	_columns.insert(_columns.begin() + nextRowPosition, column);
	_values.insert(_values.begin() + nextRowPosition, value);

	for (auto i = row + 1; i < _rowPointers.size(); ++i)
		_rowPointers[i] += 1;
}

template<class T>
T const& SparseMatrix<T>::operator()(size_t row, size_t column) const
{
	assert(row < getRowCount());
	assert(column < getColumnCount());

	auto rowPosition = _rowPointers[row];

	for (auto i = rowPosition; i < _rowPointers[row + 1]; ++i)
	{
		auto currentColumn = _columns[i];

		if (currentColumn == column)
			return _values[i];
	}

	auto result = T(0);
	return result;
}

