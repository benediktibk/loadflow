namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNet
    {
        void SetNode(int i, INode node);
        IReadOnlyAdmittanceMatrix Admittances { get; }
        double NominalVoltage { get; }
    }
}