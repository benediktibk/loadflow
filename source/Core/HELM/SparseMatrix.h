#pragma once

#include <vector>
#include "Vector.h"
#include "SparseMatrixRowIterator.h"

template<class Floating, class ComplexFloating>
class SparseMatrix
{
public:
	SparseMatrix(int rows, int columns);

	int getRowCount() const;
	int getColumnCount() const;
	void set(int row, int column, ComplexFloating const &value);
	void multiply(Vector<Floating, ComplexFloating> &destination, Vector<Floating, ComplexFloating> const &source) const;
	SparseMatrixRowIterator<ComplexFloating> getRowIterator(int row) const;
	int findAbsoluteMaximumOfColumn(int column) const;

	ComplexFloating const& operator()(int row, int column) const;

private:
	bool findPosition(int row, int column, int &position) const;

private:
	const int _rowCount;
	const int _columnCount;
	const ComplexFloating _zero;
	std::vector<int> _columns;
	std::vector<int> _rowPointers;
	std::vector<ComplexFloating> _values;
};

