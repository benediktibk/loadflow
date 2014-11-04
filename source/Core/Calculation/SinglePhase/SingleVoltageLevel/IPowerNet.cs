namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNet
    {
        void SetNode(int i, Node node);
        IReadOnlyAdmittanceMatrix Admittances { get; }
        double NominalVoltage { get; }
    }
}