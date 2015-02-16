#include "SOR.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "NumericalTraits.h"
#include <assert.h>

template class SOR<long double, Complex<long double>>;
template class SOR<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
SOR<Floating, ComplexFloating>::SOR(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon, Floating omega) : 
	_epsilon(epsilon),
	_dimension(systemMatrix.getColumnCount()),
	_omega(omega),
	_systemMatrix(systemMatrix)
{
	assert(systemMatrix.getColumnCount() == systemMatrix.getRowCount());
	assert(omega > Floating(0) && omega < Floating(2));
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> SOR<Floating, ComplexFloating>::solve(const Vector<Floating, ComplexFloating> &b) const
{
	auto x = Vector<Floating, ComplexFloating>(_dimension);
	auto residual = Vector<Floating, ComplexFloating>(_dimension);
	auto epsilonSquared = _epsilon*_epsilon;
	auto bCurrent = Vector<Floating, ComplexFloating>(_dimension);

	do
	{
		for (auto i = 0; i < _dimension; ++i)
		{
			auto firstSummand = b(i);
			auto rowIterator = _systemMatrix.getRowIterator(i);

			for (;rowIterator.getColumn() < i; rowIterator.next())
				firstSummand -= rowIterator.getValue()*x(rowIterator.getColumn());

			auto diagonalValue = rowIterator.getValue();
			rowIterator.next();
			
			for (;rowIterator.isValid(); rowIterator.next())
				firstSummand -= rowIterator.getValue()*x(rowIterator.getColumn());

			firstSummand *= ComplexFloating(_omega)/diagonalValue;
			auto secondSummand = ComplexFloating((Floating(1) - _omega))*x(i);
			x.set(i, firstSummand + secondSummand);
		}

		_systemMatrix.multiply(bCurrent, x);
		residual.subtract(bCurrent, b);
	} while(std::abs(residual.squaredNorm()) > epsilonSquared);

	return x;
}