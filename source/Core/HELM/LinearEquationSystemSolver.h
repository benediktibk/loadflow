#pragma once

#include <vector>
#include <Eigen\Sparse>
#include "Matrix.h"

template<class ComplexFloating, class Floating>
class LinearEquationSystemSolver
{
public:
	LinearEquationSystemSolver(const Matrix<ComplexFloating> &systemMatrix, Floating epsilon);
	~LinearEquationSystemSolver();

	std::vector<ComplexFloating> solve(const std::vector<ComplexFloating> &b) const;

private:	
	const Floating _epsilon;
	Eigen::SparseMatrix<ComplexFloating, Eigen::ColMajor> const &_systemMatrix;
	Eigen::SparseMatrix<ComplexFloating, Eigen::ColMajor> _preconditioner;
};

