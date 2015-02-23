#pragma once

#include "IPivotFinder.h"

template<class Floating, class ComplexFloating>
class PivotFinderBiggestElement :
	public IPivotFinder<Floating, ComplexFloating>
{
public:
	virtual int operator()(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const;
};

