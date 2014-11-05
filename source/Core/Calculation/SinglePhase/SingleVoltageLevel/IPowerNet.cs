namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNet
    {
        void AdNode(int i, INode node);
        IReadOnlyAdmittanceMatrix Admittances { get; }
        double NominalVoltage { get; }
    }
}