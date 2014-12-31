#pragma once

#include "ILinearEquationSystemSolver.h"
#include "SparseMatrix.h"

template<class Floating, class ComplexFloating>
class LUDecomposition :
	public ILinearEquationSystemSolver<Floating, ComplexFloating>
{
public:
	LUDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);

	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;

private:
	void calculateDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);

private:
	const int _dimension;
	SparseMatrix<Floating, ComplexFloating> _left;
	SparseMatrix<Floating, ComplexFloating> _upper;
	SparseMatrix<Floating, ComplexFloating> _permutation;
};

