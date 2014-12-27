#include "Vector.h"
#include <assert.h>
#include <string.h>

template class Vector<long double>;

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
	assert(i < _count);
	_values[i] = value;
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
	assert(_count == rhs.getCount());
	copyValues(rhs);
	return *this;
}

template<class T>
void Vector<T>::allocateMemory()
{
	assert(_count > 0);
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
	memset(_values, T(0), sizeof(T)*_count);
}

