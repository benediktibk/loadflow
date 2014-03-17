#pragma once

class MultiPrecision
{
public:
	MultiPrecision();
	MultiPrecision(float value);
	MultiPrecision(double value);
	MultiPrecision(long double value);
	MultiPrecision(int value);
	~MultiPrecision();

	const MultiPrecision operator+(const MultiPrecision &rhs) const;
	const MultiPrecision operator-(const MultiPrecision &rhs) const;
	const MultiPrecision operator*(const MultiPrecision &rhs) const;
	const MultiPrecision operator/(const MultiPrecision &rhs) const;
	operator double() const;
	MultiPrecision& operator=(const MultiPrecision &rhs);
	MultiPrecision& operator+=(const MultiPrecision &rhs);
	MultiPrecision& operator-=(const MultiPrecision &rhs);
	MultiPrecision& operator*=(const MultiPrecision &rhs);
	MultiPrecision& operator/=(const MultiPrecision &rhs);

private:
	double _value;
};

