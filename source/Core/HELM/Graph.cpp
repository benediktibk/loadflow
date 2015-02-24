#include "Graph.h"
#include "Node.h"
#include <list>
#include <algorithm>
#include <assert.h>

Graph::Graph()
{ }

Graph::Graph(int capacity)
{
	_nodes.reserve(capacity);
}

Graph::~Graph()
{
	for (auto i = _nodes.begin(); i != _nodes.end(); ++i)
		delete *i;
	_nodes.clear();
	_nodesByIndex.clear();
}

void Graph::addNode(int index)
{
	auto node = new Node(index);
	_nodes.push_back(node);
	_nodesByIndex.insert(std::pair<int, Node*>(index, node));
}

void Graph::connect(int one, int two)
{
	auto nodeOne = _nodesByIndex[one];
	auto nodeTwo = _nodesByIndex[two];
	nodeOne->connect(nodeTwo);
	nodeTwo->connect(nodeOne);
}

std::vector<int> Graph::calculateReverseCuthillMcKee() const
{
	std::vector<const Node*> result;
	std::set<const Node*> resultSet;
	result.reserve(_nodes.size());
	std::list<const Node*> leftOver(_nodes.cbegin(), _nodes.cend());
	auto position = 0;

	while(result.size() < _nodes.size())
	{
		while(position == result.size())
		{
			auto candidate = leftOver.front();
			leftOver.pop_front();

			if (resultSet.count(candidate) != 0)
				continue;

			result.push_back(candidate);
			resultSet.insert(candidate);
		}

		const Node *node = result[position];
		auto neighbours = node->getNeighboursSortedByDegree();
		auto neighboursReduced = filterOut(neighbours, resultSet);
		result.insert(result.end(), neighboursReduced.cbegin(), neighboursReduced.cend());
		resultSet.insert(neighboursReduced.cbegin(), neighboursReduced.cend());
		++position;
	}

	std::reverse(result.begin(), result.end());
	std::vector<int> indices;
	indices.reserve(result.size());

	for (auto i : result)
		indices.push_back(i->getIndex());

	return indices;
}

std::list<const Node*> Graph::filterOut(std::list<const Node*> const &nodes, std::set<const Node*> const &filter)
{		
	std::list<const Node*> result;

	for (auto node : nodes)
		if (filter.count(node) == 0)
			result.push_back(node);

	return result;
}