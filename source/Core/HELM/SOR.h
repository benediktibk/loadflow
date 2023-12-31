#pragma once
#include "SparseMatrix.h"
#include "Vector.h"
#include "ILinearEquationSystemSolver.h"

template<class Floating, class ComplexFloating>
class SOR : public ILinearEquationSystemSolver<Floating, ComplexFloating>
{
public:
	SOR(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon, Floating omega, int maximumIterations);

	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;
	virtual Vector<Floating, ComplexFloating> solve(Vector<Floating, ComplexFloating> x, const Vector<Floating, ComplexFloating> &b) const;

private:	
	const Floating _epsilon;
	const int _dimension;
	const Floating _omega;
	const int _maximumIterations;
	SparseMatrix<Floating, ComplexFloating> _systemMatrix;
	SparseMatrix<Floating, ComplexFloating> _preconditioner;
};

