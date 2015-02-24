#pragma once

#include <vector>
#include <set>
#include <list>

class Node
{
public:
	Node(int index);
	~Node();

	void connect(const Node *node);
	int getDegree() const;
	int getIndex() const;
	std::list<const Node*> getNeighboursSortedByDegree() const;

private:
	const int _index;
	std::vector<const Node*> _neighbours;
	std::set<const Node*> _neighboursSet;
};

