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
	std::list<const Node*> nodes(_nodes.cbegin(), _nodes.cend());
	return calculateReverseCuthillMcKee(nodes);
}

std::vector<int> Graph::calculateReverseCuthillMcKee(int startNodeIndex) const
{
	auto startNode = _nodesByIndex.at(startNodeIndex);
	std::list<const Node*> nodes;
	nodes.push_back(startNode);

	for (auto node : _nodes)
		if (node != startNode)
			nodes.push_back(node);

	return calculateReverseCuthillMcKee(nodes);
}

std::vector<std::vector<int>> Graph::createLayeringFrom(int startNode) const
{
	auto layering = createLayeringFrom(_nodesByIndex.at(startNode));	
	std::vector<std::vector<int>> layeringIndices;
	layeringIndices.reserve(layering.size());
	
	for (auto &layer : layering)
	{
		std::vector<int> indices;
		indices.reserve(layer.size());

		for (auto node : layer)
			indices.push_back(node->getIndex());

		layeringIndices.push_back(std::vector<int>(indices.cbegin(), indices.cend()));
	}

	return layeringIndices;
}

int Graph::findPseudoPeriphereNode() const
{
	std::set<const Node*> candidates;
	candidates.insert(_nodes.front());
	auto eccentricity = 0;
	const Node *result = 0;
	bool improvedResult = true;

	while(improvedResult)
	{
		auto candidate = *(candidates.begin());
		auto layering = createLayeringFrom(candidate);

		if (layering.size() > eccentricity)
		{
			result = candidate;
			eccentricity = layering.size();
			candidates = layering.back();
		}
		else
			improvedResult = false;
	}

	return result->getIndex();
}

std::vector<int> Graph::calculateReverseCuthillMcKee(std::list<const Node*> nodes)
{
	std::vector<const Node*> result;
	std::set<const Node*> resultSet;
	result.reserve(nodes.size());
	auto position = 0;
	auto nodeCount = nodes.size();

	while(result.size() < nodeCount)
	{
		while(position == result.size())
		{
			auto candidate = nodes.front();
			nodes.pop_front();

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

std::vector<std::set<const Node*>> Graph::createLayeringFrom(const Node *startNode)
{	
	std::vector<std::set<const Node*>> layering;
	layering.push_back(std::set<const Node*>());
	layering[0].insert(startNode);
	bool addedNewLayer = true;

	while(addedNewLayer)
	{
		std::set<const Node*> nextLayer;
		auto &lastLayer = layering.back();

		for (auto node : lastLayer)
		{
			auto &neighbours = node->getNeighbours();

			for (auto neighbour : neighbours)
			{
				if (nextLayer.count(neighbour) > 0)
					continue;
				
				if (containedInLayering(layering, neighbour))
					continue;

				nextLayer.insert(neighbour);
			}
		}

		if (nextLayer.empty())
			addedNewLayer = false;
		else
			layering.push_back(nextLayer);
	}

	return layering;
}

std::list<const Node*> Graph::filterOut(std::list<const Node*> const &nodes, std::set<const Node*> const &filter)
{		
	std::list<const Node*> result;

	for (auto node : nodes)
		if (filter.count(node) == 0)
			result.push_back(node);

	return result;
}

bool Graph::containedInLayering(std::vector<std::set<const Node*>> const &layering, const Node *node)
{
	for (auto &layer : layering)
		if (layer.count(node) > 0)
			return true;

	return false;
}