#pragma once

#include "Matrix.h"
#include <Eigen/Core>
#include <Eigen/Sparse>
#include <Eigen/SparseLU>

template<typename T>
class Decomposition
{
public:
	Decomposition(Matrix<T> const &matrix);

	std::vector<T> solveEquationSystem(std::vector<T> const &rightSide);

private:
	Eigen::SparseLU<Eigen::SparseMatrix<T>, Eigen::NaturalOrdering<int> > _decomposition;
};

