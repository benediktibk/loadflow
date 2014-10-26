using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class AdmittanceMatrix : IAdmittanceMatrix
    {
        #region variables

        private readonly SingleVoltageLevel.AdmittanceMatrix _values;
        private readonly IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;

        #endregion

        #region constructors

        public AdmittanceMatrix(int nodeCount, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            _values = new SingleVoltageLevel.AdmittanceMatrix(nodeCount);
            _nodeIndexes = nodeIndexes;
        }

        public AdmittanceMatrix(Matrix<Complex> values, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            _values = new SingleVoltageLevel.AdmittanceMatrix(values);
            _nodeIndexes = nodeIndexes;
        }

        #endregion

        #region public functions

        public void AddConnection(IReadOnlyNode sourceNode, IReadOnlyNode targetNode, Complex admittance)
        {
            var sourceNodeIndex = _nodeIndexes[sourceNode];
            var targetNodeIndex = _nodeIndexes[targetNode];
            _values.AddConnection(sourceNodeIndex, targetNodeIndex, admittance);
        }

        public void AddVoltageControlledCurrentSource(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode,
            IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, double g)
        {
            var inputSourceNodeIndex = _nodeIndexes[inputSourceNode];
            var inputTargetNodeIndex = _nodeIndexes[inputTargetNode];
            var outputSourceNodeIndex = _nodeIndexes[outputSourceNode];
            var outputTargetNodeIndex = _nodeIndexes[outputTargetNode];
            _values.AddVoltageControlledCurrentSource(inputSourceNodeIndex, inputTargetNodeIndex, outputSourceNodeIndex,
                outputTargetNodeIndex, g);
        }

        public void AddGyrator(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode,
            IReadOnlyNode outputTargetNode, double r)
        {
            var inputSourceNodeIndex = _nodeIndexes[inputSourceNode];
            var inputTargetNodeIndex = _nodeIndexes[inputTargetNode];
            var outputSourceNodeIndex = _nodeIndexes[outputSourceNode];
            var outputTargetNodeIndex = _nodeIndexes[outputTargetNode];
            _values.AddGyrator(inputSourceNodeIndex, inputTargetNodeIndex, outputSourceNodeIndex, outputTargetNodeIndex, r);
        }

        public void AddIdealTransformer(IReadOnlyNode inputSourceNode, IReadOnlyNode inputTargetNode, IReadOnlyNode outputSourceNode, IReadOnlyNode outputTargetNode, IReadOnlyNode internalNode, Complex ratio, double resistanceWeight)
        {
            var inputSourceNodeIndex = _nodeIndexes[inputSourceNode];
            var inputTargetNodeIndex = _nodeIndexes[inputTargetNode];
            var outputSourceNodeIndex = _nodeIndexes[outputSourceNode];
            var outputTargetNodeIndex = _nodeIndexes[outputTargetNode];
            var internalNodeIndex = _nodeIndexes[internalNode];
            _values.AddIdealTransformer(inputSourceNodeIndex, inputTargetNodeIndex, outputSourceNodeIndex,
                outputTargetNodeIndex, internalNodeIndex, ratio, resistanceWeight);
        }

        public SingleVoltageLevel.AdmittanceMatrix GetSingleVoltageAdmittanceMatrix()
        {
            return _values;
        }

        public int NodeCount
        {
            get { return _values.NodeCount; }
        }

        public Complex this[int row, int column]
        {
            get { return _values[row, column]; }
        }

        #endregion
    }
}
