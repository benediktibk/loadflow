#pragma once

class MultiPrecision
{
public:
	typedef double ValueType;

	MultiPrecision();
	MultiPrecision(float value);
	MultiPrecision(double value);
	MultiPrecision(long double value);
	MultiPrecision(int value);
	~MultiPrecision();

	operator double() const;
	MultiPrecision& operator=(const MultiPrecision &rhs);
	MultiPrecision& operator+=(const MultiPrecision &rhs);
	MultiPrecision& operator-=(const MultiPrecision &rhs);
	MultiPrecision& operator*=(const MultiPrecision &rhs);
	MultiPrecision& operator/=(const MultiPrecision &rhs);
	ValueType getValue() const;

private:
	ValueType _value;
};

const MultiPrecision operator+(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator-(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator*(const MultiPrecision &lhs, const MultiPrecision &rhs);
const MultiPrecision operator/(const MultiPrecision &lhs, const MultiPrecision &rhs);

