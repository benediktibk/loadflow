#pragma once

#include "LUDecomposition.h"

template<class Floating, class ComplexFloating>
class LUDecompositionStable : 
	public LUDecomposition<Floating, ComplexFloating>
{
public:
	LUDecompositionStable(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);

protected:
	virtual int findPivotIndex(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const;
};

