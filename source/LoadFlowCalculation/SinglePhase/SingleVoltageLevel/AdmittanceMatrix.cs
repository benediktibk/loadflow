using System;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public class AdmittanceMatrix : IAdmittanceMatrix
    {
        private readonly Matrix<Complex> _values;

        public AdmittanceMatrix(int nodeCount)
        {
            _values = new SparseMatrix(nodeCount, nodeCount);
        }

        public AdmittanceMatrix(Matrix<Complex> values)
        {
            _values = values.Clone();
        }

        public Matrix<Complex> GetValues()
        {
            return _values.Clone();
        }

        public void AddConnection(int sourceNode, int targetNode, Complex admittance)
        {
            Debug.Assert(sourceNode != targetNode);
            _values[sourceNode, sourceNode] += admittance;
            _values[targetNode, targetNode] += admittance;
            _values[sourceNode, targetNode] -= admittance;
            _values[targetNode, sourceNode] -= admittance;
        }

        public void AddUnsymmetricAdmittance(int i, int j, Complex admittance)
        {
            _values[i, j] += admittance;
        }

        public void AddVoltageControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, double g)
        {
            _values[outputSourceNode, inputSourceNode] += g;
            _values[outputTargetNode, inputTargetNode] += g;
            _values[outputSourceNode, inputTargetNode] -= g;
            _values[outputTargetNode, inputSourceNode] -= g;
        }

        public void AddGyrator(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, double r)
        {
            AddVoltageControlledCurrentSource(inputSourceNode, inputTargetNode, outputSourceNode, outputTargetNode,
                (-1) / r);
            AddVoltageControlledCurrentSource(outputSourceNode, outputTargetNode, inputSourceNode, inputTargetNode, 
                1 / r);
        }

        public void AddIdealTransformer(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, double ratio, double resistanceWeight)
        {
            if (ratio <= 0)
                throw new ArgumentOutOfRangeException("ratio", "must be positive");

            if (resistanceWeight <= 0)
                throw new ArgumentOutOfRangeException("resistanceWeight", "must be positive");

            AddGyrator(inputSourceNode, inputTargetNode, internalNode, inputTargetNode, ratio*resistanceWeight);
            AddGyrator(internalNode, inputTargetNode, outputSourceNode, outputTargetNode, resistanceWeight);
        }

        public int NodeCount
        {
            get { return _values.ColumnCount; }
        }
    }
}
