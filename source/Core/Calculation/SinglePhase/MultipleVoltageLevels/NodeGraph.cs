using System.Collections.Generic;
using System.Linq;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class NodeGraph
    {
        private readonly IList<IExternalReadOnlyNode> _nodes;
        private IList<ISet<IExternalReadOnlyNode>> _segments;
        private IList<ISet<IExternalReadOnlyNode>> _segmentsOnSameVoltageLevel;

        public NodeGraph()
        {
            _nodes = new List<IExternalReadOnlyNode>();
            CachedResultsValid = false;
        }

        public bool CachedResultsValid { get; private set; }

        public void Add(IExternalReadOnlyNode node)
        {
            _nodes.Add(node);
            CachedResultsValid = false;
        }

        public IList<ISet<IExternalReadOnlyNode>> Segments
        {
            get
            {
                UpdateCachedResultsIfNecessary();
                return _segments;
            }
        }

        public IList<ISet<IExternalReadOnlyNode>> SegmentsOnSameVoltageLevel
        {
            get
            {
                UpdateCachedResultsIfNecessary();
                return _segmentsOnSameVoltageLevel;
            }
        }

        private void UpdateCachedResultsIfNecessary()
        {
            if (CachedResultsValid)
                return;

            UpdateSegments();
            UpdateSegmentsOnSameVoltageLevel();
            CachedResultsValid = true;
        }

        private void UpdateSegments()
        {
            _segments = new List<ISet<IExternalReadOnlyNode>>();

            if (_nodes.Count == 0)
                return;

            foreach (var node in _nodes)
            {
                var alreadyContained = _segments.Count(segment => segment.Contains(node)) > 0;

                if (alreadyContained)
                    continue;

                var newSegment = new HashSet<IExternalReadOnlyNode>();
                node.AddConnectedNodes(newSegment);
                _segments.Add(newSegment);
            }
        }

        private void UpdateSegmentsOnSameVoltageLevel()
        {
            _segmentsOnSameVoltageLevel = new List<ISet<IExternalReadOnlyNode>>();

            if (_nodes.Count == 0)
                return;

            foreach (var node in _nodes)
            {
                var alreadyContained = _segmentsOnSameVoltageLevel.Count(segment => segment.Contains(node)) > 0;

                if (alreadyContained)
                    continue;

                var newSegment = new HashSet<IExternalReadOnlyNode>();
                node.AddConnectedNodesOnSameVoltageLevel(newSegment);
                _segmentsOnSameVoltageLevel.Add(newSegment);
            }
        }
    }
}
