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

protected:
	virtual int findPivotIndex(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const = 0;

private:
	void calculateDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);
	Vector<Floating, ComplexFloating> forwardSubstitution(Vector<Floating, ComplexFloating> const &b) const;
	Vector<Floating, ComplexFloating> backwardSubstitution(Vector<Floating, ComplexFloating> const &y) const;

private:
	const int _dimension;
	SparseMatrix<Floating, ComplexFloating> _left;
	SparseMatrix<Floating, ComplexFloating> _upper;
	SparseMatrix<Floating, ComplexFloating> _permutation;
	Vector<Floating, ComplexFloating> _preconditioner;
};

