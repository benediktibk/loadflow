using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface IReadOnlyPowerNet
    {
        #region properties

        int LoadCount { get; }
        int LineCount { get; }
        int FeedInCount { get; }
        int TransformatorCount { get; }
        int GeneratorCount { get; }
        int NodeCount { get; }

        #endregion

        #region functions

        bool CheckIfFloatingNodesExists();
        bool CheckIfNominalVoltagesDoNotMatch();
        bool CheckIfNodeIsOverdetermined();
        IReadOnlyNode GetNodeByName(string name);
        IReadOnlyList<IReadOnlyNode> GetNodes();
        IReadOnlyList<Line> GetLines();
        IReadOnlyList<Load> GetLoads();

        #endregion
    }
}
