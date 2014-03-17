#pragma once

template<typename T>
class Complex
{
public:
	Complex();
	Complex(T const& real, T const& imag);
	Complex(T const& real);

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
const Complex<T> operator+(Complex<T> const& lhs, Complex<T> const& rhs);
template<typename T>
const Complex<T> operator-(Complex<T> const& lhs, Complex<T> const& rhs);
template<typename T>
const Complex<T> operator*(Complex<T> const& lhs, Complex<T> const& rhs);
template<typename T>
const Complex<T> operator/(Complex<T> const& lhs, Complex<T> const& rhs);
template<typename T>
const Complex<T> operator==(Complex<T> const& lhs, Complex<T> const& rhs);
template<typename T>
const Complex<T> operator!=(Complex<T> const& lhs, Complex<T> const& rhs);

