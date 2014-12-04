#include "MultiPrecision.h"

unsigned int MultiPrecision::_precision = 300;

MultiPrecision::MultiPrecision() :
	_value(0, _precision)
{ }

MultiPrecision::MultiPrecision(MultiPrecision const& rhs) :
	_value(rhs.getValue())
{ }

MultiPrecision::MultiPrecision(ValueType const& value) :
	_value(value)
{ }

MultiPrecision::MultiPrecision(float value) :
	_value(value, _precision)
{ }

MultiPrecision::MultiPrecision(double value) :
	_value(value, _precision)
{ }

MultiPrecision::MultiPrecision(double value, int bitPrecision) :
	_value(value, bitPrecision)
{ }

MultiPrecision::MultiPrecision(int value) :
	_value(value, _precision)
{ }

MultiPrecision::operator double() const
{
	return static_cast<double>(_value.get_d());
}

MultiPrecision::operator int() const
{
	return static_cast<int>(_value.get_d());
}

MultiPrecision& MultiPrecision::operator=(const MultiPrecision &rhs)
{
	_value = rhs.getValue();
	return *this;
}

MultiPrecision& MultiPrecision::operator+=(const MultiPrecision &rhs)
{
	_value += rhs.getValue();
	return *this;
}

MultiPrecision& MultiPrecision::operator-=(const MultiPrecision &rhs)
{
	_value -= rhs.getValue();
	return *this;
}

MultiPrecision& MultiPrecision::operator*=(const MultiPrecision &rhs)
{
	_value *= rhs.getValue();
	return *this;
}

MultiPrecision& MultiPrecision::operator/=(const MultiPrecision &rhs)
{
	_value /= rhs.getValue();
	return *this;
}

MultiPrecision::ValueType const& MultiPrecision::getValue() const
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

bool operator<=(const MultiPrecision &lhs, const MultiPrecision &rhs)
{
	return lhs.getValue() <= rhs.getValue();
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
	return MultiPrecision(log(static_cast<double>(value)));
}

MultiPrecision std::ceil(MultiPrecision const& value)
{
	return MultiPrecision(ceil(value.getValue()));
}

void MultiPrecision::setDefaultPrecision()
{
	mpf_set_default_prec(_precision);
}

void MultiPrecision::setDefaultPrecision(unsigned int bitPrecision)
{
	_precision = bitPrecision;
	setDefaultPrecision();
}


unsigned int MultiPrecision::getBitPrecision()
{
	return _precision;
}