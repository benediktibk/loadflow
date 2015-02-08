using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class NodeGraph : INodeGraph
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

        public bool FloatingNodesExist
        {
            get { return Segments.Count != 1; }
        }

        public IList<IExternalReadOnlyNode> FloatingNodes
        {
            get
            {
                var mainSegmentNodeCount = Segments.Max(x => x.Count);
                var floatingSegments = Segments.Where(segment => segment.Count < mainSegmentNodeCount);
                var floatingNodes = new List<IExternalReadOnlyNode>();

                foreach (var segment in floatingSegments)
                    floatingNodes.AddRange(segment);

                return floatingNodes;
            }
        }

        public int NodeCount
        {
            get { return _nodes.Count; }
        }

        public void Add(IExternalReadOnlyNode node)
        {
            _nodes.Add(node);
            CachedResultsValid = false;
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CalculateNominalPhaseShiftPerNode(IEnumerable<IExternalReadOnlyNode> feedInNodes, IReadOnlyList<TwoWindingTransformer> twoWindingTransformers, IReadOnlyList<ThreeWindingTransformer> threeWindingTransformers)
        {
            var segments = new HashSet<ISet<IExternalReadOnlyNode>>();

            foreach (var node in feedInNodes)
                segments.Add(FindSegmentWhichContains(SegmentsOnSameVoltageLevel, node));

            var result = new Dictionary<IExternalReadOnlyNode, Angle>();

            foreach (var segment in segments)
            {
                var phaseShiftsPerTransformer = CreatePhaseShiftsPerTransformer(SegmentsOnSameVoltageLevel, twoWindingTransformers, threeWindingTransformers);
                var phaseShiftBySegmentToAllSegments = CreatePhaseShiftBySegmentToAllSegments(phaseShiftsPerTransformer);
                var phaseShiftBySegment = GetNominalPhaseShiftBySegment(segment, phaseShiftBySegmentToAllSegments);
                var partialResult = CreateDictionaryPhaseShiftByNode(SegmentsOnSameVoltageLevel, phaseShiftBySegment);

                foreach (var pair in partialResult)
                    result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        private static ISet<IExternalReadOnlyNode> FindSegmentWhichContains(IList<ISet<IExternalReadOnlyNode>> segments, IExternalReadOnlyNode node)
        {
            ISet<IExternalReadOnlyNode> segmentWhichContainsNode = null;

            for (var i = 0; i < segments.Count && segmentWhichContainsNode == null; ++i)
            {
                var segment = segments[i];

                if (segment.Contains(node))
                    segmentWhichContainsNode = segment;
            }

            if (segmentWhichContainsNode == null)
                throw new InvalidDataException("the node is not part of the segments");

            return segmentWhichContainsNode;
        }

        private static IReadOnlyDictionary<ISet<IExternalReadOnlyNode>, Angle> GetNominalPhaseShiftBySegment(ISet<IExternalReadOnlyNode> segmentWithFeedIn,
            IReadOnlyMultiDictionary<ISet<IExternalReadOnlyNode>, Tuple<ISet<IExternalReadOnlyNode>, Angle>> phaseShiftBySegmentToAllSegments)
        {
            var phaseShiftBySegment = new Dictionary<ISet<IExternalReadOnlyNode>, Angle>
            {
                {segmentWithFeedIn, new Angle(0)}
            };
            var lastSegments = new List<ISet<IExternalReadOnlyNode>>() { segmentWithFeedIn };

            do
            {
                var nextSegments = new List<ISet<IExternalReadOnlyNode>>();

                foreach (var lastSegment in lastSegments)
                {
                    var phaseShiftsToOtherSegments = phaseShiftBySegmentToAllSegments.Get(lastSegment);
                    var ownPhaseShift = phaseShiftBySegment[lastSegment];

                    foreach (var element in phaseShiftsToOtherSegments)
                    {
                        var phaseShift = element.Item2 + ownPhaseShift;
                        var otherSegment = element.Item1;
                        Angle previousPhaseShift;

                        if (phaseShiftBySegment.TryGetValue(otherSegment, out previousPhaseShift))
                        {
                            if (!Angle.Equal(previousPhaseShift, phaseShift, 0.000001))
                                throw new InvalidDataException("the phase shifts do not match");
                        }
                        else
                        {
                            phaseShiftBySegment.Add(otherSegment, phaseShift);
                            nextSegments.Add(otherSegment);
                        }
                    }
                }

                lastSegments = nextSegments;
            } while (lastSegments.Count > 0);
            return phaseShiftBySegment;
        }

        private static IReadOnlyMultiDictionary<ISet<IExternalReadOnlyNode>, Tuple<ISet<IExternalReadOnlyNode>, Angle>> CreatePhaseShiftBySegmentToAllSegments(IEnumerable<KeyValuePair<Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>, Angle>> phaseShiftsPerTransformer)
        {
            var phaseShiftBySegmentToAllSegments =
                new MultiDictionary<ISet<IExternalReadOnlyNode>, Tuple<ISet<IExternalReadOnlyNode>, Angle>>();

            foreach (var connection in phaseShiftsPerTransformer)
            {
                var firstSegment = connection.Key.Item1;
                var secondSegment = connection.Key.Item2;
                var phaseShift = connection.Value;
                phaseShiftBySegmentToAllSegments.Add(firstSegment,
                    new Tuple<ISet<IExternalReadOnlyNode>, Angle>(secondSegment, phaseShift));
                phaseShiftBySegmentToAllSegments.Add(secondSegment,
                    new Tuple<ISet<IExternalReadOnlyNode>, Angle>(firstSegment, (-1) * phaseShift));
            }

            return phaseShiftBySegmentToAllSegments;
        }

        private static void AddPhaseShiftBetweenSegmentsAndCheckExistingShifts(ISet<IExternalReadOnlyNode> upperSegment, ISet<IExternalReadOnlyNode> lowerSegment,
            IDictionary<Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>, Angle> phaseShiftsPerTransformer, Angle phaseShift)
        {
            var segmentPair = new Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>(upperSegment, lowerSegment);
            var segmentPairInverse = new Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>(lowerSegment,
                upperSegment);

            if (phaseShiftsPerTransformer.ContainsKey(segmentPair))
            {
                var previousPhaseShift = phaseShiftsPerTransformer[segmentPair];
                if (!Angle.Equal(phaseShift, previousPhaseShift, 0.000001))
                    throw new InvalidDataException("the nominal phase shifts of two transformers do not match");
            }
            else if (phaseShiftsPerTransformer.ContainsKey(segmentPairInverse))
            {
                var previousPhaseShift = phaseShiftsPerTransformer[segmentPairInverse];
                if (!Angle.Equal(phaseShift*(-1), previousPhaseShift, 0.000001))
                    throw new InvalidDataException("the nominal phase shifts of two transformers do not match");
            }
            else
                phaseShiftsPerTransformer.Add(segmentPair, phaseShift);
        }

        private static IEnumerable<KeyValuePair<Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>, Angle>> CreatePhaseShiftsPerTransformer(IList<ISet<IExternalReadOnlyNode>> segments, IEnumerable<TwoWindingTransformer> twoWindingTransformers, IEnumerable<ThreeWindingTransformer> threeWindingTransformers)
        {
            var phaseShiftsPerTransformer =
                new Dictionary<Tuple<ISet<IExternalReadOnlyNode>, ISet<IExternalReadOnlyNode>>, Angle>();

            foreach (var transformer in twoWindingTransformers)
            {
                var upperSegment = FindSegmentWhichContains(segments, transformer.UpperSideNode);
                var lowerSegment = FindSegmentWhichContains(segments, transformer.LowerSideNode);
                AddPhaseShiftBetweenSegmentsAndCheckExistingShifts(upperSegment, lowerSegment, phaseShiftsPerTransformer, transformer.NominalPhaseShift);
            }

            foreach (var transformer in threeWindingTransformers)
            {
                var segmentOne = FindSegmentWhichContains(segments, transformer.NodeOne);
                var segmentTwo = FindSegmentWhichContains(segments, transformer.NodeTwo);
                var segmentThree = FindSegmentWhichContains(segments, transformer.NodeThree);
                AddPhaseShiftBetweenSegmentsAndCheckExistingShifts(segmentOne, segmentTwo, phaseShiftsPerTransformer,
                    transformer.NominalPhaseShiftOne + transformer.NominalPhaseShiftTwo);
                AddPhaseShiftBetweenSegmentsAndCheckExistingShifts(segmentTwo, segmentThree, phaseShiftsPerTransformer,
                    (-1)*transformer.NominalPhaseShiftTwo + transformer.NominalPhaseShiftThree);
                AddPhaseShiftBetweenSegmentsAndCheckExistingShifts(segmentOne, segmentThree, phaseShiftsPerTransformer,
                    transformer.NominalPhaseShiftThree + transformer.NominalPhaseShiftOne);
            }

            return phaseShiftsPerTransformer;
        }

        private IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CreateDictionaryPhaseShiftByNode(IList<ISet<IExternalReadOnlyNode>> segments, IReadOnlyDictionary<ISet<IExternalReadOnlyNode>, Angle> phaseShiftBySegment)
        {
            var result = new Dictionary<IExternalReadOnlyNode, Angle>();

            foreach (var node in _nodes)
            {
                var segment = FindSegmentWhichContains(segments, node);
                result.Add(node, phaseShiftBySegment[segment]);
            }

            return result;
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
