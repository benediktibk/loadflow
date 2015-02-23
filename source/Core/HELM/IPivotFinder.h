#pragma once

#include "SparseMatrix.h"

template<class Floating, class ComplexFloating>
class IPivotFinder
{
public:
	virtual ~IPivotFinder() { }

	virtual int operator()(SparseMatrix<Floating, ComplexFloating> const &upper, int row) const = 0;
};