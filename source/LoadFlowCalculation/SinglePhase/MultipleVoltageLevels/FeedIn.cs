﻿using System;
using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _node;
        private readonly Complex _voltage;

        public FeedIn(string name, IReadOnlyNode node, Complex voltage)
        {
            _name = name;
            _node = node;
            _voltage = voltage;
        }

        public string Name
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public Complex Voltage
        {
            get { return _voltage; }
        }

        public bool EnforcesSlackBus
        {
            get { return true; }
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
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, 1);
            return scaler.ScaleVoltage(Voltage);
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}