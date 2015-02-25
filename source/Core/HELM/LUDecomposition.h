#pragma once

#include "ILinearEquationSystemSolver.h"
#include "SparseMatrix.h"
#include "IPivotFinder.h"

template<class Floating, class ComplexFloating>
class LUDecomposition :
	public ILinearEquationSystemSolver<Floating, ComplexFloating>
{
protected:
	LUDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix, IPivotFinder<Floating, ComplexFloating> *pivotFinder);

public:
	virtual ~LUDecomposition();
	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;

private:
	void calculateDecomposition(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);
	Vector<Floating, ComplexFloating> forwardSubstitution(Vector<Floating, ComplexFloating> const &b) const;
	Vector<Floating, ComplexFloating> backwardSubstitution(Vector<Floating, ComplexFloating> const &y) const;

private:
	const int _dimension;
	SparseMatrix<Floating, ComplexFloating> _left;
	SparseMatrix<Floating, ComplexFloating> _upper;
	SparseMatrix<Floating, ComplexFloating> _permutation;
	SparseMatrix<Floating, ComplexFloating> _permutationBandwidthReduction;
	SparseMatrix<Floating, ComplexFloating> _permutationBandwidthReductionInverse;
	const IPivotFinder<Floating, ComplexFloating> *_pivotFinder;
};

