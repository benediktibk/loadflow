#include "PVBus.h"
#include <assert.h>

PVBus::PVBus() :
	_id(0),
	_powerReal(0),
	_voltageMagnitude(0)
{ }

PVBus::PVBus(int id, double powerReal, double voltageMagnitude) :
	_id(id),
	_powerReal(powerReal),
	_voltageMagnitude(voltageMagnitude)
{ 
	assert(_voltageMagnitude >= 0);
	assert(_id >= 0);
}

int PVBus::getId() const
{
	return _id;
}

double PVBus::getPowerReal() const
{
	return _powerReal;
}

double PVBus::getVoltageMagnitude() const
{
	return _voltageMagnitude;
}