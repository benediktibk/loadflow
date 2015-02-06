using System.Collections.Generic;
using Misc;

namespace SincalConnector
{
    public interface IReadOnlyPowerNetData
    {
        double Frequency { get; set; }
        IReadOnlyList<IReadOnlyNode> Nodes { get; }
        IReadOnlyList<INetElement> NetElements { get; }
        IReadOnlyList<FeedIn> FeedIns { get; }
        IReadOnlyList<Load> Loads { get; }
        IReadOnlyList<TransmissionLine> TransmissionLines { get; }
        IReadOnlyList<TwoWindingTransformer> TwoWindingTransformers { get; }
        IReadOnlyList<Generator> Generators { get; }
        IReadOnlyList<SlackGenerator> SlackGenerators { get; }
        IReadOnlyList<ImpedanceLoad> ImpedanceLoads { get; }
        bool ContainsTransformers { get; }
        int CountOfElementsWithSlackBus { get; }
        List<int> AllSupportedElementIdsSorted { get; }
        MultiDictionary<int, int> NodeIdsByElementIds { get; }
        Dictionary<int, IReadOnlyNode> NodeByNodeIds { get; }
    }
}