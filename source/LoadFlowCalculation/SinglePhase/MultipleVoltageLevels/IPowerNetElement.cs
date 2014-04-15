using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        bool EnforcesSlackBus { get; }
        bool EnforcesPVBus { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        Complex GetTotalPowerForPQBus(double scaleBasePower);
        Complex GetSlackVoltage(double scaleBasePower);
        IList<IReadOnlyNode> GetInternalNodes();
        void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasisPower);
        bool NominalVoltagesMatch { get; }
    }
}
