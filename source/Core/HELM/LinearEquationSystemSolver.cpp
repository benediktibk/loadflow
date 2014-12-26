#include "LinearEquationSystemSolver.h"
#include "Complex.h"
#include "MultiPrecision.h"

using namespace std;

template class LinearEquationSystemSolver< std::complex<long double>, long double >;
template class LinearEquationSystemSolver< Complex<MultiPrecision>, MultiPrecision >;

template<class ComplexFloating, class Floating>
LinearEquationSystemSolver<ComplexFloating, Floating>::LinearEquationSystemSolver(const Matrix<ComplexFloating> &systemMatrix, Floating epsilon) :
	_epsilon(epsilon),
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
	auto n = b.size();
	typedef Eigen::Matrix<ComplexFloating, Eigen::Dynamic, 1> Vector;
	typedef Eigen::SparseMatrix<ComplexFloating, Eigen::ColMajor> SparseVector;
	Vector x = SparseVector(n, 1);
	auto bConverted = Matrix<ComplexFloating>::stdToEigenVector(b);
	Vector residual = bConverted - _systemMatrix*x;
	auto firstResidual = residual;
	auto lastRho = ComplexFloating(1.0);
	auto omega = ComplexFloating(1.0);
	auto alpha = ComplexFloating(1.0);
	Vector p = SparseVector(n, 1);
	Vector v = SparseVector(n, 1);
	auto residualNormRelative = _epsilon + Floating(1);
	auto bNorm = bConverted.norm();

	if (bNorm <= Floating(0))
		bNorm = Floating(1);

	for (size_t i = 0; i < 2*n && residualNormRelative > _epsilon; ++i)
	{
		auto rho = firstResidual.dot(residual);

		if (i > 0)
		{
			if (abs(omega) < _nearlyZero || abs(lastRho) < _nearlyZero)
				return Matrix<ComplexFloating>::eigenToStdVector(x);

			auto beta = (rho/lastRho)*(alpha/omega);
			p = residual + beta*(p - omega*v);
		}
		else
			p = residual;

		auto pWithPreconditioner = _preconditioner*p;
		v = _systemMatrix*pWithPreconditioner;
		auto firstResidualDotV = firstResidual.dot(v);

		if (abs(firstResidualDotV) < _nearlyZero)
			return Matrix<ComplexFloating>::eigenToStdVector(x);

		alpha = rho/firstResidualDotV;
		auto s = residual - alpha*v;
		auto sWithPreconditioner = _preconditioner*s;
		auto t = _systemMatrix*sWithPreconditioner;
		auto tDotS = t.dot(s);

		if (tDotS == ComplexFloating(0))
			omega = ComplexFloating(0);
		else
		{
			auto tDotT = t.dot(t);

			if (abs(tDotT) < _nearlyZero)
				return Matrix<ComplexFloating>::eigenToStdVector(x);

			omega = tDotS/tDotT;
		}

		x = x + alpha*pWithPreconditioner + omega*sWithPreconditioner;
		residual = s - omega*t;
		lastRho = rho;
		residualNormRelative = residual.norm()/bNorm;;
	}

	return Matrix<ComplexFloating>::eigenToStdVector(x);
}