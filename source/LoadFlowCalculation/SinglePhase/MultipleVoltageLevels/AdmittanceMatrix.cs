using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class AdmittanceMatrix : IAdmittanceMatrix
    {
        private readonly Matrix<Complex> _values;
        private readonly IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;

        public AdmittanceMatrix(int nodeCount, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            _values = new SparseMatrix(nodeCount, nodeCount);
            _nodeIndexes = nodeIndexes;
        }

        public AdmittanceMatrix(Matrix<Complex> values, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            _values = values.Clone();
            _nodeIndexes = nodeIndexes;
        }

        public Matrix<Complex> GetValues()
        {
            return _values.Clone();
        }

        public void AddConnection(IReadOnlyNode sourceNode, IReadOnlyNode targetNode, Complex admittance)
        {
            var sourceNodeIndex = _nodeIndexes[sourceNode];
            var targetNodeIndex = _nodeIndexes[targetNode];
            Debug.Assert(sourceNodeIndex != targetNodeIndex);
            _values[sourceNodeIndex, sourceNodeIndex] += admittance;
            _values[targetNodeIndex, targetNodeIndex] += admittance;
            _values[sourceNodeIndex, targetNodeIndex] -= admittance;
            _values[targetNodeIndex, sourceNodeIndex] -= admittance;
        }

        public void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double g)
        {
            var inputSourceNodeIndex = _nodeIndexes[inputSourceNode];
            var inputTargetNodeIndex = _nodeIndexes[inputTargetNode];
            var outputSourceNodeIndex = _nodeIndexes[outputSourceNode];
            var outputTargetNodeIndex = _nodeIndexes[outputTargetNode];
            _values[outputSourceNodeIndex, inputSourceNodeIndex] += g;
            _values[outputTargetNodeIndex, inputTargetNodeIndex] += g;
            _values[outputSourceNodeIndex, inputTargetNodeIndex] -= g;
            _values[outputTargetNodeIndex, inputSourceNodeIndex] -= g;
        }

        public void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double r)
        {
            AddVoltageControlledCurrentSource(inputSourceNode, inputTargetNode, outputSourceNode, outputTargetNode,
                (-1) / r);
            AddVoltageControlledCurrentSource(outputSourceNode, outputTargetNode, inputSourceNode, inputTargetNode, 
                1 / r);
        }

        public void AddIdealTransformer(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, IReadOnlyNode internalNode, double ratio, double resistanceWeight)
        {
            if (ratio <= 0)
                throw new ArgumentOutOfRangeException("ratio", "must be positive");

            if (resistanceWeight <= 0)
                throw new ArgumentOutOfRangeException("resistanceWeight", "must be positive");

            AddGyrator(inputSourceNode, inputTargetNode, internalNode, inputTargetNode, ratio*resistanceWeight);
            AddGyrator(internalNode, inputTargetNode, outputSourceNode, outputTargetNode, resistanceWeight);
        }
    }
}
