using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalSlackNode : DerivedInternalNode
    {
        #region variables

        private readonly Complex _voltage;

        #endregion

        #region constructor

        public DerivedInternalSlackNode(IExternalReadOnlyNode sourceNode, int id, Complex voltage) : base(sourceNode, id)
        {
            _voltage = voltage;
        }

        #endregion

        #region protected functions

        protected override SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node() {Voltage = scaler.ScaleVoltage(_voltage)};
        }

        #endregion
    }
}
