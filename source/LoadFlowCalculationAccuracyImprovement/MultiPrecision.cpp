#include "MultiPrecision.h"
#include <mpirxx.h>

MultiPrecision::MultiPrecision() :
	_value(0)
{ }

MultiPrecision::MultiPrecision(float value) :
	_value(value)
{ }

MultiPrecision::MultiPrecision(double value) :
	_value(value)
{ }

MultiPrecision::MultiPrecision(long double value) :
	_value(value)
{ }

MultiPrecision::MultiPrecision(int value) :
	_value(value)
{ }

MultiPrecision::~MultiPrecision(void)
{ }

const MultiPrecision MultiPrecision::operator+(const MultiPrecision &rhs) const
{
	return MultiPrecision(_value+rhs._value);
}

const MultiPrecision MultiPrecision::operator-(const MultiPrecision &rhs) const
{
	return MultiPrecision(_value-rhs._value);
}

const MultiPrecision MultiPrecision::operator*(const MultiPrecision &rhs) const
{
	return MultiPrecision(_value*rhs._value);
}

const MultiPrecision MultiPrecision::operator/(const MultiPrecision &rhs) const
{
	return MultiPrecision(_value/rhs._value);
}

MultiPrecision::operator double() const
{
	return _value;
}

MultiPrecision& MultiPrecision::operator=(const MultiPrecision &rhs)
{
	_value = rhs._value;
	return *this;
}

MultiPrecision& MultiPrecision::operator+=(const MultiPrecision &rhs)
{
	_value += rhs._value;
	return *this;
}

MultiPrecision& MultiPrecision::operator-=(const MultiPrecision &rhs)
{
	_value -= rhs._value;
	return *this;
}

MultiPrecision& MultiPrecision::operator*=(const MultiPrecision &rhs)
{
	_value *= rhs._value;
	return *this;
}

MultiPrecision& MultiPrecision::operator/=(const MultiPrecision &rhs)
{
	_value /= rhs._value;
	return *this;
}
