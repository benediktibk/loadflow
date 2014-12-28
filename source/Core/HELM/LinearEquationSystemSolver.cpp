#include "LinearEquationSystemSolver.h"
#include "Complex.h"
#include "MultiPrecision.h"

template class LinearEquationSystemSolver< std::complex<long double>, long double >;
template class LinearEquationSystemSolver< Complex<MultiPrecision>, MultiPrecision >;

template<class ComplexFloating, class Floating>
LinearEquationSystemSolver<ComplexFloating, Floating>::LinearEquationSystemSolver(const SparseMatrix<ComplexFloating> &systemMatrix, Floating epsilon) :
	_dimension(systemMatrix.getRowCount()),
	_systemMatrix(systemMatrix),
	_preconditioner(_systemMatrix.getRowCount(), _systemMatrix.getColumnCount())
{
	assert(_systemMatrix.getRowCount() == _systemMatrix.getColumnCount());

	for (size_t i = 0; i < _dimension; ++i)
	{
		ComplexFloating const &diagonalValue = _systemMatrix(i, i);
		_preconditioner.set(i, i, ComplexFloating(1)/diagonalValue);
	}
}

template<class ComplexFloating, class Floating>
Vector<ComplexFloating> LinearEquationSystemSolver<ComplexFloating, Floating>::solve(const Vector<ComplexFloating> &b) const
{	
	Vector<ComplexFloating> x(_dimension);
	_preconditioner.multiply(x, b);
	int maximumIterations = 10*_dimension;
	Vector<ComplexFloating> residual(_dimension);
	Vector<ComplexFloating> temp(_dimension); 
	_systemMatrix.multiply(temp, x);
	residual.subtract(b, temp);
	auto firstResidual = residual;  
	auto firstResidualSquaredNorm = std::abs(firstResidual.squaredNorm());
	auto rhsSquaredNorm = b.squaredNorm();
	auto epsilon = Eigen::NumTraits<ComplexFloating>::epsilon();
	auto epsilonSquared = epsilon*epsilon;
	auto i = 0;
	auto restarts = 0;
	auto rho = ComplexFloating(1);
	auto alpha = ComplexFloating(1);
	auto w = ComplexFloating(1);  
	auto v = Vector<ComplexFloating>(_dimension);
	auto p = Vector<ComplexFloating>(_dimension);
	auto y = Vector<ComplexFloating>(_dimension);
	auto z = Vector<ComplexFloating>(_dimension);
	auto kt = Vector<ComplexFloating>(_dimension);
	auto ks = Vector<ComplexFloating>(_dimension);
	auto s = Vector<ComplexFloating>(_dimension);
	auto t = Vector<ComplexFloating>(_dimension);

	if(std::abs(rhsSquaredNorm) == Floating(0))
		return b;	

	while (std::abs(residual.squaredNorm()/rhsSquaredNorm) > epsilonSquared && i < maximumIterations)
	{
		auto rho_old = rho;
		rho = firstResidual.dot(residual);

		if (std::abs(std::abs(rho) - firstResidualSquaredNorm) < Floating(1e-20))
		{
			firstResidual = residual;
			firstResidualSquaredNorm = std::abs(residual.squaredNorm());
			rho = ComplexFloating(firstResidualSquaredNorm);
			i = 0;
			++restarts;

			if(restarts >= 5)
				break;
		}

		auto beta = (rho/rho_old)*(alpha/w);
		temp.weightedSum(p, w*ComplexFloating(-1), v);
		p.weightedSum(residual, beta, temp);
		_preconditioner.multiply(y, p);
		_systemMatrix.multiply(v, y);
		alpha = rho / firstResidual.dot(v);
		s.weightedSum(residual, alpha*ComplexFloating(-1), v);
		_preconditioner.multiply(z, s);
		_systemMatrix.multiply(t, z);

		auto tmp = std::abs(t.squaredNorm());
		if(tmp > Floating(0))
			w = t.dot(s) / ComplexFloating(tmp);
		else
			w = ComplexFloating(0);

		x.addWeightedSum(alpha, y, w, z);
		residual.weightedSum(s, w*ComplexFloating(-1), t);
		++i;
	}

	return x;
}