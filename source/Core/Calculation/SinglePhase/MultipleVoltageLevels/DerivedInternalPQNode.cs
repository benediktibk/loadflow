﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        private readonly Complex _power;

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, int id, Complex power, string name) : base(sourceNode, id, name)
        {
            if (Double.IsNaN(power.Magnitude))
                throw new ArgumentOutOfRangeException("power");

            _power = power;
        }

        public override INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new PqNode(scaler.ScalePower(_power));
        }
    }
}
