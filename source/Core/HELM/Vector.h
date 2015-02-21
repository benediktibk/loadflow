#pragma once

#include <vector>

template<class Floating, class ComplexFloating>
class Vector
{
public:
	Vector(int n);
	Vector(Vector<Floating, ComplexFloating> const &rhs);

	int getCount() const;
	void set(int i, ComplexFloating const &value);
	ComplexFloating dot(Vector<Floating, ComplexFloating> const &rhs) const;
	ComplexFloating squaredNorm() const;
	void weightedSum(Vector<Floating, ComplexFloating> const &x, ComplexFloating const &yWeight, Vector<Floating, ComplexFloating> const &y);
	void addWeightedSum(ComplexFloating const &xWeight, Vector<Floating, ComplexFloating> const &x, ComplexFloating const &yWeight, Vector<Floating, ComplexFloating> const &y);
	void pointwiseMultiply(Vector<Floating, ComplexFloating> const &x, Vector<Floating, ComplexFloating> const &y);
	void subtract(Vector<Floating, ComplexFloating> const &x, Vector<Floating, ComplexFloating> const &y);
	void conjugate();
	bool isFinite() const;

	ComplexFloating const& operator()(int i) const;
	Vector<Floating, ComplexFloating> const& operator=(Vector<Floating, ComplexFloating> const &rhs);

private:
	void setToZero();

private:
	const int _count;
	std::vector<ComplexFloating> _values;
	mutable std::vector<Floating> _tempReal;
	mutable std::vector<Floating> _tempImaginary;
};

