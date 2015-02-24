#pragma once

#include <vector>
#include <utility>
#include <iostream>
#include <sstream>
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
	std::vector<std::pair<int, ComplexFloating>> findNonZeroValuesInColumnWithSmallestElementCount(int column, int rowStart) const;
	void swapRows(int one, int two);
	std::vector<std::pair<int, ComplexFloating>> getRowValuesAndColumns(int row, int startColumn) const;
	void compress();
	void addWeightedRowElements(int row, ComplexFloating const &weight, std::vector<std::pair<int, ComplexFloating>> const &columnsAndValues);
	Vector<Floating, ComplexFloating> getInverseMainDiagonal() const;
	void multiplyWithDiagonalMatrix(Vector<Floating, ComplexFloating> const &diagonalValues);
	int calculateBandwidth() const;
	std::vector<int> reduceBandwidth() const;
	void transpose();

	ComplexFloating const& operator()(int row, int column) const;
	SparseMatrix<Floating, ComplexFloating> const& operator=(SparseMatrix<Floating, ComplexFloating> const &rhs);

private:
	bool findPosition(int row, int column, int &position) const;
	bool isValidRowIndex(int row) const;
	bool isValidColumnIndex(int column) const;
	ComplexFloating multiply(Vector<Floating, ComplexFloating> const &vector, int startPosition, int endPosition, int row) const;
	SparseMatrixRowIterator<ComplexFloating>* getRowIteratorPointer(int row) const;

private:
	const int _rowCount;
	const int _columnCount;
	const ComplexFloating _zero;
	std::vector<std::vector<int>> _columns;
	std::vector<std::vector<ComplexFloating>> _values;
};

template<class Floating, class ComplexFloating>
bool operator==(SparseMatrix<Floating, ComplexFloating> const &one, SparseMatrix<Floating, ComplexFloating> const &two)
{
	if (one.getRowCount() != two.getRowCount())
		return false;
	
	if (one.getColumnCount() != two.getColumnCount())
		return false;

	for (auto row = 0; row < one.getRowCount(); ++row)
	{
		auto iteratorOne = one.getRowIterator(row);
		auto iteratorTwo = two.getRowIterator(row);

		while(iteratorOne.isValid() && iteratorTwo.isValid())
		{
			if (iteratorOne.getColumn() != iteratorTwo.getColumn())
				return false;
			
			if (iteratorOne.getValue() != iteratorTwo.getValue())
				return false;

			iteratorOne.next();
			iteratorTwo.next();
		}

		if (iteratorOne.isValid() || iteratorTwo.isValid())
			return false;
	}

	return true;
}

template<class Floating, class ComplexFloating>
std::ostream& operator<<(std::ostream &stream, SparseMatrix<Floating, ComplexFloating> const &matrix)
{
	for (auto row = 0; row < matrix.getRowCount(); ++row)
		for (auto iterator = matrix.getRowIterator(row); iterator.isValid(); iterator.next())
			stream << row << ";" << iterator.getColumn() << ";" << iterator.getValue() << std::endl;

	return stream;
}

template<class Floating, class ComplexFloating>
std::istream& operator>>(std::istream &stream, SparseMatrix<Floating, ComplexFloating> &matrix)
{
	string nextLine;
	std::getline(stream, nextLine);

	while(nextLine.size() > 0)
	{
		size_t firstComma = nextLine.find(';');
		size_t secondComma = nextLine.find(';', firstComma + 1);
		auto rowString = stringstream(nextLine.substr(0, firstComma));
		auto columnString = stringstream(nextLine.substr(firstComma + 1, secondComma - firstComma - 1));
		auto valueString = stringstream(nextLine.substr(secondComma + 1, nextLine.size() - secondComma));
		int row;
		int column;
		ComplexFloating value;
		rowString >> row;
		columnString >> column;
		valueString >> value;
		matrix.set(row, column, value);
		std::getline(stream, nextLine);
	}

	return stream;
}

