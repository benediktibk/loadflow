using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class SparseMatrixStorage
    {
        private readonly int _dimension;
        private readonly List<List<int>> _columns;
        private readonly List<List<double>> _values;

        public SparseMatrixStorage(int dimension)
        {
            if (dimension <= 0)
                throw new ArgumentOutOfRangeException("dimension", "must be positive");

            _dimension = dimension;
            _values = new List<List<double>>(_dimension);
            _columns = new List<List<int>>(_dimension);

            for (var i = 0; i < _dimension; ++i)
            {
                _values.Add(new List<double>());
                _columns.Add(new List<int>());
            }
        }

        public void Add(int row, int column, double value)
        {
            if (row >= _dimension || row < 0)
                throw new ArgumentOutOfRangeException("row");

            if (column >= _dimension || column < 0)
                throw new ArgumentOutOfRangeException("column");

            var index = _columns[row].BinarySearch(column);

            if (index >= 0)
                _values[row][index] += value;
            else
            {
                index = ~index;
                _columns[row].Insert(index, column);
                _values[row].Insert(index, value);
            }
        }

        public void Subtract(int row, int column, double value)
        {
            Add(row, column, (-1)*value);
        }

        public void Reset()
        {
            foreach (var rowValues in _values)
            {
                for (var i = 0; i < rowValues.Count; ++i)
                    rowValues[i] = 0;
            }
        }

        public Matrix<double> ToMatrix()
        {
            var count = CalculateElementCount();
            var values = new List<Tuple<int, int, double>>(count);

            for (var row = 0; row < _dimension; row++)
                values.AddRange(_columns.Select((t, i) => new Tuple<int, int, double>(row, _columns[row][i], _values[row][i])));

            return SparseMatrix.OfIndexed(_dimension, _dimension, values);
        }

        private int CalculateElementCount()
        {
            return _values.Sum(row => row.Count);
        }
    }
}
