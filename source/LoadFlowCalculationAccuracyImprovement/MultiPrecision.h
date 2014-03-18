#pragma once

#include <ostream>
#include <mpirxx.h>

class MultiPrecision
{
public:
	typedef mpf_class ValueType;

	MultiPrecision();
	MultiPrecision(MultiPrecision const& rhs);
	explicit MultiPrecision(ValueType const& valueType);
	explicit MultiPrecision(float value);
	explicit MultiPrecision(double value);
	explicit MultiPrecision(int value);
	
	operator double() const;
	operator int() const;
	MultiPrecision& operator=(const MultiPrecision &rhs);
	MultiPrecision& operator+=(const MultiPrecision &rhs);
	MultiPrecision& operator-=(const MultiPrecision &rhs);
	MultiPrecision& operator*=(const MultiPrecision &rhs);
	MultiPrecision& operator/=(const MultiPrecision &rhs);
	ValueType const& getValue() const;
	const MultiPrecision operator+() const;
	const MultiPrecision operator-() const;

public:
	static void setDefaultPrecision();
	static unsigned int getBitPrecision();

private:
	static const unsigned int _precision;

private:
	ValueType _value;
};

const MultiPrecision operator+(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator-(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator*(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator/(const MultiPrecision &lhs, const MultiPrecision &rhs);
bool operator<(const MultiPrecision &lhs, const MultiPrecision &rhs);
bool operator>(const MultiPrecision &lhs, const MultiPrecision &rhs);
bool operator>=(const MultiPrecision &lhs, const MultiPrecision &rhs);
bool operator==(const MultiPrecision &lhs, const MultiPrecision &rhs);
bool operator!=(const MultiPrecision &lhs, const MultiPrecision &rhs);
std::ostream& operator<<(std::ostream &stream, MultiPrecision const& value);

namespace std
{
	MultiPrecision abs(MultiPrecision const& value);
	MultiPrecision sqrt(MultiPrecision const& value);
	// not very accurate as MPIR does not provide a function for the logarithm
	MultiPrecision log(MultiPrecision const& value);
	MultiPrecision ceil(MultiPrecision const& value);
}