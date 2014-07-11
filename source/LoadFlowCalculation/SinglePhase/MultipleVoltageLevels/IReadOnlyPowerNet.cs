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
        bool CheckIfGroundNodeIsNecessary();
        IExternalReadOnlyNode GetNodeByName(string name);
        IReadOnlyList<IReadOnlyNode> GetAllNodes();
        void FillInAdmittances(AdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode);

        #endregion
    }
}
