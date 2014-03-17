#include "MultiPrecision.h"
#include <mpirxx.h>

MultiPrecision::MultiPrecision() :
	_value(0)
{ }

MultiPrecision::MultiPrecision(MultiPrecision const& rhs) :
	_value(rhs)
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
	return static_cast<double>(_value);

}

MultiPrecision::operator int() const
{
	return static_cast<int>(_value);
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

const MultiPrecision MultiPrecision::operator+() const
{
	return *this;
}

const MultiPrecision MultiPrecision::operator-() const
{
	return MultiPrecision(-_value);
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

bool operator<(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() < rhs.getValue();
}

bool operator>(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() > rhs.getValue();
}

bool operator>=(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() >= rhs.getValue();
}

bool operator==(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() == rhs.getValue();
}

bool operator!=(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() != rhs.getValue();
}

std::ostream& operator<<(std::ostream &stream, MultiPrecision const& value)
{
	stream << value.getValue();
	return stream;
}

MultiPrecision std::abs(MultiPrecision const& value)
{
	return MultiPrecision(abs(value.getValue()));
}

MultiPrecision std::sqrt(MultiPrecision const& value)
{
	return MultiPrecision(sqrt(value.getValue()));
}

MultiPrecision std::log(MultiPrecision const& value)
{
	return MultiPrecision(log(value.getValue()));
}

MultiPrecision std::ceil(MultiPrecision const& value)
{
	return MultiPrecision(ceil(value.getValue()));
}