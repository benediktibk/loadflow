#pragma once

#include <complex>

class PQBus
{
public:
	PQBus();
	PQBus(int id, std::complex<double> power);

	int getId() const;
	const std::complex<double>& getPower() const;

private:
	int _id;
	std::complex<double> _power;
};

