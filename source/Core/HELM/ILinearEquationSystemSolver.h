#pragma once

#include "Vector.h"

template<class Floating, class ComplexFloating>
class ILinearEquationSystemSolver
{
public:
	~ILinearEquationSystemSolver() { }

	virtual Vector<Floating, ComplexFloating> solve(const Vector<Floating, ComplexFloating> &b) const = 0;
};