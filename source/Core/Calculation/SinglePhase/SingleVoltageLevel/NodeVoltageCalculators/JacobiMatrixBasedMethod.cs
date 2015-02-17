using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public abstract class JacobiMatrixBasedMethod : NodeVoltageCalculator
    {
        protected JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations)
        {
            TargetPrecision = targetPrecision;
            MaximumIterations = maximumIterations;
        }

        public int MaximumIterations { get; private set; }
        public double TargetPrecision { get; private set; }

        public abstract Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<double> pvBusVoltages, double residualImprovementFactor, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex, IReadOnlyDictionary<int, int> busToMatrixIndex);
        public abstract void InitializeMatrixStorage(int pqBusCount, int pvBusCount);

        public override Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            ResetProgress();
            InitializeMatrixStorage(pqBuses.Count, pvBuses.Count);
            Vector<Complex> currentVoltages = DenseVector.OfVector(initialVoltages);
            var iterations = 0;
            IList<double> powersRealDifference;
            IList<double> powersImaginaryDifference;
            CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);
            var pvBusVoltages = new List<double>(pvBuses.Count);
            var pvBusIds = new List<int>(pvBuses.Count);
            var pqBusIds = new List<int>(pqBuses.Count);
            pvBusVoltages.AddRange(pvBuses.Select(bus => bus.VoltageMagnitude));
            pvBusIds.AddRange(pvBuses.Select(bus => bus.Index));
            pqBusIds.AddRange(pqBuses.Select(bus => bus.Index));
            var maximumPower = pvBuses.Select(x => x.RealPower).Concat(pqBuses.Select(x => x.Power.Magnitude)).Max();
            var powerDifferenceBase = maximumPower > 0 ? maximumPower : 1;
            var pqBusToMatrixIndex = CreateMappingBusToMatrixIndex(pqBusIds);
            var pvBusToMatrixIndex = CreateMappingBusToMatrixIndex(pvBusIds);
            var busToMatrixIndex = CreateMappingBusToMatrixIndex(pqBusIds.Concat(pvBusIds).ToList());

            do
            {
                ++iterations;
                currentVoltages = CalculateImprovedVoltages(admittances, currentVoltages, constantCurrents, powersRealDifference, powersImaginaryDifference, pvBusVoltages, 1e-6, pqBusToMatrixIndex, pvBusToMatrixIndex, busToMatrixIndex);
                CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);
                var powersRealDifferenceAbsolute = powersRealDifference.Select(Math.Abs);
                var powersImaginaryDifferenceAbsolute = powersImaginaryDifference.Select(Math.Abs);
                var powersDifferenceAbsolute = powersRealDifferenceAbsolute.Concat(powersImaginaryDifferenceAbsolute);
                var maximumPowerDifference = powersDifferenceAbsolute.Max();
                var relativePowerError = maximumPowerDifference/powerDifferenceBase;
                Progress = (double) iterations/MaximumIterations;
                RelativePowerError = relativePowerError;
            } while (10*RelativePowerError > TargetPrecision && iterations <= MaximumIterations);
            
            return currentVoltages;
        }

        public override double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        public static Vector<double> CombineParts(IList<double> upperParts, IList<double> lowerParts)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(upperParts.Count + lowerParts.Count);

            for (var i = 0; i < upperParts.Count; ++i)
                result[i] = upperParts[i];

            for (var i = 0; i < lowerParts.Count; ++i)
                result[i + upperParts.Count] = lowerParts[i];

            return result;
        }

        public static void DivideParts(IList<double> complete, int firstPartCount, int secondPartCount, out IList<double> firstParts,
            out IList<double> secondParts, out IList<double> thirdParts)
        {
            firstParts = new List<double>(firstPartCount);
            secondParts = new List<double>(secondPartCount);
            var thirdPartCount = complete.Count - firstPartCount - secondPartCount;
            thirdParts = new List<double>(thirdPartCount);

            for (var i = 0; i < firstPartCount; ++i)
                firstParts.Add(complete[i]);

            for (var i = firstPartCount; i < firstPartCount + secondPartCount; ++i)
                secondParts.Add(complete[i]);

            for (var i = firstPartCount + secondPartCount; i < complete.Count; ++i)
                thirdParts.Add(complete[i]);
        }

        public static Dictionary<int, int> CreateMappingBusToMatrixIndex(IList<int> buses)
        {
            var busIdToIndex = new Dictionary<int, int>();
            var i = 0;

            foreach (var bus in buses)
            {
                busIdToIndex.Add(bus, i);
                ++i;
            }

            return busIdToIndex;
        }

        private static void CalculatePowerDifferences(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses,
            Vector<Complex> currentVoltages, out IList<double> powersRealDifference, out IList<double> powersImaginaryDifference)
        {
            var powersCurrent = admittances.CalculateAllPowers(currentVoltages, constantCurrents);
            powersRealDifference = new List<double>(pqBuses.Count + pvBuses.Count);
            powersImaginaryDifference = new List<double>(pqBuses.Count);

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var powerIs = powersCurrent[i].Real;
                var powerShouldBe = pqBuses[i].Power.Real;
                powersRealDifference.Add(powerShouldBe - powerIs);
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                var powerIs = powersCurrent[pqBuses.Count + i].Real;
                var powerShouldBe = pvBuses[i].RealPower;
                powersRealDifference.Add(powerShouldBe - powerIs);
            }

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var powerIs = powersCurrent[i].Imaginary;
                var powerShouldBe = pqBuses[i].Power.Imaginary;
                powersImaginaryDifference.Add(powerShouldBe - powerIs);
            }
        }
    }
}