#include "LinearEquationSystemSolver.h"
#include "Complex.h"
#include "MultiPrecision.h"

using namespace std;

template class LinearEquationSystemSolver< std::complex<long double>, long double >;
template class LinearEquationSystemSolver< Complex<MultiPrecision>, MultiPrecision >;

template<class ComplexFloating, class Floating>
LinearEquationSystemSolver<ComplexFloating, Floating>::LinearEquationSystemSolver(const Matrix<ComplexFloating> &systemMatrix, Floating epsilon) :
	_epsilon(Eigen::NumTraits<ComplexFloating>::epsilon()),
	_nearlyZero(1e-100),
	_systemMatrix(systemMatrix.getValues()),
	_preconditioner(_systemMatrix.rows(), _systemMatrix.cols())
{
	for (auto i = 0; i < _systemMatrix.rows(); ++i)
	{
		auto diagonalValue = _systemMatrix.coeff(i, i);
		_preconditioner.coeffRef(i, i) = ComplexFloating(1)/diagonalValue;
	}
}

template<class ComplexFloating, class Floating>
LinearEquationSystemSolver<ComplexFloating, Floating>::~LinearEquationSystemSolver()
{
}

template<class ComplexFloating, class Floating>
vector<ComplexFloating> LinearEquationSystemSolver<ComplexFloating, Floating>::solve(const vector<ComplexFloating> &b) const
{	
	typedef Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> Vector;
	typedef Eigen::SparseMatrix<ComplexFloating, Eigen::ColMajor> SparseVector;
	auto n = b.size();
	auto bConverted = Matrix<ComplexFloating>::stdToEigenVector(b);
	Vector x = _preconditioner*bConverted;
	int maxIters = 2*n;
	Vector residual = bConverted - _systemMatrix * x;
	auto firstResidual = residual;  
	auto firstResidualSquaredNorm = firstResidual.squaredNorm();
	auto rhs_sqnorm = bConverted.squaredNorm();
	auto epsilonSquared = _epsilon*_epsilon;
	auto i = 0;
	auto restarts = 0;
	auto rho = ComplexFloating(1);
	auto alpha = ComplexFloating(1);
	auto w = ComplexFloating(1);  
	Vector v = SparseVector(n, 1);
	Vector p = SparseVector(n, 1);
	Vector y = SparseVector(n, 1);
	Vector z = SparseVector(n, 1);
	Vector kt = SparseVector(n, 1);
	Vector ks = SparseVector(n, 1);
	Vector s = SparseVector(n, 1);
	Vector t = SparseVector(n, 1);;

	if(rhs_sqnorm == Floating(0))
		return b;	

	while (residual.squaredNorm()/rhs_sqnorm > epsilonSquared && i < maxIters)
	{
		auto rho_old = rho;
		rho = firstResidual.dot(residual);

		if (abs(abs(rho) - firstResidualSquaredNorm) < Floating(1e-20))
		{
			firstResidual = residual;
			firstResidualSquaredNorm = residual.squaredNorm();
			rho = ComplexFloating(firstResidualSquaredNorm);
			i = 0;
			++restarts;

			if(restarts >= 5)
				break;
		}

		auto beta = (rho/rho_old) * (alpha / w);
		p = residual + beta * (p - w * v);    
		y = _preconditioner*p;    
		v = _systemMatrix * y;
		alpha = rho / firstResidual.dot(v);
		s = residual - alpha * v;
		z = _preconditioner*s;
		t = _systemMatrix * z;

		auto tmp = t.squaredNorm();
		if(tmp > Floating(0))
			w = t.dot(s) / ComplexFloating(tmp);
		else
			w = ComplexFloating(0);

		x += alpha * y + w * z;
		residual = s - w * t;
		++i;
	}

	return Matrix<ComplexFloating>::eigenToStdVector(x);
}