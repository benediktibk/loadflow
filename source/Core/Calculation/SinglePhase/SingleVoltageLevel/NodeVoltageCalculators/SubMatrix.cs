using System;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class SubMatrix
    {
        private readonly SparseMatrixStorage _matrix;
        private readonly int _rowOffset;
        private readonly int _columnOffset;
        private readonly int _rows;
        private readonly int _columns;

        public SubMatrix(SparseMatrixStorage matrix, int rowOffset, int columnOffset, int rows, int columns)
        {
            if (matrix == null)
                throw new ArgumentNullException("matrix");

            if (rowOffset < 0)
                throw new ArgumentOutOfRangeException("rowOffset", "must be positive");

            if (columnOffset < 0)
                throw new ArgumentOutOfRangeException("columnOffset", "must be positive");

            if (rows < 0)
                throw new ArgumentOutOfRangeException("rows", "must be positive");

            if (columns < 0)
                throw new ArgumentOutOfRangeException("columns", "must be positive");

            if (rowOffset + rows > matrix.Dimension)
                throw new ArgumentOutOfRangeException("rows", "too many rows selected");

            if (columnOffset + columns > matrix.Dimension)
                throw new ArgumentOutOfRangeException("columns", "too many columns selected");

            _matrix = matrix;
            _rowOffset = rowOffset;
            _columnOffset = columnOffset;
            _rows = rows;
            _columns = columns;
        }

        public double this[int row, int column]
        {
            get 
            {
                CheckRange(row, column);
                return _matrix[row + _rowOffset, column + _columnOffset];
            }

            set
            {
                CheckRange(row, column);
                _matrix[row + _rowOffset, column + _columnOffset] = value;
            }
        }

        private void CheckRange(int row, int column)
        {
            if (row >= _rows)
                throw new ArgumentOutOfRangeException("row");

            if (column >= _columns)
                throw new ArgumentOutOfRangeException("column");
        }
    }
}
