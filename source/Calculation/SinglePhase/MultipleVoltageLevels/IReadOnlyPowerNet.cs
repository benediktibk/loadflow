using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyPowerNet
    {
        #region properties

        int LoadCount { get; }
        int ImpedanceLoadCount { get; }
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
        bool IsGroundNodeNecessary();
        IReadOnlyList<IReadOnlyNode> GetAllNecessaryNodes();
        void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower);

        #endregion
    }
}
