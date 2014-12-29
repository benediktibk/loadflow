#include "Complex.h"

template class Complex<double>;
template class Complex<long double>;
template class Complex<MultiPrecision>;

template<typename T>
Complex<T>::Complex() :
	_real(0),
	_imag(0)
{ }

template<typename T>
Complex<T>::Complex(T const& real, T const& imag) :
	_real(real),
	_imag(imag)
{ }

template<typename T>
Complex<T>::Complex(T const& real) :
	_real(real),
	_imag(static_cast<T>(0))
{ }

template<typename T>
T const& Complex<T>::real() const
{
	return _real;
}

template<typename T>
T const& Complex<T>::imag() const
{
	return _imag;
}

template<typename T>
Complex<T>& Complex<T>::operator=(Complex<T> const& rhs)
{
	_real = rhs.real();
	_imag = rhs.imag();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator+=(Complex<T> const& rhs)
{
	_real += rhs.real();
	_imag += rhs.imag();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator-=(Complex<T> const& rhs)
{
	_real -= rhs.real();
	_imag -= rhs.imag();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator*=(Complex<T> const& rhs)
{
	*this = *this * rhs;
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator/=(Complex<T> const& rhs)
{
	*this = *this / rhs;
	return *this;
}

double toDouble(double value)
{
	return value;
}