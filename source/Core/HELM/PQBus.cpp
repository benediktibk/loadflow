#include "PQBus.h"
#include <assert.h>

using namespace std;

PQBus::PQBus() :
	_id(-1),
	_power(0, 0)
{ }

PQBus::PQBus(int id, Complex<long double> power) :
	_id(id),
	_power(power)
{
	assert(_id >= 0);
}

int PQBus::getId() const
{
	return _id;
}

const Complex<long double>& PQBus::getPower() const
{
	return _power;
}
