using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

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
        void FillInAdmittances(Matrix<Complex> admittances, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasePower, IReadOnlyNode groundNode);

        #endregion
    }
}
