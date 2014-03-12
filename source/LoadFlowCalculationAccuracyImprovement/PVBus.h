#pragma once

class PVBus
{
public:
	PVBus();
	PVBus(int id, double powerReal, double voltageMagnitude);

	int getId() const;
	double getPowerReal() const;
	double getVoltageMagnitude() const;

private:
	int _id;
	double _powerReal;
	double _voltageMagnitude;
};

