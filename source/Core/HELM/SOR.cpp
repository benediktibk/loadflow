#include "SOR.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "NumericalTraits.h"
#include <assert.h>

template class SOR<long double, Complex<long double>>;
template class SOR<MultiPrecision, Complex<MultiPrecision>>;

template<class Floating, class ComplexFloating>
SOR<Floating, ComplexFloating>::SOR(const SparseMatrix<Floating, ComplexFloating> &systemMatrix, Floating epsilon, Floating omega, int maximumIterations) : 
	_epsilon(epsilon),
	_dimension(systemMatrix.getColumnCount()),
	_omega(omega),
	_maximumIterations(maximumIterations),
	_systemMatrix(systemMatrix),
	_preconditioner(_dimension, _dimension)
{
	assert(systemMatrix.getColumnCount() == systemMatrix.getRowCount());
	assert(omega > Floating(0) && omega < Floating(2));
	assert(maximumIterations > 0);

	auto diagonal = _systemMatrix.getInverseMainDiagonal();
	_systemMatrix.multiplyWithDiagonalMatrix(diagonal);

	for (auto i = 0; i < _dimension; ++i)
		_preconditioner.set(i, i, diagonal(i));	
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> SOR<Floating, ComplexFloating>::solve(const Vector<Floating, ComplexFloating> &b) const
{
	auto bPreconditioned = Vector<Floating, ComplexFloating>(b.getCount());
	_preconditioner.multiply(bPreconditioned, b);
	auto x = Vector<Floating, ComplexFloating>(_dimension);
	auto residual = Vector<Floating, ComplexFloating>(_dimension);
	auto epsilonSquared = _epsilon*_epsilon;
	auto bCurrent = Vector<Floating, ComplexFloating>(_dimension);
	auto iterations = 0;
	auto bSquaredNorm = bPreconditioned.squaredNorm();
	auto relativeError = epsilonSquared + Floating(1);

	do
	{
		for (auto i = 0; i < _dimension; ++i)
		{
			auto firstSummand = bPreconditioned(i);
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
		++iterations;

		if (iterations%128 == 0)
		{
			_systemMatrix.multiply(bCurrent, x);
			residual.subtract(bCurrent, bPreconditioned);
			relativeError = std::abs(residual.squaredNorm()/bSquaredNorm);
		}

		if (!x.isFinite())
			throw overflow_error("SOR did not converge to a proper value");
	} while(relativeError > epsilonSquared && iterations < _maximumIterations);	

	return x;
}