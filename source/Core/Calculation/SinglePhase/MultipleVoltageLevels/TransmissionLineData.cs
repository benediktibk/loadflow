using System;
using System.Numerics;
using MathNet.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class TransmissionLineData
    {
        public TransmissionLineData(double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntCapacityPerUnitLength, double shuntConductancePerUnitLength, double length, double frequency, bool transmissionEquationModel)
        {
            if (seriesResistancePerUnitLength < 0)
                throw new ArgumentOutOfRangeException();

            if (seriesInductancePerUnitLength < 0)
                throw new ArgumentOutOfRangeException();

            if (shuntCapacityPerUnitLength < 0)
                throw new ArgumentOutOfRangeException();

            if (shuntConductancePerUnitLength < 0)
                throw new ArgumentOutOfRangeException();

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "must be positive");

            if (frequency <= 0)
                throw new ArgumentOutOfRangeException("frequency", "must be positive");

            if ((shuntCapacityPerUnitLength <= 0 && shuntConductancePerUnitLength <= 0) || length <= 0)
                CalculateElectricCharacteristicsWithSimplifiedDirectModel(seriesResistancePerUnitLength,
                    seriesInductancePerUnitLength, length, frequency);
            else if (transmissionEquationModel && (seriesResistancePerUnitLength > 0 || seriesInductancePerUnitLength > 0))
                CalculateElectricCharacteristicsWithTransmissionEquationModel(seriesResistancePerUnitLength,
                    seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength,
                    length, frequency);
            else
                CalculateElectricCharacteristicsWithSimplifiedPiModel(seriesResistancePerUnitLength,
                    seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength,
                    length, frequency);

            IsDirectConnection = LengthImpedance.MagnitudeSquared() <= 0;
        }

        public Complex LengthImpedance { get; private set; }

        public Complex ShuntAdmittance { get; private set; }

        public bool NeedsGroundNode { get; private set; }

        public bool IsDirectConnection { get; private set; }

        private void CalculateElectricCharacteristicsWithSimplifiedDirectModel(double lengthResistance, double lengthInductance, double length, double frequency)
        {
            NeedsGroundNode = false;
            LengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
        }

        private void CalculateElectricCharacteristicsWithTransmissionEquationModel(double lengthResistance, double lengthInductance, double shuntCapacity, double shuntConductance, double length, double frequency)
        {
            NeedsGroundNode = true;
            var directLengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
            var directShuntAdmittance = CalculateDirectShuntAdmittance(shuntCapacity, shuntConductance, length, frequency);
            var waveImpedance = Complex.Sqrt(directLengthImpedance / directShuntAdmittance);
            var angle = Complex.Sqrt(directLengthImpedance * directShuntAdmittance);
            LengthImpedance = waveImpedance * Complex.Sinh(angle);
            ShuntAdmittance = (Complex.Tanh(angle / 2) / waveImpedance) / 2;
        }

        private void CalculateElectricCharacteristicsWithSimplifiedPiModel(double lengthResistance,
            double lengthInductance, double shuntCapacity, double shuntConductance, double length, double frequency)
        {
            NeedsGroundNode = true;
            LengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
            ShuntAdmittance = CalculateDirectShuntAdmittance(shuntCapacity, shuntConductance, length, frequency) / 2;
        }

        private static Complex CalculateDirectShuntAdmittance(double shuntCapacity, double shuntConductance, double length, double frequency)
        {
            return new Complex(shuntConductance * length, CalculateOmega(frequency) * shuntCapacity * length);
        }

        private static Complex CalculateDirectLengthImpedance(double lengthResistance, double lengthInductance, double length, double frequency)
        {
            return new Complex(lengthResistance * length, CalculateOmega(frequency) * lengthInductance * length);
        }

        private static double CalculateOmega(double frequency)
        {
            return 2 * Math.PI * frequency;
        }
    }
}
