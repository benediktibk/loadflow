#pragma once

#include "SparseMatrix.h"
#include "Vector.h"

template<class ComplexFloating, class Floating>
class LinearEquationSystemSolver
{
public:
	LinearEquationSystemSolver(const SparseMatrix<ComplexFloating> &systemMatrix, Floating epsilon);

	Vector<ComplexFloating> solve(const Vector<ComplexFloating> &b) const;

private:	
	const int _dimension;
	SparseMatrix<ComplexFloating> const &_systemMatrix;
	SparseMatrix<ComplexFloating> _preconditioner;
};

