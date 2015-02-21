#pragma once

#include <limits>
#include "MultiPrecision.h"
#include "Complex.h"

using namespace std;

template<class T>
class NumericalTraits
{ };

template<>
class NumericalTraits<long double>
{
public:
	static long double epsilon()
	{
		return numeric_limits<long double>::epsilon();
	}
};

template<>
class NumericalTraits<MultiPrecision>
{
public:
	static MultiPrecision epsilon()
	{
		return MultiPrecision(pow(2, static_cast<double>(MultiPrecision::getBitPrecision())*(-1)));
	}
};


template<typename T> bool isValueFinite(T const &arg)
{
	if (arg == T(0))
		return true;

    auto equal = arg == arg;
    auto notPositiveInfinity = arg != std::numeric_limits<T>::infinity();
    auto notNegativeInfinity = arg != -std::numeric_limits<T>::infinity();
	return equal && notPositiveInfinity && notNegativeInfinity;
}

