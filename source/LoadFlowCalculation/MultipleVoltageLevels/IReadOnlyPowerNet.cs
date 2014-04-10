using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        INode GetNodeByName(string name);

        #endregion
    }
}
