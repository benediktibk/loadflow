﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Load : IPowerNetElement
    {
        private readonly Complex _load;
        private readonly IExternalReadOnlyNode _node;

        public Load(Complex load, IExternalReadOnlyNode node)
        {
            _load = load;
            _node = node;
        }

        public Complex Value
        {
            get { return _load; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return scaler.ScalePower(Value);
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {

        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}