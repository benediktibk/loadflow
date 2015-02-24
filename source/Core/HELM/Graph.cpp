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
	std::vector<const Node*> nodesSorted;
	std::list<const Node*> queue;
	std::set<const Node*> queueSet;
	std::vector<const Node*> result;
	std::set<const Node*> resultSet;
	result.reserve(_nodes.size());
	nodesSorted.reserve(_nodes.size());

	for (auto i = _nodes.cbegin(); i != _nodes.cend(); ++i)
		nodesSorted.push_back(*i);

	std::sort(nodesSorted.begin(), nodesSorted.end(), [](const Node *a, const Node *b) -> bool { return a->getDegree() < b->getDegree(); } );
	std::list<const Node*> nodesSortedList(nodesSorted.cbegin(), nodesSorted.cend());

	while(result.size() < _nodes.size())
	{
		const Node *nextNode = 0;

		if (queue.empty())
		{
			while(nextNode == 0)
			{
				const Node *node = nodesSortedList.front();
				nodesSortedList.pop_front();

				if (resultSet.count(node) == 0)
					nextNode = node;
			}
		}
		else
		{
			nextNode = queue.front();
			queue.pop_front();
			queueSet.erase(queueSet.find(nextNode));
		}

		result.push_back(nextNode);
		resultSet.insert(nextNode);

		auto neighbours = nextNode->getNeighboursSortedByDegree();
		std::list<const Node*> neighboursReduced;

		for (auto neighbour : neighbours)
			if (resultSet.count(neighbour) == 0 && queueSet.count(neighbour) == 0)
				neighboursReduced.push_back(neighbour);

		queueSet.insert(neighboursReduced.cbegin(), neighboursReduced.cend());
		queue.merge(neighboursReduced, [](const Node *a, const Node *b) -> bool { return a->getDegree() < b->getDegree(); });
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
