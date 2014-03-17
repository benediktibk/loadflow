#pragma once

#include <complex>
#include <Eigen/Core>
#include <ostream>

template<typename T>
class Complex
{
public:
	Complex();
	Complex(T const& real, T const& imag);
	Complex(T const& real);
	Complex(std::complex<double> rhs);
	Complex(std::complex<T> rhs);

	T const& real() const;
	T const& imag() const;

	Complex<T>& operator=(Complex<T> const& rhs);
	Complex<T>& operator+=(Complex<T> const& rhs);
	Complex<T>& operator-=(Complex<T> const& rhs);
	Complex<T>& operator*=(Complex<T> const& rhs);
	Complex<T>& operator/=(Complex<T> const& rhs);
	std::complex<T> const& getValue() const;

private:
	std::complex<T> _value;
};

template<typename T>
const Complex<T> operator+(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.getValue() + rhs.getValue());
}

template<typename T>
const Complex<T> operator-(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.getValue() - rhs.getValue());
}

template<typename T>
const Complex<T> operator*(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.getValue() * rhs.getValue());
}

template<typename T>
const Complex<T> operator/(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return Complex<T>(lhs.getValue() / rhs.getValue());
}

template<typename T>
bool operator==(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return lhs.getValue() == rhs.getValue();
}

template<typename T>
bool operator!=(Complex<T> const& lhs, Complex<T> const& rhs)
{
	return lhs.getValue() != rhs.getValue();
}

template<typename T>
std::ostream& operator<<(std::ostream &stream, Complex<T> const& value)
{
	stream << value.getValue();
	return stream;
}

namespace std
{
	template<typename T>
	const Complex<T> conj(Complex<T> const& value)
	{
		return Complex<T>(conj(value.getValue()));
	}

	template<typename T>
	const T abs(Complex<T> const& value)
	{
		return abs(value.getValue());
	}

	template<typename T>
	const T abs2(Complex<T> const& value)
	{
		return abs2(value.getValue());
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

namespace Eigen 
{
	template<> struct NumTraits< Complex<long double> > : NumTraits<long double>
	{
		typedef long double Real;
		typedef long double NonInteger;
		typedef Complex<long double> Nested;

		enum 
		{
			IsComplex = 1,
			IsInteger = 0,
			IsSigned = 1,
			RequireInitialization = 1,
			ReadCost = 1,
			AddCost = 3,
			MulCost = 3
		};
	};
}
