#pragma once

#include <vector>

template<class T>
class SparseMatrixRowIterator
{
public:
	SparseMatrixRowIterator(std::vector<T> const &values, std::vector<int> const &columns, int start, int end, int row);

	bool isValid() const;
	void next();
	T const& getValue() const;
	int getColumn() const;
	int getRow() const;
	int getNonZeroCount() const;

private:
	const std::vector<T> &_values;
	const std::vector<int> &_columns;
	const int _startPosition;
	const int _endPosition;
	const int _row;
	int _position;
};

