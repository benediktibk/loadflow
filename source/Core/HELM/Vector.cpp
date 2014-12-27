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
Vector<T>::Vector(size_t n) :
	_count(n)
{
	allocateMemory();
	setToZero();
}

template<class T>
Vector<T>::Vector(Vector<T> const &rhs) :
	_count(rhs.getCount())
{
	allocateMemory();
	copyValues(rhs);
}

template<class T>
Vector<T>::~Vector()
{
	freeMemory();
}

template<class T>
size_t Vector<T>::getCount() const
{
	return _count;
}

template<class T>
void Vector<T>::set(size_t i, T const &value)
{
	assert(i < getCount());
	_values[i] = value;
}

template<class T>
T Vector<T>::dot(Vector<T> const &rhs) const
{
	assert(getCount() == rhs.getCount());
	T result(0);

	for (size_t i = 0; i < _count; ++i)
		result += _values[i]*rhs._values[i];

	return result;
}

template<class T>
T Vector<T>::squaredNorm() const
{
	T result(0);

	for (size_t i = 0; i < _count; ++i)
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

	for (size_t i = 0; i < _count; ++i)
		_values[i] = x._values[i] + yWeight*y._values[i];
}

template<class T>
void Vector<T>::addWeightedSum(T const &xWeight, Vector<T> const &x, T const &yWeight, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	for (size_t i = 0; i < _count; ++i)
		_values[i] += xWeight*x._values[i] + yWeight*y._values[i];
}

template<class T>
void Vector<T>::pointwiseMultiply(Vector<T> const &x, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());

	for (size_t i = 0; i < _count; ++i)
		_values[i] = x._values[i]*y._values[i];
}

template<class T>
void Vector<T>::subtract(Vector<T> const &x, Vector<T> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());

	for (size_t i = 0; i < _count; ++i)
		_values[i] = x._values[i] - y._values[i];
}

template<class T>
void Vector<T>::conjugate()
{
	for (size_t i = 0; i < _count; ++i)
		_values[i] = std::conj(_values[i]);
}

template<class T>
T const& Vector<T>::operator()(size_t i) const
{
	assert(i < _count);
	return _values[i];
}

template<class T>
Vector<T> const& Vector<T>::operator=(Vector<T> const& rhs)
{
	assert(getCount() == rhs.getCount());
	copyValues(rhs);
	return *this;
}

template<class T>
void Vector<T>::allocateMemory()
{
	assert(getCount() > 0);
	_values = new T[_count];
}

template<class T>
void Vector<T>::freeMemory()
{
	delete[] _values;
	_values = 0;
}

template<class T>
void Vector<T>::copyValues(Vector<T> const &rhs)
{
	memcpy(_values, rhs._values, _count*sizeof(T));
}

template<class T>
void Vector<T>::setToZero()
{
	memset(_values, 0, sizeof(T)*_count);
}

