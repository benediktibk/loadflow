#pragma once

#include <vector>
#include <Eigen/Core>
#include <Eigen/Sparse>

template<typename T>
class Matrix
{
public:
	Matrix(int rows, int columns);
	
	void setValue(int row, int column, T const& value);
	std::vector<T> calculateRowSums() const;
	std::vector<T> multiply(std::vector<T> const& rhs) const;
	Eigen::SparseMatrix<T, Eigen::ColMajor > const& getValues() const;
	T getValue(int row, int column) const;

public:
	static std::vector<T> pointwiseMultiply(const std::vector<T> &one, const std::vector<T> &two);
	static std::vector<T> pointwiseDivide(const std::vector<T> &one, const std::vector<T> &two);
	static std::vector<T> add(const std::vector<T> &one, const std::vector<T> &two);
	static std::vector<T> subtract(const std::vector<T> &one, const std::vector<T> &two);
	static std::vector<T> multiply(const std::vector<T> &one, const T &two);
	static std::vector<T> divide(const T &one, const std::vector<T> &two);
	static std::vector<T> eigenToStdVector(const Eigen::Matrix<T, Eigen::Dynamic, 1> &values);
	static Eigen::Matrix<T, Eigen::Dynamic, 1> stdToEigenVector(const std::vector<T> &values);
	static Eigen::Matrix<T, Eigen::Dynamic, 1> stdToEigenVector(const std::vector< std::complex<double> > &values);
	static std::vector<T> stdComplexVectorToComplexFloatingVector(const std::vector< std::complex<double> > &values);

private:
	const int _rows;
	const int _columns;
	Eigen::SparseMatrix<T, Eigen::ColMajor > _values;
};

