using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        #region variables

        private readonly Complex _power;

        #endregion

        #region constructor

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, int id, Complex power, string name) : base(sourceNode, id, name)
        {
            _power = power;
        }

        #endregion

        #region protected functions

        protected override SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node() {Power = scaler.ScalePower(_power)};
        }

        #endregion
    }
}
