#include "Vector.h"
#include "Complex.h"
#include "MultiPrecision.h"
#include "NumericalTraits.h"
#include <assert.h>
#include <string.h>
#include <algorithm>
#include <functional>

template class Vector<long double, Complex<long double> >;
template class Vector<MultiPrecision, Complex<MultiPrecision> >;

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating>::Vector(int n) :
	_count(n),
	_values(_count)
{
	setToZero();
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating>::Vector(Vector<Floating, ComplexFloating> const &rhs) :
	_count(rhs.getCount()),
	_values(rhs._values)
{ }

template<class Floating, class ComplexFloating>
int Vector<Floating, ComplexFloating>::getCount() const
{
	return _count;
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::set(int i, ComplexFloating const &value)
{
	assert(i < getCount());
	assert(i >= 0);
	_values[i] = value;
}

template<class Floating, class ComplexFloating>
ComplexFloating Vector<Floating, ComplexFloating>::dot(Vector<Floating, ComplexFloating> const &rhs) const
{
	assert(getCount() == rhs.getCount());
	_tempReal.resize(getCount());
	_tempImaginary.resize(getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
	{
		auto value = _values[i]*rhs._values[i];
		_tempReal[i] = std::real(value);
		_tempImaginary[i] = std::imag(value);
	}

	std::sort(_tempReal.begin(), _tempReal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
	std::sort(_tempImaginary.begin(), _tempImaginary.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
	ComplexFloating result;
	
	for (auto i = 0; i < _count; ++i)
		result += ComplexFloating(_tempReal[i], _tempImaginary[i]);

	return result;
}

template<class Floating, class ComplexFloating>
ComplexFloating Vector<Floating, ComplexFloating>::squaredNorm() const
{
	_tempReal.resize(getCount());
	_tempImaginary.resize(getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
	{
		ComplexFloating const& value(_values[i]);
		auto valueSquared = value*value;
		_tempReal[i] = std::real(valueSquared);
		_tempImaginary[i] = std::imag(valueSquared);
	}
	
	std::sort(_tempReal.begin(), _tempReal.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
	std::sort(_tempImaginary.begin(), _tempImaginary.end(), [](Floating const &a, Floating const &b){ return std::abs(a) < std::abs(b); });
	ComplexFloating result;
	
	for (auto i = 0; i < _count; ++i)
		result += ComplexFloating(_tempReal[i], _tempImaginary[i]);

	return result;
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::weightedSum(Vector<Floating, ComplexFloating> const &x, ComplexFloating const &yWeight, Vector<Floating, ComplexFloating> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());

	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i] + yWeight*y._values[i];
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::addWeightedSum(ComplexFloating const &xWeight, Vector<Floating, ComplexFloating> const &x, ComplexFloating const &yWeight, Vector<Floating, ComplexFloating> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] += xWeight*x._values[i] + yWeight*y._values[i];
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::pointwiseMultiply(Vector<Floating, ComplexFloating> const &x, Vector<Floating, ComplexFloating> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i]*y._values[i];
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::subtract(Vector<Floating, ComplexFloating> const &x, Vector<Floating, ComplexFloating> const &y)
{
	assert(getCount() == x.getCount());
	assert(getCount() == y.getCount());
	
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = x._values[i] - y._values[i];
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::conjugate()
{
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = std::conj(_values[i]);
}

template<typename Floating, typename ComplexFloating> 
bool Vector<Floating, ComplexFloating>::isFinite() const
{	
	for (auto i = 0; i < _count; ++i)
		if (!isValueFinite(std::abs2(_values[i])))
			return false;

	return true;
}

template<class Floating, class ComplexFloating>
ComplexFloating const& Vector<Floating, ComplexFloating>::operator()(int i) const
{
	assert(i < _count);
	assert(i >= 0);
	return _values[i];
}

template<class Floating, class ComplexFloating>
Vector<Floating, ComplexFloating> const& Vector<Floating, ComplexFloating>::operator=(Vector<Floating, ComplexFloating> const& rhs)
{
	assert(getCount() == rhs.getCount());
	_values = rhs._values;
	return *this;
}

template<class Floating, class ComplexFloating>
void Vector<Floating, ComplexFloating>::setToZero()
{
	ComplexFloating zero(Floating(0));
	#pragma omp parallel for
	for (auto i = 0; i < _count; ++i)
		_values[i] = zero;
}

