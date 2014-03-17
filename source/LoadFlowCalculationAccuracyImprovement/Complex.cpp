#include "Complex.h"
#include "MultiPrecision.h"

template class Complex<long double>;
template class Complex<MultiPrecision>;

template<typename T>
Complex<T>::Complex()
{ }

template<typename T>
Complex<T>::Complex(T const& real, T const& imag) :
	_value(real, imag)
{ }

template<typename T>
Complex<T>::Complex(T const& real) :
	_value(real, 0)
{ }

template<typename T>
Complex<T>::Complex(std::complex<double> rhs) :
	_value(rhs.real(), rhs.imag())
{ }

template<typename T>
Complex<T>::Complex(std::complex<T> rhs) :
	_value(rhs)
{ }

template<typename T>
T const& Complex<T>::real() const
{
	return _value.real();
}

template<typename T>
T const& Complex<T>::imag() const
{
	return _value.imag();
}

template<typename T>
Complex<T>& Complex<T>::operator=(Complex<T> const& rhs)
{
	_value = rhs.getValue();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator+=(Complex<T> const& rhs)
{
	_value += rhs.getValue();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator-=(Complex<T> const& rhs)
{
	_value -= rhs.getValue();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator*=(Complex<T> const& rhs)
{
	_value *= rhs.getValue();
	return *this;
}

template<typename T>
Complex<T>& Complex<T>::operator/=(Complex<T> const& rhs)
{
	_value /= rhs.getValue();
	return *this;
}

template<typename T>
std::complex<T> const& Complex<T>::getValue() const
{
	return _value;
}