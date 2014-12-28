#pragma once

#include <vector>

template<class T>
class SparseMatrixRowIterator
{
public:
	SparseMatrixRowIterator(std::vector<T> const &values, std::vector<int> const &rowPointers, std::vector<int> const &columns, int row);

	bool isValid() const;
	void next();
	T const& getValue() const;
	int getColumn() const;
	int getNonZeroCount() const;

private:
	const std::vector<T> &_values;
	const std::vector<int> &_columns;
	const int _startPosition;
	const int _endPosition;
	int _position;
};

