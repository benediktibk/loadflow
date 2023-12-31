﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Solvers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class AdmittanceMatrix : IAdmittanceMatrix
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

        public Complex CalculatePowerLoss(Vector<Complex> allVoltages)
        {
            var powerLoss = new Complex();

            for (var i = 0; i < NodeCount; ++i)
                for (var j = i + 1; j < NodeCount; ++j)
                {
                    var admittance = this[i, j];
                    var voltageDifference = allVoltages[i] - allVoltages[j];
                    var branchCurrent = admittance * voltageDifference;
                    var branchPowerLoss = voltageDifference * branchCurrent.Conjugate();
                    powerLoss += branchPowerLoss;
                }

            return powerLoss * (-1);
        }

        public double CalculatePowerError(Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            var powers = CalculateAllPowers(voltages, constantCurrents);
            return
                pqBuses.Sum(bus => Math.Abs(bus.Power.Real - powers[bus.Index].Real) + Math.Abs(bus.Power.Imaginary - powers[bus.Index].Imaginary)) +
                pvBuses.Sum(bus => Math.Abs(bus.RealPower - powers[bus.Index].Real));
        }

        public IEnumerable<Tuple<int, int, Complex>> EnumerateIndexed()
        {
            return _values.EnumerateIndexed(Zeros.AllowSkip);
        }

        public void CalculateVoltages(Vector<Complex> x, Vector<Complex> b, IIterativeSolver<Complex> solver, Iterator<Complex> iterator)
        {
/*            var customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            File.WriteAllLines("C:\\temp\\vector.csv", b.Select(y => "(" + y.Real + "," + y.Imaginary + ")"));*/
            solver.Solve(_values, b, x, iterator, new DiagonalPreconditioner());
        }

        public LU<Complex> CalculateFactorization()
        {
            return _values.LU();
        }

        public double FindMaximumMagnitude()
        {
            var values = _values.Enumerate(Zeros.AllowSkip);
            var resultSquared = values.Select(value => value.MagnitudeSquared()).Max();
            return Math.Sqrt(resultSquared);
        }

        public Vector<Complex> CalculateAllPowers(Vector<Complex> voltages, Vector<Complex> constantCurrents)
        {
            var currents = CalculateCurrents(voltages) - constantCurrents;
            var powers = voltages.PointwiseMultiply(currents.Conjugate());
            return powers;
        }

        public Vector<Complex> CalculateAllPowers(Vector<Complex> allVoltages)
        {
            var currents = CalculateCurrents(allVoltages);
            var allPowers = allVoltages.PointwiseMultiply(currents.Conjugate());
            return allPowers;
        }

        public void AddConnection(int sourceNode, int targetNode, Complex admittance)
        {
            if (IsInvalid(admittance))
                throw new ArgumentOutOfRangeException("admittance");

            _values[sourceNode, sourceNode] += admittance;
            _values[targetNode, targetNode] += admittance;
            _values[sourceNode, targetNode] -= admittance;
            _values[targetNode, sourceNode] -= admittance;
        }

        public void AddUnsymmetricAdmittance(int i, int j, Complex admittance)
        {
            if (IsInvalid(admittance))
                throw new ArgumentOutOfRangeException("admittance");

            _values[i, j] += admittance;
        }

        public void AddVoltageControlledCurrentSource(int inputSourceNode, int inputTargetNode, int outputSourceNode, int outputTargetNode, Complex g)
        {
            if (IsInvalid(g))
                throw new ArgumentOutOfRangeException("g");

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
        }

        public Vector<Complex> GetRow(int row)
        {
            var result = new SparseVector(NodeCount);
            _values.Row(row).CopyTo(result);
            return result;
        }

        public IReadOnlyAdmittanceMatrix CreateReducedAdmittanceMatrix(IReadOnlyList<int> indexOfNodesWithUnknownVoltage, IReadOnlyList<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages, out Vector<Complex> constantCurrentRightHandSide)
        {
            var unknownVoltagesIndices = DetermineNewIndices(indexOfNodesWithUnknownVoltage);
            var knownVoltagesIndices = DetermineNewIndices(indexOfNodesWithKnownVoltage);
            var admittancesToUnknownVoltages = Extract(unknownVoltagesIndices, unknownVoltagesIndices);
            var admittancesToKnownVoltages = Extract(unknownVoltagesIndices, knownVoltagesIndices);
            constantCurrentRightHandSide = admittancesToKnownVoltages.Multiply(knownVoltages)*(-1);
            return new AdmittanceMatrix(admittancesToUnknownVoltages);
        }

        public Vector<Complex> CalculateRowSums()
        {
            return _values.RowSums();
        }

        private static Dictionary<int, int> DetermineNewIndices(IEnumerable<int> indexOfNodesWithUnknownVoltage)
        {
            var unknownRows = new Dictionary<int, int>();

            var newIndex = 0;
            foreach (var oldIndex in indexOfNodesWithUnknownVoltage)
            {
                unknownRows[oldIndex] = newIndex;
                ++newIndex;
            }

            return unknownRows;
        }

        private static bool IsInvalid(Complex value)
        {
            var magnitude = value.Magnitude;
            return Double.IsNaN(magnitude) || Double.IsInfinity(magnitude);
        }

        private Matrix<Complex> Extract(IReadOnlyDictionary<int, int> rows, IReadOnlyDictionary<int, int> columns)
        {
            var matrix = new SparseMatrix(rows.Count, columns.Count);

            foreach (var entry in _values.EnumerateIndexed(Zeros.AllowSkip))
            {
                int row;
                if (!rows.TryGetValue(entry.Item1, out row)) continue;

                int column;
                if (columns.TryGetValue(entry.Item2, out column))
                    matrix[row, column] = entry.Item3;
            }

            return matrix;
        }
    }
}
