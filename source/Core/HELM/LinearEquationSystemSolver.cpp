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
	Vector r = bConverted - _systemMatrix * x;
	auto r0 = r;  
	auto r0_sqnorm = r0.squaredNorm();
	auto rhs_sqnorm = bConverted.squaredNorm();

	if(rhs_sqnorm == Floating(0))
		return b;

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

	auto tol2 = _epsilon*_epsilon;
	auto i = 0;
	auto restarts = 0;

	while (r.squaredNorm()/rhs_sqnorm > tol2 && i < maxIters)
	{
		auto rho_old = rho;
		rho = r0.dot(r);

		if (abs(abs(rho) - r0_sqnorm) < Floating(1e-20))
		{
			r0 = r;
			r0_sqnorm = r.squaredNorm();
			rho = ComplexFloating(r0_sqnorm);
			if(restarts++ == 0)
				i = 0;
		}

		auto beta = (rho/rho_old) * (alpha / w);
		p = r + beta * (p - w * v);    
		y = _preconditioner*p;    
		v = _systemMatrix * y;
		alpha = rho / r0.dot(v);
		s = r - alpha * v;
		z = _preconditioner*s;
		t = _systemMatrix * z;

		auto tmp = t.squaredNorm();
		if(tmp > Floating(0))
			w = t.dot(s) / ComplexFloating(tmp);
		else
			w = ComplexFloating(0);

		x += alpha * y + w * z;
		r = s - w * t;
		++i;
	}

	return Matrix<ComplexFloating>::eigenToStdVector(x);
}