namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNet
    {
        void AddNode(INode node);
        IReadOnlyAdmittanceMatrix Admittances { get; }
        double NominalVoltage { get; }
    }
}