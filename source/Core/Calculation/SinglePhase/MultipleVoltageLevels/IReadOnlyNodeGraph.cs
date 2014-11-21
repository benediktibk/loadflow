using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNodeGraph
    {
        IList<ISet<IExternalReadOnlyNode>> Segments { get; }
        IList<ISet<IExternalReadOnlyNode>> SegmentsOnSameVoltageLevel { get; }
        bool FloatingNodesExist { get; }
        IList<IExternalReadOnlyNode> FloatingNodes { get; } 
    }
}