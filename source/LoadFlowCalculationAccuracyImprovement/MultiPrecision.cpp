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

MultiPrecision::ValueType MultiPrecision::getValue() const
{
	return _value;
}

const MultiPrecision operator+(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return MultiPrecision(lhs.getValue() + rhs.getValue());
}

const MultiPrecision operator-(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return MultiPrecision(lhs.getValue() - rhs.getValue());
}

const MultiPrecision operator*(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return MultiPrecision(lhs.getValue() * rhs.getValue());
}

const MultiPrecision operator/(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return MultiPrecision(lhs.getValue() / rhs.getValue());
}