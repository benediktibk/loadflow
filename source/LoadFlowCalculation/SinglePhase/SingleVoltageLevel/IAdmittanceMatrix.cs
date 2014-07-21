using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public interface IAdmittanceMatrix
    {
        void AddConnection(int sourceNode, int targetNode, Complex admittance);
        void AddUnsymmetricAdmittance(int i, int j, Complex admittance);
        void AddVoltageControlledCurrentSource(int inputSourceNode, int inputTargetNode,
            int outputSourceNode, int outputTargetNode, double g);
        void AddGyrator(int inputSourceNode, int inputTargetNode,
            int outputSourceNode, int outputTargetNode, double r);
        void AddIdealTransformer(int inputSourceNode, int inputTargetNode, int outputSourceNode, 
            int outputTargetNode, int internalNode, double ratio, double resistanceWeight);
        Matrix<Complex> GetValues();
        int NodeCount { get; }
    }
}