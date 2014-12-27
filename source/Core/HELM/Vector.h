#pragma once

template<class T>
class Vector
{
public:
	Vector(size_t n);
	Vector(Vector<T> const &rhs);
	~Vector();

	size_t getCount() const;
	void set(size_t i, T const &value);
	T dot(Vector<T> const &rhs) const;
	T squaredNorm() const;
	void weightedSum(Vector<T> const &x, T const &yWeight, Vector<T> const &y);
	void addWeightedSum(T const &xWeight, Vector<T> const &x, T const &yWeight, Vector<T> const &y);

	T const& operator()(size_t i) const;
	Vector<T> const& operator=(Vector<T> const &rhs);

private:
	void allocateMemory();
	void freeMemory();
	void copyValues(Vector<T> const &rhs);
	void setToZero();

private:
	const size_t _count;
	T *_values;
};

