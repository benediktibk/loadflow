using System.Collections.Generic;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface INodeGraph : IReadOnlyNodeGraph
    {
        void Add(IExternalReadOnlyNode node);
        IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CalculateNominalPhaseShiftPerNode(IEnumerable<IExternalReadOnlyNode> feedInNodes, IReadOnlyList<TwoWindingTransformer> twoWindingTransformers, IReadOnlyList<ThreeWindingTransformer> threeWindingTransformers);
    }
}