#include "Decomposition.h"
#include <complex>
#include "Complex.h"
#include "MultiPrecision.h"

template class Decomposition< std::complex<long double> >;
template class Decomposition< Complex<MultiPrecision> >;

template<typename T>
Decomposition<T>::Decomposition(Matrix<T> const &matrix)
{
	_decomposition.compute(matrix.getValues());
}

template<typename T>
std::vector<T> Decomposition<T>::solveEquationSystem(std::vector<T> const &rightSide)
{
	return Matrix<T>::eigenToStdVector(_decomposition.solve(Matrix<T>::stdToEigenVector(rightSide)));
}
