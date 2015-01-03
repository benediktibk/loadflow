#pragma once

#include <vector>
#include <utility>
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
	ComplexFloating multiplyRowWithStartColumn(int row, Vector<Floating, ComplexFloating> const &vector, int startColumn) const;
	ComplexFloating multiplyRowWithEndColumn(int row, Vector<Floating, ComplexFloating> const &vector, int endColumn) const;
	SparseMatrixRowIterator<ComplexFloating> getRowIterator(int row) const;
	SparseMatrixRowIterator<ComplexFloating> getRowIterator(int row, int startColumn) const;
	int findAbsoluteMaximumOfColumn(int column, int rowStart) const;
	void swapRows(int one, int two);
	std::vector<std::pair<int, ComplexFloating>> getRowValuesAndColumns(int row, int startColumn) const;
	void compress();
	void addWeightedRowElements(int row, ComplexFloating const &weight, std::vector<std::pair<int, ComplexFloating>> const &columnsAndValues);

	ComplexFloating const& operator()(int row, int column) const;
	SparseMatrix<Floating, ComplexFloating> const& operator=(SparseMatrix<Floating, ComplexFloating> const &rhs);

private:
	bool findPosition(int row, int column, int &position) const;
	bool isValidRowIndex(int row) const;
	bool isValidColumnIndex(int column) const;
	ComplexFloating multiply(Vector<Floating, ComplexFloating> const &vector, int startPosition, int endPosition, int row) const;

private:
	const int _rowCount;
	const int _columnCount;
	const ComplexFloating _zero;
	std::vector<std::vector<int>> _columns;
	std::vector<std::vector<ComplexFloating>> _values;
};

