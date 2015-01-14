#include "SparseMatrix.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "SparseMatrixRowIterator.h"
#include <algorithm>
#include <functional>
#include <list>
#include <utility>
#include <exception>

template class SparseMatrix<long double, Complex<long double> >;
template class SparseMatrix<MultiPrecision, Complex<MultiPrecision> >;

template<class Floating, class ComplexFloating>
SparseMatrix<Floating, ComplexFloating>::SparseMatrix(int rows, int columns) :
	_rowCount(rows),
	_columnCount(columns),
	_zero(Floating(0), Floating(0))
{
	if (getRowCount() <= 0)
		throw std::range_error("row count must be positive");
	if (getColumnCount() <= 0)
		throw std::range_error("column count must be positive");

	_columns.resize(getRowCount(), std::vector<int>());
	_values.resize(getRowCount(), std::vector<ComplexFloating>());
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::getRowCount() const
{
	return _rowCount;
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::getColumnCount() const
{
	return _columnCount;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::set(int row, int column, ComplexFloating const &value)
{
	if (!isValidRowIndex(row))
		throw std::range_error("invalid row index");
	if (!isValidColumnIndex(column))
		throw std::range_error("invalid column index");

	int position;
	std::vector<ComplexFloating> &values = _values[row];

	if (findPosition(row, column, position))
	{
		values[position] = value;
		return;
	}

	std::vector<int> &columns = _columns[row];

	columns.insert(columns.begin() + position, column);
	values.insert(values.begin() + position, value);
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::multiply(Vector<Floating, ComplexFloating> &destination, Vector<Floating, ComplexFloating> const &source) const
{
	if (destination.getCount() != getRowCount() || source.getCount() != getColumnCount())
		throw std::invalid_argument("sizes of vector and matrix do not match");

	#pragma omp parallel
	{
		std::vector<Floating> summandsReal;
		std::vector<Floating> summandsImaginary;

		#pragma omp for
		for (auto i = 0; i < _rowCount; ++i)
		{
			const std::vector<int> &columns = _columns[i];
			const std::vector<ComplexFloating> &values = _values[i];
			const int count = values.size();
			summandsReal.clear();
			summandsImaginary.clear();
			summandsReal.reserve(count);
			summandsImaginary.reserve(count);

			for (auto j = 0; j < count; ++j)
			{
				auto column = columns[j];
				ComplexFloating const &value = values[j];
				auto summand = value*source(column);
				summandsReal.push_back(std::real(summand));
				summandsImaginary.push_back(std::imag(summand));
			}

			std::sort(summandsReal.begin(), summandsReal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
			std::sort(summandsImaginary.begin(), summandsImaginary.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
			ComplexFloating result;

			for (auto j = 0; j < count; ++j)
				result += ComplexFloating(summandsReal[j], summandsImaginary[j]);

			destination.set(i, result);
		}
	}
}

template<class Floating, class ComplexFloating>
ComplexFloating SparseMatrix<Floating, ComplexFloating>::multiplyRowWithStartColumn(int row, Vector<Floating, ComplexFloating> const &vector, int startColumn) const
{
	int startPosition;
	findPosition(row, startColumn, startPosition);
	auto endPosition = _columns[row].size();
	return multiply(vector, startPosition, endPosition, row);
}

template<class Floating, class ComplexFloating>
ComplexFloating SparseMatrix<Floating, ComplexFloating>::multiplyRowWithEndColumn(int row, Vector<Floating, ComplexFloating> const &vector, int endColumn) const
{
	auto startPosition = 0;
	int endPosition;
	if (findPosition(row, endColumn, endPosition))
		++endPosition;
	return multiply(vector, startPosition, endPosition, row);
}

template<class Floating, class ComplexFloating>
SparseMatrixRowIterator<ComplexFloating> SparseMatrix<Floating, ComplexFloating>::getRowIterator(int row) const
{
	return getRowIterator(row, 0);
}

template<class Floating, class ComplexFloating>
SparseMatrixRowIterator<ComplexFloating> SparseMatrix<Floating, ComplexFloating>::getRowIterator(int row, int startColumn) const
{
	if (!isValidRowIndex(row))
		throw std::range_error("invalid row index");
	if (!isValidColumnIndex(startColumn))
		throw std::range_error("invalid column index");
	int startPosition;
	findPosition(row, startColumn, startPosition);
	return SparseMatrixRowIterator<ComplexFloating>(_values[row], _columns[row], startPosition, _columns[row].size());
}

template<class Floating, class ComplexFloating>
int SparseMatrix<Floating, ComplexFloating>::findAbsoluteMaximumOfColumn(int column, int rowStart) const
{
	if (!isValidRowIndex(rowStart))
		throw std::range_error("invalid row index");
	if (!isValidColumnIndex(column))
		throw std::range_error("invalid column index");
	std::list<std::pair<int, Floating>> candidates;

	#pragma omp parallel
	{
		auto maximumPartialRow = -1;
		auto maximumPartialValue = Floating(0);

		#pragma omp for
		for (auto row = rowStart; row < _rowCount; ++row)
		{
			auto value = std::abs2(operator()(row, column));
		
			if (value >= maximumPartialValue)
			{
				maximumPartialRow = row;
				maximumPartialValue = value;
			}
		}

		#pragma omp critical
		{
			candidates.push_back(std::pair<int, Floating>(maximumPartialRow, maximumPartialValue));
		}
	}

	auto maximumRow = candidates.front().first;
	auto maximumValue = candidates.front().second;

	for (auto i = ++candidates.begin(); i != candidates.end(); ++i)
		if (i->second > maximumValue)
		{
			maximumValue = i->second;
			maximumRow = i->first;
		}
		
	if (!isValidRowIndex(maximumRow))
		throw std::range_error("maximum row is invalid row index");

	return maximumRow;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::swapRows(int one, int two)
{
	if (!isValidRowIndex(one))
		throw std::range_error("invalid row index");
	if (!isValidRowIndex(two))
		throw std::range_error("invalid row index");
	
	if (one == two)
		return;

	if (one > two)
		std::swap(one, two);

	_values[one].swap(_values[two]);
	_columns[one].swap(_columns[two]);
}

template<class Floating, class ComplexFloating>
std::vector<std::pair<int, ComplexFloating>> SparseMatrix<Floating, ComplexFloating>::getRowValuesAndColumns(int row, int startColumn) const
{
	if (!isValidRowIndex(row))
		throw std::range_error("invalid row index");
	if (!isValidColumnIndex(startColumn))
		throw std::range_error("invalid column index");

	int startPosition;
	findPosition(row, startColumn, startPosition);
	const std::vector<int> &columns = _columns[row];
	const std::vector<ComplexFloating> &values = _values[row];
	int endPosition = columns.size();
	std::vector<std::pair<int, ComplexFloating>> result(endPosition - startPosition);

	#pragma omp parallel for
	for (auto i = startPosition; i < endPosition; ++i)
		result[i - startPosition] = std::pair<int, ComplexFloating>(columns[i], values[i]);

	return result;
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::compress()
{
	#pragma omp parallel
	{
		std::vector<int> tempColumns;
		std::vector<ComplexFloating> tempValues;

		#pragma omp for
		for (auto row = 0; row < _rowCount; ++row)
		{
			std::vector<int> &columns = _columns[row];
			std::vector<ComplexFloating> &values = _values[row];
			const int count = columns.size();
			tempColumns.clear();
			tempValues.clear();
			tempColumns.reserve(count);
			tempValues.reserve(count);

			for (auto i = 0; i < count; ++i)
			{
				ComplexFloating const &value = values[i];

				if (value == _zero)
					continue;

				tempValues.push_back(value);
				tempColumns.push_back(columns[i]);
			}

			tempValues.swap(values);
			tempColumns.swap(columns);
		}
	}
}

template<class Floating, class ComplexFloating>
void SparseMatrix<Floating, ComplexFloating>::addWeightedRowElements(int row, ComplexFloating const &weight, std::vector<std::pair<int, ComplexFloating>> const &columnsAndValues)
{
	if (!isValidRowIndex(row))
		throw std::range_error("invalid row index");

	std::vector<ComplexFloating> leftOverValues;
	std::vector<int> leftOverColumns;
	leftOverValues.reserve(columnsAndValues.size());
	leftOverColumns.reserve(columnsAndValues.size());
	std::vector<int> &columns = _columns[row];
	std::vector<ComplexFloating> &values = _values[row];
	auto position = 0;
	const int count = columns.size();

	for (auto i = columnsAndValues.cbegin(); i != columnsAndValues.end(); ++i)
	{
		auto column = i->first;

		if (!isValidColumnIndex(column))
			throw std::range_error("invalid column index");

		while(position < count && columns[position] < column)
			++position;

		if (position < count && columns[position] == column)
			values[position] += weight*i->second;
		else
		{
			leftOverValues.push_back(i->second);
			leftOverColumns.push_back(column);
		}
	}

	int currentValueCount = values.size();
	int additionalValueCount = leftOverValues.size();
	values.reserve(currentValueCount + additionalValueCount);
	columns.reserve(currentValueCount + additionalValueCount);

	for (auto i = 0; i < additionalValueCount; ++i)
		set(row, leftOverColumns[i], weight*leftOverValues[i]);
}

template<class Floating, class ComplexFloating>
ComplexFloating const& SparseMatrix<Floating, ComplexFloating>::operator()(int row, int column) const
{
	int position;
	if (findPosition(row, column, position))
		return _values[row][position];

	return _zero;
}

template<class Floating, class ComplexFloating>
SparseMatrix<Floating, ComplexFloating> const& SparseMatrix<Floating, ComplexFloating>::operator=(SparseMatrix<Floating, ComplexFloating> const &rhs)
{
	if (getRowCount() != rhs.getRowCount() || getColumnCount() != rhs.getColumnCount())
		throw std::invalid_argument("sizes of matrices do not match");
	_columns = rhs._columns;
	_values = rhs._values;
	return *this;
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::findPosition(int row, int column, int &position) const
{
	if (!isValidRowIndex(row))
		throw std::range_error("invalid row index");
	if (!isValidColumnIndex(column))
		throw std::range_error("invalid column index");

	const std::vector<int> &columns = _columns[row];
	const int count = columns.size();
	auto start = 0;
	auto end = count;

	if (end == start)
	{
		position = end;
		return false;
	}

	auto rangeBecameSmaller = true;
	auto previousRangeSize = end - start;

	while(rangeBecameSmaller)
	{
		auto middle = (start + end)/2;
		auto middleColumn = columns[middle];

		if (middleColumn < column)
			start = middle;
		else
			end = middle;

		auto rangeSize = end - start;

		rangeBecameSmaller = rangeSize < previousRangeSize;
		previousRangeSize = rangeSize;
	}

	position = end;
	return position < count && column == columns[position];
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::isValidRowIndex(int row) const
{
	return row >= 0 && row < getRowCount();
}

template<class Floating, class ComplexFloating>
bool SparseMatrix<Floating, ComplexFloating>::isValidColumnIndex(int column) const
{
	return column >= 0 && column < getColumnCount();
}

template<class Floating, class ComplexFloating>
ComplexFloating SparseMatrix<Floating, ComplexFloating>::multiply(Vector<Floating, ComplexFloating> const &vector, int startPosition, int endPosition, int row) const
{	
	auto count = endPosition - startPosition;
	std::vector<Floating> summandsRealTotal;
	std::vector<Floating> summandsImaginaryTotal;
	ComplexFloating resultTotal(Floating(0));
	summandsRealTotal.reserve(count);
	summandsImaginaryTotal.reserve(count);
	const std::vector<int> &columns = _columns[row];
	const std::vector<ComplexFloating> &values = _values[row];

	#pragma omp parallel
	{
		std::vector<Floating> summandsReal;
		std::vector<Floating> summandsImaginary;
		summandsReal.reserve(count);
		summandsImaginary.reserve(count);

		#pragma omp for
		for (auto i = startPosition; i < endPosition; ++i)
		{
			auto column = columns[i];
			auto summand = values[i]*vector(column);
			summandsReal.push_back(std::real(summand));
			summandsImaginary.push_back(std::imag(summand));
		}

		#pragma omp critical
		{
			summandsRealTotal.insert(summandsRealTotal.begin(), summandsReal.begin(), summandsReal.end());
			summandsImaginaryTotal.insert(summandsImaginaryTotal.begin(), summandsImaginary.begin(), summandsImaginary.end());
		}
	}

	std::sort(summandsRealTotal.begin(), summandsRealTotal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
	std::sort(summandsImaginaryTotal.begin(), summandsImaginaryTotal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });

	#pragma omp parallel
	{
		ComplexFloating result(Floating(0));

		#pragma omp for
		for (auto j = 0; j < count; ++j)
			result += ComplexFloating(summandsRealTotal[j], summandsImaginaryTotal[j]);

		#pragma omp critical
		{
			resultTotal += result;
		}
	}

	return resultTotal;
}
