#pragma once

#include <vector>
#include <string>
#include <iostream>
#include <sstream>

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

template<class Floating, class ComplexFloating>
bool operator==(Vector<Floating, ComplexFloating> const &one, Vector<Floating, ComplexFloating> const &two)
{
	if (one.getCount() != two.getCount())
		return false;

	for (auto i = 0; i < one.getCount(); ++i)
		if (one(i) != two(i))
			return false;

	return true;
}

template<class Floating, class ComplexFloating>
std::ostream& operator<<(std::ostream &stream, Vector<Floating, ComplexFloating> const &vector)
{
	for (auto i = 0; i < vector.getCount(); ++i)
		stream << vector(i) << std::endl;

	return stream;
}

template<class Floating, class ComplexFloating>
std::istream& operator>>(std::istream &stream, Vector<Floating, ComplexFloating> &vector)
{
	string nextLine;
	std::getline(stream, nextLine);
	auto i = 0;

	while(nextLine.size() > 0)
	{
		stringstream lineStream(nextLine);
		ComplexFloating value;
		lineStream >> value;
		vector.set(i, value);
		std::getline(stream, nextLine);
		++i;
	}

	return stream;
}
