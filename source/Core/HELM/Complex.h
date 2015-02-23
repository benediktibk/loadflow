#pragma once

#include <iostream>
#include <sstream>
#include <string>
#include "MultiPrecision.h"

template<typename T>
class Complex
{
public:
	Complex();
	Complex(T const& real, T const& imag);
	explicit Complex(T const& real);

	T const& real() const;
	T const& imag() const;
	
	Complex<T>& operator=(Complex<T> const& rhs);
	Complex<T>& operator+=(Complex<T> const& rhs);
	Complex<T>& operator-=(Complex<T> const& rhs);
	Complex<T>& operator*=(Complex<T> const& rhs);
	Complex<T>& operator/=(Complex<T> const& rhs);

private:
	T _real;
	T _imag;
};

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
	T imag = lhs.imag()*rhs.real() - lhs.real()*rhs.imag();
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
std::ostream& operator<<(std::ostream &stream, Complex<T> const& value)
{
	stream << "(" << value.real() << "," << value.imag() << ")";
	return stream;
}

template<typename T>
std::istream& operator>>(std::istream &stream, Complex<T> &value)
{
	string completeString;
	std::getline(stream, completeString, ')');
	auto comma = completeString.find(',');
	auto realString = stringstream(completeString.substr(1, comma - 1));
	auto imagString = stringstream(completeString.substr(comma + 1, completeString.size() - comma));
	T real;
	T imag;
	realString >> real;
	imagString >> imag;
	value = Complex<T>(real, imag);
	return stream;
}

namespace std
{
	template<typename T>
	const Complex<T> conj(Complex<T> const& value)
	{
		return Complex<T>(value.real(), -value.imag());
	}

	template<typename T>
	const T abs(Complex<T> const& value)
	{
		return sqrt(abs2(value));
	}

	template<typename T>
	const T abs2(Complex<T> const& value)
	{
		return value.real()*value.real() + value.imag()*value.imag();
	}

	template<typename T>
	const T real(Complex<T> const& value)
	{
		return value.real();
	}

	template<typename T>
	const T imag(Complex<T> const& value)
	{
		return value.imag();
	}
}