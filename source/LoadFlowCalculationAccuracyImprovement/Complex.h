#pragma once

#include <complex>
#include <Eigen/Core>
#include <ostream>
#include "MultiPrecision.h"
#include <Eigen/src/Core/MathFunctions.h>

template<typename T>
class Complex
{
public:
	Complex();
	Complex(T const& real, T const& imag);
	explicit Complex(T const& real);
	explicit Complex(std::complex<double> const& rhs);

	T const& real() const;
	T const& imag() const;
	
	operator std::complex<double>() const;
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
			return static_cast<Real>(std::pow(2, static_cast<double>(MultiPrecision::getBitPrecision())*(-1))); 
		}

		static inline Real dummy_precision()
		{
			return static_cast<Real>(std::pow(2, static_cast<double>(MultiPrecision::getBitPrecision() - 4)*(-1))); 
		}
	};

	namespace internal
	{
		template<>
		struct abs2_impl< Complex<long double> >
		{
		  static inline long double run(const Complex<long double>& x)
		  {
			return x.real()*x.real() + x.imag()*x.imag();
		  }
		};

		template<>
		struct abs2_impl< Complex<MultiPrecision> >
		{
		  static inline MultiPrecision run(const Complex<MultiPrecision>& x)
		  {
			return x.real()*x.real() + x.imag()*x.imag();
		  }
		};
	}
}