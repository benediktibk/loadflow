#pragma once

#include "LUDecomposition.h"

template<class Floating, class ComplexFloating>
class LUDecompositionSparse :
	public LUDecomposition<Floating, ComplexFloating>
{
public:
	LUDecompositionSparse(SparseMatrix<Floating, ComplexFloating> const &systemMatrix);
};

