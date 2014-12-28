#include "Vector.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include <complex>
#include <assert.h>
#include <string.h>

template class Vector<long double>;
template class Vector< std::complex<long double> >;
template class Vector< Complex<MultiPrecision> >;

template<class T>
Vector<T>::Vector(int n) :
	_count(n),
	_values(_count)
{
	setToZero();
}

template<class T>
Vector<T>::Vector(Vector<T> const &rhs) :
	_count(rhs.getCount()),
	_values(rhs._values)
{ }

template<class T>
int Vector<T>::getCount() const
{
	return _count;
}

template<class T>
void Vector<T>::set(int i, T const &value)
{
	assert(i < getCount());
	assert(i >= 0);
	_values[i] = value;
}

template<class T>
T Vector<T>::dot(Vector<T> const &rhs) const
{
	assert(getCount() == rhs.getCount());
	T result(0);
	
	for (auto i = 0; i < _count; ++i)
		result += _values[i]*rhs._values[i];

	return result;
}

template<class T>
T Vector<T>::squaredNorm() const
{
	T result(0);
	
	for (auto i = 0; i < _count; ++i)
	{
		T const& value(_values[i]);
		result += value*value;
	}

	return result;
}

template<class T>
void Vector<T>::weightedSum(Vector<T> const &x, T const &yWeight, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());

	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i] + yWeight*y._values[i];
}

template<class T>
void Vector<T>::addWeightedSum(T const &xWeight, Vector<T> const &x, T const &yWeight, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] += xWeight*x._values[i] + yWeight*y._values[i];
}

template<class T>
void Vector<T>::pointwiseMultiply(Vector<T> const &x, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i]*y._values[i];
}

template<class T>
void Vector<T>::subtract(Vector<T> const &x, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i] - y._values[i];
}

template<class T>
void Vector<T>::conjugate()
{
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = std::conj(_values[i]);
}

template<class T>
T const& Vector<T>::operator()(int i) const
{
	assert(i < _count);
	assert(i >= 0);
	return _values[i];
}

template<class T>
Vector<T> const& Vector<T>::operator=(Vector<T> const& rhs)
{
	assert(getCount() == rhs.getCount());
	_values = rhs._values;
	return *this;
}

template<class T>
void Vector<T>::setToZero()
{
	T zero(0);
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = zero;
}

