#pragma once

#include <vector>
#include <Eigen\Sparse>
#include "Matrix.h"

template<class T>
class LinearEquationSystemSolver
{
public:
	LinearEquationSystemSolver(const Matrix<T> &systemMatrix);
	~LinearEquationSystemSolver();

	std::vector<T> solve(const std::vector<T> &b) const;

private:	
	Eigen::BiCGSTAB<Eigen::SparseMatrix<T>, Eigen::DiagonalPreconditioner<T> > *_solver;
};

