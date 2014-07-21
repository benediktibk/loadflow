using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public interface IAdmittanceMatrix
    {
        void AddConnection(IReadOnlyNode sourceNode, IReadOnlyNode targetNode, Complex admittance);
        void AddConnection(int sourceNode, int targetNode, Complex admittance);
        void AddUnsymmetricAdmittance(int i, int j, Complex admittance);
        void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, 
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double g);
        void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, 
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double r);
        void AddIdealTransformer(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, IReadOnlyNode internalNode, double ratio, double resistanceWeight);
    }
}