using System.Collections.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
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
        IExternalReadOnlyNode GetNodeByName(string name);
        IReadOnlyList<IExternalReadOnlyNode> GetNodes();
        IReadOnlyList<IPowerNetElement> GetElements();

        #endregion
    }
}
