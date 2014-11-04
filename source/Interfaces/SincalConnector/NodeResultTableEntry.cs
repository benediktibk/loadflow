using Misc;

namespace SincalConnector
{
    /// <summary>
    /// The properties in this class are not in SI units, but instead follow the documentation of SINCAL.
    /// </summary>
    public class NodeResultTableEntry
    {
        public NodeResultTableEntry(ISafeDatabaseRecord record)
        {
            VoltageMagnitude = record.Parse<double>("U");
            VoltageMagnitudeToNominalVoltage = record.Parse<double>("U_Un");
            VoltagePhase = record.Parse<double>("phi");
            RealPower = record.Parse<double>("P");
            ImaginaryPower = record.Parse<double>("Q");
            PowerMagnitude = record.Parse<double>("S");
            ResultType = record.Parse<int>("Flag_Result");
            ResultState = record.Parse<int>("Flag_State");
            StringVoltageMagnitude = record.Parse<double>("Uph");
            StringVoltageMagnitudeToNominalStringVoltage = record.Parse<double>("Uph_Unph");
            StringVoltagePhase = record.Parse<double>("phi_ph");
            VoltagePhaseWithRotation = record.Parse<double>("phi_rot");
            StringVoltagePhaseWithRotation = record.Parse<double>("phi_ph_rot");
        }

        public double VoltageMagnitude { get; private set; }

        public double VoltageMagnitudeToNominalVoltage { get; private set; }

        public double VoltagePhase { get; private set; }

        public double RealPower { get; private set; }

        public double ImaginaryPower { get; private set; }

        public double PowerMagnitude { get; private set; }

        public int ResultType { get; private set; }

        public int ResultState { get; private set; }

        public double StringVoltageMagnitude { get; private set; }

        public double StringVoltageMagnitudeToNominalStringVoltage { get; private set; }

        public double StringVoltagePhase { get; private set; }

        public double VoltagePhaseWithRotation { get; private set; }

        public double StringVoltagePhaseWithRotation { get; private set; }
    }
}
