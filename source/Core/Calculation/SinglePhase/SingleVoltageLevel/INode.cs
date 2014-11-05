using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface INode
    {
        void AddTo(IList<NodeWithIndex> slackNodes, IList<NodeWithIndex> pqNodes, IList<NodeWithIndex> pvNodes, int index);
        void AddTo(IList<PvNodeWithIndex> pvBuses, int index);
        void AddTo(IList<PqNodeWithIndex> pqBuses, int index);
        void SetVoltageIn(Vector<Complex> voltages, int index);
        void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index);
        void SetPowerIn(Vector<Complex> powers, int index);
        void SetRealPowerIn(Vector<Complex> powers, int index);
    }
}
