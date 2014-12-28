#pragma once

template<class Floating, class ComplexFloating>
class ComplexGreaterCompare
{
public:
	bool operator()(ComplexFloating const &a, ComplexFloating const &b) const;
};

