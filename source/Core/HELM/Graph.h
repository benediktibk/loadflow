#pragma once

#include <vector>
#include <map>
#include <set>
#include <list>

class Node;

class Graph
{
public:
	Graph();
	Graph(int capacity);
	~Graph();

	void addNode(int index);
	void connect(int one, int two);
	std::vector<int> calculateReverseCuthillMcKee() const;
	std::vector<int> calculateReverseCuthillMcKee(int startNode) const;
	std::vector<std::vector<int>> createLayeringFrom(int startNode) const;
	int findPseudoPeriphereNode() const;
	int findPseudoPeriphereNode(size_t &eccentricity) const;
	int calculateSecondLevelDegree(int index) const;

private:
	static std::vector<int> calculateReverseCuthillMcKee(std::list<const Node*> nodes);
	static std::vector<std::set<const Node*>> createLayeringFrom(const Node *startNode);
	static std::list<const Node*> filterOut(std::list<const Node*> const &nodes, std::set<const Node*> const &filter);
	static bool containedInLayering(std::vector<std::set<const Node*>> const &layering, const Node *node);

private:
	Graph(Graph const & rhs) { }
	void operator=(Graph const &rhs) { }

private:
	std::vector<Node*> _nodes;
	std::map<int, Node*> _nodesByIndex;
};

