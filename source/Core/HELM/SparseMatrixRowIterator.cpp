#include "SparseMatrixRowIterator.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <complex>
#include <assert.h>

template class SparseMatrixRowIterator<long double>;
template class SparseMatrixRowIterator< std::complex<long double> >;
template class SparseMatrixRowIterator< Complex<MultiPrecision> >;

template<class T>
SparseMatrixRowIterator<T>::SparseMatrixRowIterator(std::vector<T> const &values, std::vector<int> const &rowPointers, std::vector<int> const &columns, int row) :
	_values(values),
	_columns(columns),
	_startPosition(rowPointers[row]),
	_endPosition(rowPointers[row + 1]),
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
int SparseMatrixRowIterator<T>::getNonZeroCount() const
{
	assert(isValid());
	return _endPosition - _startPosition;
}

