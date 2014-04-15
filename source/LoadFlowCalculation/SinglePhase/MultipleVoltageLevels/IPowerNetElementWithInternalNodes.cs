using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElementWithInternalNodes : IPowerNetElement
    {
        IList<IReadOnlyNode> GetInternalNodes();
        void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes,
            double scaleBasisPower);
    }
}
