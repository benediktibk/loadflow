using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IAdmittanceMatrix
    {
        void AddConnection(IReadOnlyNode sourceNode, IReadOnlyNode targetNode, Complex admittance);
        void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode,
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double g);
        void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode,
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double r);
        void AddIdealTransformer(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, IReadOnlyNode internalNode, Complex ratio, double resistanceWeight);
        SingleVoltageLevel.AdmittanceMatrix GetSingleVoltageAdmittanceMatrix();
        int NodeCount { get; }
        Complex this[int row, int column] { get; }
    }
}
