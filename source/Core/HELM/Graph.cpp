#include "Graph.h"
#include "Node.h"
#include <list>
#include <algorithm>

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

std::map<int, int> Graph::calculateReverseCuthillMcKee() const
{
	std::set<const Node*> leftOver;
	std::vector<const Node*> result;
	std::set<const Node*> resultSet;
	result.reserve(_nodes.size());

	for (auto i = _nodes.cbegin() + 1; i != _nodes.cend(); ++i)
		leftOver.insert(*i);

	result.push_back(_nodes.front());
	resultSet.insert(_nodes.front());

	while(result.size() < _nodes.size())
	{
		const Node *node = result.back();

		auto neighbours = node->getNeighboursSortedByDegree();
		std::list<const Node*> neighboursReduced;

		for (auto neighbour : neighbours)
			if (resultSet.count(neighbour) == 0)
				neighboursReduced.push_back(neighbour);

		if (!neighboursReduced.empty())
		{
			result.insert(result.end(), neighboursReduced.cbegin(), neighboursReduced.cend());
			resultSet.insert(neighboursReduced.cbegin(), neighboursReduced.cend());
			
			for (auto neighbour : neighboursReduced)
				leftOver.erase(leftOver.find(neighbour));
		}
		else
		{
			node = *leftOver.begin();
			leftOver.erase(leftOver.begin());
			result.push_back(node);
			resultSet.insert(node);
		}
	}

	std::reverse(result.begin(), result.end());
	std::vector<int> indices;
	indices.reserve(_nodes.size());

	for (auto i : _nodes)
		indices.push_back(i->getIndex());

	std::sort(indices.begin(), indices.end());
	std::map<int, int> resultMap;

	for (auto i = 0; i < _nodes.size(); ++i)
		resultMap.insert(std::pair<int, int>(result[i]->getIndex(), indices[i]));

	return resultMap;
}
