#pragma once

#include <vector>
#include <map>
#include <complex>
#include "PQBus.h"
#include "PVBus.h"
#include <Eigen/Sparse>

template<typename ComplexType, typename RealType>
class CoefficientStorage
{
public:
	CoefficientStorage(int maximumNumberOfCoefficients, int nodeCount, std::vector<PQBus> const& pqBuses, std::vector<PVBus> const &pvBuses, Eigen::SparseMatrix<ComplexType, Eigen::ColMajor > const& admittances);

	void addCoefficients(std::vector<ComplexType> const &coefficients);
	ComplexType const& getCoefficient(int node, int step) const;
	ComplexType const& getLastCoefficient(int node) const;
	ComplexType const& getInverseCoefficient(int node, int step) const;
	ComplexType const& getLastInverseCoefficient(int node) const;
	ComplexType const& getSquaredCoefficient(int node, int step) const;
	ComplexType const& getLastSquaredCoefficient(int node) const;
	ComplexType const& getCombinedCoefficient(int node, int step) const;
	ComplexType const& getLastCombinedCoefficient(int node) const;
	std::vector< std::complex<double> > calculateVoltagesFromCoefficients();
	size_t getCoefficientCount() const;

private:
	void calculateNextInverseCoefficients();
	void calculateNextInverseCoefficient(int node);
	void calculateFirstInverseCoefficients();
	void calculateNextSquaredCoefficients();
	void calculateNextSquaredCoefficient(int node);
	void calculateNextCombinedCoefficients();
	void calculateNextCombinedCoefficient(int node);
	void insertInverseCoefficient(int node, ComplexType const& value);
	void insertSquaredCoefficient(int node, ComplexType const& value);
	void insertCombinedCoefficient(int node, ComplexType const& value);
	std::complex<double> calculateVoltageFromCoefficients(int node);

private:
	const int _nodeCount;
	Eigen::SparseMatrix<ComplexType, Eigen::ColMajor > const& _admittances;
	std::vector< std::vector<ComplexType> > _coefficients;
	std::map<int, std::vector<ComplexType> > _inverseCoefficients;
	std::vector<int> _pqBuses;
	std::map<int, std::vector<ComplexType> > _squaredCoefficients;
	std::map<int, std::vector<ComplexType> > _combinedCoefficients;
	std::vector<int> _pvBuses;
};

