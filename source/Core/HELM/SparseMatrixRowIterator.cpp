#include "SparseMatrixRowIterator.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <assert.h>

template class SparseMatrixRowIterator< Complex<long double> >;
template class SparseMatrixRowIterator< Complex<MultiPrecision> >;

template<class T>
SparseMatrixRowIterator<T>::SparseMatrixRowIterator(std::vector<T> const &values, std::vector<int> const &columns, int start, int end, int row) :
	_values(values),
	_columns(columns),
	_startPosition(start),
	_endPosition(end),
	_row(row),
	_position(_startPosition)
{
	assert(_endPosition >= _startPosition);
}

template<class T>
bool SparseMatrixRowIterator<T>::isValid() const
{
	return _position < _endPosition;
}

template<class T>
void SparseMatrixRowIterator<T>::next()
{
	++_position;
}

template<class T>
T const& SparseMatrixRowIterator<T>::getValue() const
{
	assert(isValid());
	return _values[_position];
}

template<class T>
int SparseMatrixRowIterator<T>::getColumn() const
{
	assert(isValid());
	return _columns[_position];
}

template<class T>
int SparseMatrixRowIterator<T>::getRow() const
{
	return _row;
}

template<class T>
int SparseMatrixRowIterator<T>::getNonZeroCount() const
{
	return _endPosition - _startPosition;
}

