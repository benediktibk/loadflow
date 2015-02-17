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

        public int Dimension
        {
            get { return _dimension; }
        }

        public double this[int row, int column]
        {
            get
            {
                CheckRange(row, column);
                var columns = _columns[row];
                var values = _values[row];
                var index = columns.BinarySearch(column);
                return index < 0 ? 0 : values[index];
            }

            set
            {
                CheckRange(row, column);
                var columns = _columns[row];
                var values = _values[row];
                var index = columns.BinarySearch(column);

                if (index >= 0)
                    values[index] = value;
                else
                {
                    index = ~index;
                    columns.Insert(index, column);
                    values.Insert(index, value);
                }
            }
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
            var valueList = new List<Tuple<int, int, double>>(count);

            for (var row = 0; row < _dimension; row++)
            {
                var columns = _columns[row];
                var values = _values[row];
                valueList.AddRange(columns.Select((t, i) => new Tuple<int, int, double>(row, t, values[i])));
            }

            return SparseMatrix.OfIndexed(_dimension, _dimension, valueList);
        }

        private void CheckRange(int row, int column)
        {
            if (row >= _dimension || row < 0)
                throw new ArgumentOutOfRangeException("row");

            if (column >= _dimension || column < 0)
                throw new ArgumentOutOfRangeException("column");
        }

        private int CalculateElementCount()
        {
            return _values.Sum(row => row.Count);
        }
    }
}
