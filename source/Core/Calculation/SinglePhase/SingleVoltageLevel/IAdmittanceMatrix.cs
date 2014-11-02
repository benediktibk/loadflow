using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IAdmittanceMatrix : IReadOnlyAdmittanceMatrix
    {
        void AddConnection(int sourceNode, int targetNode, Complex admittance);
        void AddUnsymmetricAdmittance(int i, int j, Complex admittance);
        void AddVoltageControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, Complex g);
        void AddCurrentControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex amplification, double resistanceWeight);
        void AddVoltageControlledVoltageSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex amplification, double resistanceWeight);
        void AddGyrator(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, Complex r);
        void AddIdealTransformer(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex ratio, double resistanceWeight);
    }
}