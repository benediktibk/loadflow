#include "Matrix.h"
#include <complex>
#include "Complex.h"
#include "MultiPrecision.h"
#include <assert.h>

template class Matrix< std::complex<long double> >;
template class Matrix< Complex<MultiPrecision> >;

template<typename T>
Matrix<T>::Matrix(int rows, int columns) :
	_rows(rows),
	_columns(columns),
	_values(rows, columns)
{
	assert(_rows >= 0);
	assert(_columns >= 0);
}

template<typename T>
void Matrix<T>::setValue(int row, int column, T const& value)
{
	assert(row >= 0);
	assert(row < _rows);
	assert(column >= 0);
	assert(column < _columns);
	_values.coeffRef(row, column) = value;
}
	
template<typename T>
std::vector<T> Matrix<T>::multiply(std::vector<T> const& rhs) const
{
	Eigen::Matrix<T, Eigen::Dynamic, 1> rhsConverted = stdToEigenVector(rhs);
	Eigen::Matrix<T, Eigen::Dynamic, 1> result = _values * rhsConverted;
	return eigenToStdVector(result);
}

template<typename T>
Eigen::SparseMatrix<T, Eigen::ColMajor > const& Matrix<T>::getValues() const
{
	return _values;
}

template<typename T>
T Matrix<T>::getValue(int row, int column) const
{
	return _values.coeff(row, column);
}

template<typename T>
std::vector<T> Matrix<T>::pointwiseMultiply(const std::vector<T> &one, const std::vector<T> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<T> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]*two[i];

	return result;
}

template<typename T>
std::vector<T> Matrix<T>::pointwiseDivide(const std::vector<T> &one, const std::vector<T> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<T> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i]/two[i];

	return result;
}

template<typename T>
std::vector<T> Matrix<T>::add(const std::vector<T> &one, const std::vector<T> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<T> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] + two[i];
	
	return result;
}

template<typename T>
std::vector<T> Matrix<T>::subtract(const std::vector<T> &one, const std::vector<T> &two)
{
	assert(one.size() == two.size());
	size_t n = one.size();
	std::vector<T> result(n);

	for (size_t i = 0; i < n; ++i)
		result[i] = one[i] - two[i];
	
	return result;
}

template<typename T>
std::vector<T> Matrix<T>::multiply(const std::vector<T> &one, const T &two)
{
	std::vector<T> result(one);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] *= two;

	return result;
}

template<typename T>
std::vector<T> Matrix<T>::divide(const T &one, const std::vector<T> &two)
{
	std::vector<T> result(two);

	for (size_t i = 0; i < result.size(); ++i)
		result[i] = one/result[i];

	return result;
}

template<typename T>
std::vector<T> Matrix<T>::eigenToStdVector(const Eigen::Matrix<T, Eigen::Dynamic, 1> &values)
{
	std::vector<T> result(values.size());

	for (int i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename T>
Eigen::Matrix<T, Eigen::Dynamic, 1> Matrix<T>::stdToEigenVector(const std::vector<T> &values)
{
	Eigen::Matrix<T, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = values[i];

	return result;
}

template<typename T>
Eigen::Matrix<T, Eigen::Dynamic, 1> Matrix<T>::stdToEigenVector(const std::vector< std::complex<double> > &values)
{
	Eigen::Matrix<T, Eigen::Dynamic, 1> result(values.size(), 1);

	for (size_t i = 0; i < values.size(); ++i)
		result[i] = static_cast<T>(values[i]);

	return result;
}

template<typename T>
std::vector<T> Matrix<T>::stdComplexVectorToComplexFloatingVector(const std::vector< std::complex<double> > &values)
{
	std::vector<T> result(values.size());
	
	for (size_t i = 0; i < values.size(); ++i)
		result[i] = static_cast<T>(values[i]);

	return result;
}