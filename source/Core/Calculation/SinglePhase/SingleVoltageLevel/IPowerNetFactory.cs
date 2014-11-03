namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNetFactory
    {
        IPowerNetComputable Create(IAdmittanceMatrix admittances, double nominalVoltage);
    }
}