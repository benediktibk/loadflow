#pragma once

#include <vector>
#include "Vector.h"
#include "SparseMatrixRowIterator.h"

template<class T>
class SparseMatrix
{
public:
	SparseMatrix(int rows, int columns);

	int getRowCount() const;
	int getColumnCount() const;
	void set(int row, int column, T const &value);
	void multiply(Vector<T> &destination, Vector<T> const &source) const;
	SparseMatrixRowIterator<T> getRowIterator(int row) const;

	T const& operator()(int row, int column) const;

private:
	bool findPosition(int row, int column, int &position) const;

private:
	const int _rowCount;
	const int _columnCount;
	const T _zero;
	std::vector<int> _columns;
	std::vector<int> _rowPointers;
	std::vector<T> _values;
};

