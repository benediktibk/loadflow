using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Numerics;
using Calculation.ThreePhase;
using Misc;
using MathNet.Numerics;

namespace SincalConnector
{
    public class Node : IReadOnlyNode
    {
        private readonly OleDbConnection _databaseConnection;

        public Node(ISafeDatabaseRecord record, OleDbConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
            var nameFull = record.Parse<string>("Name");
            var nameLength = nameFull.Length;

            while (nameLength > 0 && nameFull[nameLength - 1] == ' ')
                --nameLength;

            Id = record.Parse<int>("Id");
            Name = nameFull.Substring(0, nameLength);
            NominalVoltage = record.Parse<double>("Un")*1000;
            Voltage = new Complex();
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public double NominalVoltage { get; private set; }

        public Complex Voltage { get; private set; }

        public Complex Load { get; private set; }

        public void SetResult(Complex voltage, Complex load, IReadOnlyList<ImpedanceLoad> impedanceLoads)
        {
            var loadByImpedances = new Complex();

            foreach (var impedanceLoad in impedanceLoads)
            {
                var impedance = impedanceLoad.Impedance;
                var loadByImpedance = voltage * (voltage / impedance).Conjugate();
                loadByImpedances = loadByImpedances + loadByImpedance;
            }

            Voltage = voltage;
            Load = load - loadByImpedances;
        }

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddNode(Id, NominalVoltage, Name);
        }
    }
}
