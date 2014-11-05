using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class TransmissionLine : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly IExternalReadOnlyNode _targetNode;
        private Complex _lengthImpedance;
        private Complex _shuntAdmittance;
        private bool _hasShuntAdmittance;

        public TransmissionLine(IExternalReadOnlyNode sourceNode, IExternalReadOnlyNode targetNode, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntCapacityPerUnitLength, double shuntConductancePerUnitLength, double length, double frequency, bool transmissionEquationModel)
        {
            if (seriesResistancePerUnitLength <= 0 && seriesInductancePerUnitLength <= 0)
                throw new ArgumentOutOfRangeException();

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "must be positive");

            if (frequency <= 0)
                throw new ArgumentOutOfRangeException("frequency", "must be positive");

            _sourceNode = sourceNode;
            _targetNode = targetNode;

            if (shuntCapacityPerUnitLength == 0 && shuntConductancePerUnitLength == 0)
                CalculateElectricCharacteristicsWithSimplifiedDirectModel(seriesResistancePerUnitLength,
                    seriesInductancePerUnitLength, length, frequency);
            else if (transmissionEquationModel)
                CalculateElectricCharacteristicsWithTransmissionEquationModel(seriesResistancePerUnitLength, 
                    seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength, 
                    length, frequency);
            else
                CalculateElectricCharacteristicsWithSimplifiedPiModel(seriesResistancePerUnitLength, 
                    seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength, 
                    length, frequency);
        }

        public Complex LengthImpedance
        {
            get { return _lengthImpedance; }
        }

        public Complex ShuntAdmittance
        {
            get { return _shuntAdmittance; }
        }

        public double SourceNominalVoltage
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public double TargetNominalVoltage
        {
            get { return _targetNode.NominalVoltage; }
        }

        public bool NominalVoltagesMatch
        {
            get { return Math.Abs((TargetNominalVoltage - SourceNominalVoltage)/TargetNominalVoltage) < 0.0000001; }
        }

        public bool NeedsGroundNode
        {
            get { return _hasShuntAdmittance; }
        }

        public INode CreateSingleVoltageNode(double scaleBasePower)
        {
            return new PqNode(new Complex());
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            _targetNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            Debug.Assert(!NeedsGroundNode || groundNode != null);

            var scaler = new DimensionScaler(TargetNominalVoltage, scaleBasisPower);
            var lengthAdmittanceScaled = scaler.ScaleAdmittance(1.0/LengthImpedance);
            admittances.AddConnection(_sourceNode, _targetNode, lengthAdmittanceScaled);

            if (!NeedsGroundNode)
                return;

            var shuntAdmittanceScaled = scaler.ScaleAdmittance(ShuntAdmittance);
            admittances.AddConnection(_sourceNode, groundNode, shuntAdmittanceScaled);
            admittances.AddConnection(_targetNode, groundNode, shuntAdmittanceScaled);
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }

        private void CalculateElectricCharacteristicsWithSimplifiedDirectModel(double lengthResistance,
            double lengthInductance, double length, double frequency)
        {
            _hasShuntAdmittance = false;
            _lengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
        }

        private void CalculateElectricCharacteristicsWithTransmissionEquationModel(double lengthResistance, double lengthInductance, double shuntCapacity, double shuntConductance, double length, double frequency)
        {
            _hasShuntAdmittance = true;
            var directLengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
            var directShuntAdmittance = CalculateDirectShuntAdmittance(shuntCapacity, shuntConductance, length, frequency);
            var waveImpedance = Complex.Sqrt(directLengthImpedance / directShuntAdmittance);
            var angle = Complex.Sqrt(directLengthImpedance * directShuntAdmittance);
            _lengthImpedance = waveImpedance * Complex.Sinh(angle); 
            _shuntAdmittance = (Complex.Tanh(angle / 2) / waveImpedance) / 2;
        }

        private void CalculateElectricCharacteristicsWithSimplifiedPiModel(double lengthResistance,
            double lengthInductance, double shuntCapacity, double shuntConductance, double length, double frequency)
        {
            _hasShuntAdmittance = true;
            _lengthImpedance = CalculateDirectLengthImpedance(lengthResistance, lengthInductance, length, frequency);
            _shuntAdmittance = CalculateDirectShuntAdmittance(shuntCapacity, shuntConductance, length, frequency)/2;
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
