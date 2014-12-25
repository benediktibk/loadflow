#include "LinearEquationSystemSolver.h"
#include "Complex.h"
#include "MultiPrecision.h"

using namespace std;

template class LinearEquationSystemSolver< std::complex<long double> >;
template class LinearEquationSystemSolver< Complex<MultiPrecision> >;

template<class T>
LinearEquationSystemSolver<T>::LinearEquationSystemSolver(const Matrix<T> &systemMatrix) :
	_solver(new Eigen::BiCGSTAB<Eigen::SparseMatrix<T>, Eigen::DiagonalPreconditioner<T> >(systemMatrix.getValues()))
{ }

template<class T>
LinearEquationSystemSolver<T>::~LinearEquationSystemSolver()
{
	delete _solver;
	_solver = 0;
}

template<class T>
vector<T> LinearEquationSystemSolver<T>::solve(const vector<T> &b) const
{	
	return Matrix<T>::eigenToStdVector(_solver->solve(Matrix<T>::stdToEigenVector(b)));
}