#pragma once

#include <vector>

template<class T>
class Vector
{
public:
	Vector(int n);
	Vector(Vector<T> const &rhs);

	int getCount() const;
	void set(int i, T const &value);
	T dot(Vector<T> const &rhs) const;
	T squaredNorm() const;
	void weightedSum(Vector<T> const &x, T const &yWeight, Vector<T> const &y);
	void addWeightedSum(T const &xWeight, Vector<T> const &x, T const &yWeight, Vector<T> const &y);
	void pointwiseMultiply(Vector<T> const &x, Vector<T> const &y);
	void subtract(Vector<T> const &x, Vector<T> const &y);
	void conjugate();

	T const& operator()(int i) const;
	Vector<T> const& operator=(Vector<T> const &rhs);

private:
	void setToZero();

private:
	const int _count;
	std::vector<T> _values;
};

