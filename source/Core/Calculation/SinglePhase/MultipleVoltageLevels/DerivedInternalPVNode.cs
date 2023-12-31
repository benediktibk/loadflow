﻿using System;
using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPVNode : DerivedInternalNode
    {
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        public DerivedInternalPVNode(IExternalReadOnlyNode sourceNode, int id, double voltageMagnitude, double realPower, string name) : base(sourceNode, id, name)
        {
            if (Double.IsNaN(voltageMagnitude))
                throw new ArgumentOutOfRangeException("voltageMagnitude");

            if (Double.IsNaN(realPower))
                throw new ArgumentOutOfRangeException("realPower");

            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        public override INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new PvNode(scaler.ScalePower(_realPower), scaler.ScaleVoltage(_voltageMagnitude));
        }
    }
}
