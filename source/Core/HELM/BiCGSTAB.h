#pragma once

#include "SparseMatrix.h"
#include "Vector.h"
#include "ILinearEquationSystemSolver.h"

template<class Floating, class ComplexFloating>
class BiCGSTAB : public ILinearEquationSystemSolver<Floating, ComplexFloating>
{
public:
	BiCGSTAB(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon);

	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;

private:	
	const int _dimension;
	SparseMatrix<Floating, ComplexFloating> const &_systemMatrix;
	SparseMatrix<Floating, ComplexFloating> _preconditioner;
};

