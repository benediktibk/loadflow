﻿using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public interface INodeVoltageCalculator
    {
        Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses);
        double GetMaximumPowerError();
    }
}