using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class AdmittanceMatrix
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

        public void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode,
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double amplification)
        {
            var inputSourceNodeIndex = _nodeIndexes[inputSourceNode];
            var inputTargetNodeIndex = _nodeIndexes[inputTargetNode];
            var outputSourceNodeIndex = _nodeIndexes[outputSourceNode];
            var outputTargetNodeIndex = _nodeIndexes[outputTargetNode];
            _values[outputSourceNodeIndex, inputSourceNodeIndex] += amplification;
            _values[outputTargetNodeIndex, inputTargetNodeIndex] += amplification;
            _values[outputSourceNodeIndex, inputTargetNodeIndex] -= amplification;
            _values[outputTargetNodeIndex, inputSourceNodeIndex] -= amplification;
        }

        public void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode,
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double amplification)
        {
            AddVoltageControlledCurrentSource(inputSourceNode, inputTargetNode, outputSourceNode, outputTargetNode,
                (-1) * amplification);
            AddVoltageControlledCurrentSource(outputSourceNode, outputTargetNode, inputSourceNode, inputTargetNode, 
                amplification);
        }
    }
}
