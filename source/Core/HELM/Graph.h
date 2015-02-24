#pragma once

#include <vector>
#include <map>

class Node;

class Graph
{
public:
	Graph();
	Graph(int capacity);
	~Graph();

	void addNode(int index);
	void connect(int one, int two);
	std::map<int, int> calculateReverseCuthillMcKee() const;

private:
	std::vector<Node*> _nodes;
	std::map<int, Node*> _nodesByIndex;
};

