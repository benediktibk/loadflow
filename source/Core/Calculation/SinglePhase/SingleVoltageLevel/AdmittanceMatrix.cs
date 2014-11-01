using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class AdmittanceMatrix
    {
        private readonly Matrix<Complex> _values;

        public AdmittanceMatrix(int nodeCount)
        {
            if (nodeCount <= 0)
                throw new ArgumentOutOfRangeException("nodeCount", "must be positive");

            _values = new SparseMatrix(nodeCount, nodeCount);
        }

        public AdmittanceMatrix(Matrix<Complex> values)
        {
            if (values.RowCount != values.ColumnCount)
                throw new ArgumentOutOfRangeException("values", "must be quadratic");

            _values = values.Clone();
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

        public void AddVoltageControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, Complex g)
        {
            _values[outputSourceNode, inputSourceNode] += g;
            _values[outputTargetNode, inputTargetNode] += g;
            _values[outputSourceNode, inputTargetNode] -= g;
            _values[outputTargetNode, inputSourceNode] -= g;
        }

        public void AddCurrentControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex amplification, double resistanceWeight)
        {
            AddGyrator(inputSourceNode, inputTargetNode, internalNode, inputTargetNode, resistanceWeight);
            AddVoltageControlledCurrentSource(internalNode, inputTargetNode, outputSourceNode, outputTargetNode,
                amplification / resistanceWeight);
        }

        public void AddVoltageControlledVoltageSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex amplification, double resistanceWeight)
        {
            AddVoltageControlledCurrentSource(inputSourceNode, inputTargetNode, internalNode, outputTargetNode,
                (-1) * amplification / resistanceWeight);
            AddGyrator(internalNode, outputTargetNode, outputSourceNode, outputTargetNode, resistanceWeight);
        }

        public void AddGyrator(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, Complex r)
        {
            var g = 1/r;
            AddVoltageControlledCurrentSource(inputSourceNode, inputTargetNode, outputSourceNode, outputTargetNode, (-1) * g);
            AddVoltageControlledCurrentSource(outputSourceNode, outputTargetNode, inputSourceNode, inputTargetNode, g);
        }

        public void AddIdealTransformer(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, int internalNode, Complex ratio, double resistanceWeight)
        {
            if (ratio.Magnitude <= 0)
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

        public Vector<Complex> CalculateCurrents(Vector<Complex> voltages)
        {
            return _values.Multiply(voltages);
        }

        public Complex this[int row, int column]
        {
            get { return _values[row, column]; }
            set { _values[row, column] = value; }
        }

        public ISolver<Complex> CalculateFactorization()
        {
            return _values.LU();
        }

        public Vector<Complex> GetRow(int row)
        {
            return _values.Row(row);
        }

        public AdmittanceMatrix CreateReducedAdmittanceMatrix(IReadOnlyList<int> indexOfNodesWithUnknownVoltage,
            IReadOnlyList<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages,
            out Vector<Complex> constantCurrentRightHandSide)
        {
            var admittancesToUnknownVoltages = Extract(indexOfNodesWithUnknownVoltage, indexOfNodesWithUnknownVoltage);
            var admittancesToKnownVoltages = Extract(indexOfNodesWithUnknownVoltage, indexOfNodesWithKnownVoltage);
            constantCurrentRightHandSide = admittancesToKnownVoltages.Multiply(knownVoltages)*(-1);
            return new AdmittanceMatrix(admittancesToUnknownVoltages);
        }

        public Vector<Complex> CalculateRowSums()
        {
            var result = new SparseVector(NodeCount);

            var rowIndex = 0;
            foreach (var row in _values.EnumerateRows())
            {
                foreach (var value in row)
                    result[rowIndex] += value;

                ++rowIndex;
            }

            return result;
        }

        private Matrix<Complex> Extract(IReadOnlyList<int> rows, IReadOnlyList<int> columns)
        {
            var matrix = new SparseMatrix(rows.Count, columns.Count);

            for (var targetRowIndex = 0; targetRowIndex < rows.Count; ++targetRowIndex)
            {
                var sourceRowIndex = rows[targetRowIndex];

                for (var targetColumnIndex = 0; targetColumnIndex < columns.Count; ++targetColumnIndex)
                {
                    var sourceColumnIndex = columns[targetColumnIndex];
                    var value = _values[sourceRowIndex, sourceColumnIndex];

                    if (value.Magnitude > 0)
                        matrix[targetRowIndex, targetColumnIndex] = value;
                }
            }

            return matrix;
        }
    }
}
