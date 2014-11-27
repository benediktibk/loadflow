using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IReadOnlyAdmittanceMatrix
    {
        int NodeCount { get; }
        Vector<Complex> CalculateCurrents(Vector<Complex> voltages);
        Complex this[int row, int column] { get; }
        Vector<Complex> GetRow(int row);
        IReadOnlyAdmittanceMatrix CreateReducedAdmittanceMatrix(IReadOnlyList<int> indexOfNodesWithUnknownVoltage, IReadOnlyList<int> indexOfNodesWithKnownVoltage, Vector<Complex> knownVoltages, out Vector<Complex> constantCurrentRightHandSide);
        Vector<Complex> CalculateRowSums();
        Vector<Complex> CalculateAllPowers(Vector<Complex> allVoltages);
        Complex CalculatePowerLoss(Vector<Complex> allVoltages);
        Vector<Complex> CalculateAllPowers(Vector<Complex> voltages, Vector<Complex> constantCurrents);
        double CalculatePowerError(Vector<Complex> voltages,
            Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses);
        IEnumerable<Tuple<int, int, Complex>> EnumerateIndexed();
        void CalculateVoltages(Vector<Complex> x, Vector<Complex> b, IIterativeSolver<Complex> solver, Iterator<Complex> iterator);
    }
}