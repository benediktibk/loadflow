#include "Complex.h"
#include "MultiPrecision.h"

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
	_imag(0)
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
const Complex<T> operator+(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.real() + rhs.real(), lhs.imag() + rhs.imag());
}

template<typename T>
const Complex<T> operator-(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.real() - rhs.real(), lhs.imag() - rhs.imag());
}

template<typename T>
const Complex<T> operator*(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.real()*rhs.real() - lhs.imag()*rhs.imag(), lhs.imag()*rhs.real() + lhs.real()*rhs.imag());
}

template<typename T>
const Complex<T> operator/(Complex<T> const& lhs, Complex<T> const& rhs)
{
	T divisor = rhs.real()*rhs.real() + rhs.imag()*rhs.imag();
	T real = lhs.real()*rhs.real() + lhs.imag()*rhs.imag();
	T imag = rhs.real()*lhs.imag() - rhs.real()*lhs.imag();
	return Complex<T>(real/divisor, imag/divisor);
}

template<typename T>
bool operator==(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return	lhs.real() == rhs.real() &&
			lhs.imag() == rhs.imag();
}

template<typename T>
bool operator!=(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return	lhs.real() != rhs.real() ||
			lhs.imag() != rhs.imag();
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
	*this = *this * rhs;
	return *this;
}
