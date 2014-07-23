#include "Complex.h"

template class Complex<double>;
template class Complex<long double>;
template class Complex<MultiPrecision>;

template<typename T>
Complex<T>::Complex()
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
Complex<T>::Complex(std::complex<double> const& rhs) :
	_real(static_cast<T>(rhs.real())),
	_imag(static_cast<T>(rhs.imag()))
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
Complex<T>::operator std::complex<double>() const
{
	return std::complex<double>(static_cast<double>(_real), static_cast<double>(_imag));
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