#pragma once

#include "SparseMatrix.h"
#include "Vector.h"

template<class ComplexFloating, class Floating>
class LinearEquationSystemSolver
{
public:
	LinearEquationSystemSolver(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon);

	Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;

private:	
	const int _dimension;
	SparseMatrix<Floating, ComplexFloating> const &_systemMatrix;
	SparseMatrix<Floating, ComplexFloating> _preconditioner;
};

