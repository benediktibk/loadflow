#include "ComplexGreaterCompare.h"
#include "Complex.h"
#include "MultiPrecision.h"

template class ComplexGreaterCompare<long double, long double>;
template class ComplexGreaterCompare<long double, Complex<long double> >;
template class ComplexGreaterCompare<MultiPrecision, Complex<MultiPrecision> >;

namespace std
{
	long double abs2(long double x)
	{
		return x;
	}

	long double abs2(Complex<long double> const &x)
	{
		auto real = x.real();
		auto imag = x.imag();
		return real*real + imag*imag;
	}
}

template<class Floating, class ComplexFloating>
bool ComplexGreaterCompare<Floating, ComplexFloating>::operator()(ComplexFloating const &a, ComplexFloating const &b) const
{
	return std::abs2(a) > std::abs2(b);
}
