#pragma once

#include <complex>
#include <Eigen/Core>
#include <ostream>
#include "MultiPrecision.h"

template<typename T>
class Complex
{
public:
	Complex();
	Complex(T const& real, T const& imag);
	explicit Complex(T const& real);
	Complex(int real);
	explicit Complex(std::complex<double> const& rhs);

	T const& real() const;
	T const& imag() const;
	
	operator std::complex<double>() const;
	const Complex<T> operator+(Complex<T> const& rhs) const;
	const Complex<T> operator-(Complex<T> const& rhs) const;
	const Complex<T> operator*(Complex<T> const& rhs) const;
	const Complex<T> operator/(Complex<T> const& rhs) const;
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
	stream << "(" << value.real() << ", " << value.imag() << ")";
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

namespace Eigen 
{
	template<> struct NumTraits< Complex<long double> > : NumTraits<double>
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

	template<> struct NumTraits< Complex<MultiPrecision> >
	{
		typedef MultiPrecision Real;
		typedef MultiPrecision NonInteger;
		typedef Complex<MultiPrecision> Nested;

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

		static inline Real epsilon() 
		{ 
			return static_cast<Real>(std::numeric_limits<long double>::epsilon()); 
		}

		static inline Real dummy_precision()
		{
			return Real(1e-30);
		}
	};
}