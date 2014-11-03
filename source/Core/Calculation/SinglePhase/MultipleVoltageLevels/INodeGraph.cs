using System.Collections.Generic;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface INodeGraph : IReadOnlyNodeGraph
    {
        void Add(IExternalReadOnlyNode node);
        IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CalculateNominalPhaseShiftPerNode(FeedIn feedIn, IEnumerable<TwoWindingTransformer> twoWindingTransformers, IEnumerable<ThreeWindingTransformer> threeWindingTransformers);
    }
}