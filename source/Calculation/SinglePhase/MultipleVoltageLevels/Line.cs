using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Line : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly IExternalReadOnlyNode _targetNode;
        private Complex _lengthImpedance;
        private Complex _shuntAdmittance;
        private bool _hasShuntAdmittance;

        public Line(IExternalReadOnlyNode sourceNode, IExternalReadOnlyNode targetNode, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntCapacityPerUnitLength, double shuntConductancePerUnitLength, double length, double frequency)
        {
            if (seriesResistancePerUnitLength <= 0 && seriesInductancePerUnitLength <= 0)
                throw new ArgumentOutOfRangeException();

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "must be positive");

            _sourceNode = sourceNode;
            _targetNode = targetNode;
            CalculateElectricCharacteristics
                (seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength, length, frequency);
        }

        private void CalculateElectricCharacteristics(double lengthResistance, double lengthInductance, double shuntCapacity,
            double shuntConductance, double length, double frequency)
        {
            var omega = 2*Math.PI*frequency;
            var directLengthImpedance = new Complex(lengthResistance * length, omega * lengthInductance * length);
            var directShuntAdmittance = new Complex(shuntConductance * length, omega * shuntCapacity * length);

            if (directShuntAdmittance == new Complex())
            {
                _hasShuntAdmittance = false;
                _lengthImpedance = directLengthImpedance;
                _shuntAdmittance = new Complex();
            }
            else
            {
                _hasShuntAdmittance = true;
                var waveImpedance = Complex.Sqrt(directLengthImpedance / directShuntAdmittance);
                var angle = Complex.Sqrt(directLengthImpedance * directShuntAdmittance);
                _lengthImpedance = waveImpedance * Complex.Sinh(angle);
                _shuntAdmittance = Complex.Tanh(angle / 2) / waveImpedance;
            }
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

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            return new Complex();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
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
    }
}
