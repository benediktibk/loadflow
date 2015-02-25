#include "Node.h"
#include <assert.h>

Node::Node(int index) :
	_index(index)
{
	assert(index >= 0);
}

Node::~Node()
{
	_neighbours.clear();
	_neighboursSet.clear();
}

void Node::connect(const Node *node)
{
	if (_neighboursSet.count(node) > 0)
		return;

	_neighbours.push_back(node);
	_neighboursSet.insert(node);
}

int Node::getDegree() const
{
	return _neighbours.size();
}

int Node::calculateSecondLevelDegree() const
{
	std::set<const Node*> nodes;

	for (auto i : _neighbours)
	{
		auto &neighbours = i->getNeighbours();
		nodes.insert(neighbours.cbegin(), neighbours.cend());
	}

	return nodes.size() - 1;
}

int Node::getIndex() const
{
	return _index;
}

std::vector<const Node*> const& Node::getNeighbours() const
{
	return _neighbours;
}

std::list<const Node*> Node::getNeighboursSortedByDegree() const
{
	std::list<const Node*> result(_neighbours.cbegin(), _neighbours.cend());
	result.sort([](const Node *a, const Node *b) -> bool { return a->getDegree() < b->getDegree(); });
	return result;
}