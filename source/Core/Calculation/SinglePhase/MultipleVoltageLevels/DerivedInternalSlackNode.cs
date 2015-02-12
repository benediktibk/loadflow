using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalSlackNode : DerivedInternalNode
    {
        private readonly Complex _voltage;

        public DerivedInternalSlackNode(IExternalReadOnlyNode sourceNode, int id, Complex voltage, string name) : base(sourceNode, id, name)
        {
            if (Double.IsNaN(voltage.Magnitude))
                throw new ArgumentOutOfRangeException("voltage");

            _voltage = voltage;
        }

        public override INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SlackNode(scaler.ScaleVoltage(_voltage));
        }
    }
}
