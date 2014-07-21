using System.Collections.Generic;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyPowerNet
    {
        #region properties

        int LoadCount { get; }
        int LineCount { get; }
        int FeedInCount { get; }
        int TransformerCount { get; }
        int GeneratorCount { get; }
        int NodeCount { get; }
        IReadOnlyNode GroundNode { get; }

        #endregion

        #region functions

        bool CheckIfFloatingNodesExists();
        bool CheckIfNominalVoltagesDoNotMatch();
        bool CheckIfNodeIsOverdetermined();
        bool CheckIfGroundNodeIsNecessary();
        IExternalReadOnlyNode GetNodeByName(string name);
        IReadOnlyList<IReadOnlyNode> GetAllNecessaryNodes();
        void FillInAdmittances(AdmittanceMatrix admittances, double scaleBasePower);

        #endregion
    }
}
