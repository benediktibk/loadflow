#pragma once
#include "SparseMatrix.h"
#include "Vector.h"
#include "ILinearEquationSystemSolver.h"

template<class Floating, class ComplexFloating>
class SOR : public ILinearEquationSystemSolver<Floating, ComplexFloating>
{
public:
	SOR(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon, Floating omega);

	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const;

private:	
	const Floating _epsilon;
	const int _dimension;
	const Floating _omega;
	SparseMatrix<Floating, ComplexFloating> const &_systemMatrix;
};

