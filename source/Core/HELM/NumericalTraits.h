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


template<typename T> bool isFinite(T const &arg)
{
    return arg == arg && 
           arg != std::numeric_limits<T>::infinity() &&
           arg != -std::numeric_limits<T>::infinity();
}

