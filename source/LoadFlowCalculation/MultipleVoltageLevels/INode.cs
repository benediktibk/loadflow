namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface INode : IPowerNetElement
    {
        double NominalVoltage { get; }
        string Name { get; }
    }
}
