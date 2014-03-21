#pragma once

#include <vector>
#include <map>
#include "PQBus.h"
#include "PVBus.h"

template<class T>
class CoefficientStorage
{
public:
	CoefficientStorage(int maximumNumberOfCoefficients, std::vector<PQBus> const& pqBuses, std::vector<PVBus> const &pvBuses);

	void addCoefficients(std::vector<T> const &coefficients);
	T const& getCoefficient(int node, int step) const;
	T const& getLastCoefficient(int node) const;
	T const& getInverseCoefficient(int node, int step) const;
	T const& getLastInverseCoefficient(int node) const;

private:
	void calculateNextInverseCoefficients();
	void calculateNextInverseCoefficient(int node);
	void calculateFirstInverseCoefficients();
	void insertInverseCoefficient(int node, T const& value);

private:
	std::vector< std::vector<T> > _coefficients;
	std::map<int, std::vector<T> > _inverseCoefficients;
	std::vector<int> _pqBuses;
};

