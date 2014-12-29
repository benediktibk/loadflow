#pragma once

#include "Complex.h"

class PQBus
{
public:
	PQBus();
	PQBus(int id, Complex<long double> power);

	int getId() const;
	const Complex<long double>& getPower() const;

private:
	int _id;
	Complex<long double> _power;
};

