#pragma once

#include <vector>
#include <map>
#include "PQBus.h"
#include "PVBus.h"
#include "SparseMatrix.h"
#include "Vector.h"

template<typename ComplexType, typename RealType>
class CoefficientStorage
{
public:
	CoefficientStorage(int maximumNumberOfCoefficients, int nodeCount, std::vector<PQBus> const& pqBuses, std::vector<PVBus> const &pvBuses, SparseMatrix<RealType, ComplexType> const& admittances);

	void addCoefficients(Vector<RealType, ComplexType> const &coefficients);
	ComplexType const& getCoefficient(int node, int step) const;
	ComplexType const& getLastCoefficient(int node) const;
	ComplexType const& getInverseCoefficient(int node, int step) const;
	ComplexType const& getLastInverseCoefficient(int node) const;
	ComplexType const& getSquaredCoefficient(int node, int step) const;
	ComplexType const& getLastSquaredCoefficient(int node) const;
	ComplexType const& getWeightedCoefficient(int node, int step) const;
	ComplexType const& getCombinedCoefficient(int node, int step) const;
	ComplexType const& getLastCombinedCoefficient(int node) const;
	int getCoefficientCount() const;

private:
	void calculateNextInverseCoefficients();
	void calculateNextInverseCoefficient(int node);
	void calculateFirstInverseCoefficients();
	void calculateNextSquaredCoefficients();
	void calculateNextSquaredCoefficient(int node);
	void calculateNextCombinedCoefficients();
	void calculateNextCombinedCoefficient(int node);
	void calculateNextWeightedCoefficients();
	void calculateNextWeightedCoefficient(int node);
	void insertInverseCoefficient(int node, ComplexType const& value);
	void insertSquaredCoefficient(int node, ComplexType const& value);
	void insertCombinedCoefficient(int node, ComplexType const& value);
	void insertWeightedCoefficent(int node, ComplexType const& value);

private:
	const int _nodeCount;
	const int _pqBusCount;
	const int _pvBusCount;
	SparseMatrix<RealType, ComplexType> const& _admittances;
	std::vector< Vector<RealType, ComplexType> > _coefficients;
	std::map<int, std::vector<ComplexType> > _inverseCoefficients;
	std::vector<int> _pqBuses;
	std::map<int, std::vector<ComplexType> > _squaredCoefficients;
	std::map<int, std::vector<ComplexType> > _combinedCoefficients;
	std::map<int, std::vector<ComplexType> > _weightedCoefficients;
	std::vector<int> _pvBuses;
	std::map<int, RealType> _pvBusVoltageMagnitudeSquares;
};

