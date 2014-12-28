#pragma once

#include <vector>

template<class T>
class SparseMatrixRowIterator
{
public:
	SparseMatrixRowIterator(std::vector<T> const &values, std::vector<size_t> const &rowPointers, std::vector<size_t> const &columns, size_t row);

	bool isValid() const;
	void next();
	T const& getValue() const;
	size_t getColumn() const;

private:
	const std::vector<T> &_values;
	const std::vector<size_t> &_columns;
	const size_t _startPosition;
	const size_t _endPosition;
	size_t _position;
};

