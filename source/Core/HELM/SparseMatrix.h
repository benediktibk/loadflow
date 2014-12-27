#pragma once

#include <vector>
#include "Vector.h"

template<class T>
class SparseMatrix
{
public:
	SparseMatrix(size_t rows, size_t columns);

	size_t getRowCount() const;
	size_t getColumnCount() const;
	void set(size_t row, size_t column, T const &value);
	void multiply(Vector<T> &destination, Vector<T> const &source) const;

	T const& operator()(size_t row, size_t column) const;

private:
	bool findPosition(size_t row, size_t column, size_t &position) const;

private:
	const size_t _rowCount;
	const size_t _columnCount;
	std::vector<size_t> _columns;
	std::vector<size_t> _rowPointers;
	std::vector<T> _values;
};

