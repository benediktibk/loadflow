using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IReadOnlyNode
    {
        Complex Power { get; }
        Complex Voltage { get; }
        double RealPower { get; }
        double VoltageMagnitude { get; }
        bool VoltageIsKnown { get; }
        bool PowerIsKnown { get; }
        bool VoltageMagnitudeIsKnown { get; }
        bool RealPowerIsKnown { get; }
        bool IsPqBus { get; }
        bool IsPvBus { get; }
        bool IsSlackBus { get; }
        void AddTo(IList<int> indexOfSlackBuses, IList<int> indexOfPqBuses, IList<int> indexOfPvBuses, int index);
        void AddTo(IList<PvBus> pvBuses, int index);
        void AddTo(IList<PqBus> pqBuses, int index);
        void SetVoltageIn(Vector<Complex> voltages, int index);
        void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index);
        void SetPowerIn(Vector<Complex> powers, int index);
        void SetRealPowerIn(Vector<Complex> powers, int index);
    }
}
