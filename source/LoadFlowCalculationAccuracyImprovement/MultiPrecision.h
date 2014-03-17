#pragma once

#include <ostream>

class MultiPrecision
{
public:
	typedef double ValueType;

	MultiPrecision();
	MultiPrecision(MultiPrecision const& rhs);
	explicit MultiPrecision(float value);
	explicit MultiPrecision(double value);
	explicit MultiPrecision(long double value);
	explicit MultiPrecision(int value);
	~MultiPrecision();
	
	operator double() const;
	operator int() const;
	MultiPrecision& operator=(const MultiPrecision &rhs);
	MultiPrecision& operator+=(const MultiPrecision &rhs);
	MultiPrecision& operator-=(const MultiPrecision &rhs);
	MultiPrecision& operator*=(const MultiPrecision &rhs);
	MultiPrecision& operator/=(const MultiPrecision &rhs);
	ValueType getValue() const;
	const MultiPrecision operator+() const;
	const MultiPrecision operator-() const;

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
	MultiPrecision log(MultiPrecision const& value);
	MultiPrecision ceil(MultiPrecision const& value);
}