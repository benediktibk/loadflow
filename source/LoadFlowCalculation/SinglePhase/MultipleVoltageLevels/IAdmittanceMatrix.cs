using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IAdmittanceMatrix
    {
        void AddConnection(IReadOnlyNode sourceNode, IReadOnlyNode targetNode, Complex admittance);

        void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, 
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double g);

        void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, 
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double r);

        void AddIdealTransformer(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, IReadOnlyNode internalNode, double ratio, double resistanceWeight);
    }
}