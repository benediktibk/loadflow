#include "PQBus.h"
#include <assert.h>

using namespace std;

PQBus::PQBus() :
	_id(-1),
	_power(0, 0)
{ }

PQBus::PQBus(int id, complex<double> power) :
	_id(id),
	_power(power)
{
	assert(_id >= 0);
}

int PQBus::getId() const
{
	return _id;
}

const complex<double>& PQBus::getPower() const
{
	return _power;
}
