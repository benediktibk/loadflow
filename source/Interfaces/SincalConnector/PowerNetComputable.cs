using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class PowerNetComputable : PowerNetDatabaseAdapter
    {
        public PowerNetComputable(string database) : base(database)
        { }

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet(calculator);
            
            var nominalPhaseShifts = symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            var slackPhaseShift = Data.ContainsTransformers ? symmetricPowerNet.SlackPhaseShift : new Angle();
            var nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            var nodeResults = symmetricPowerNet.CalculateNodeVoltages();

            if (nodeResults == null)
                return false;

            SetNodeResults(nodeResults);

            using (var connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                var commandFactory = new SqlCommandFactory(connection);
                var deleteCommand = commandFactory.CreateCommandToDeleteAllNodeResults();
                var insertCommands = new List<OleDbCommand> { Capacity = Data.Nodes.Count };
                insertCommands.AddRange(Data.Nodes.Select(node => commandFactory.CreateCommandToAddResult(node, nominalPhaseShiftByIds[node.Id], slackPhaseShift)));

                deleteCommand.ExecuteNonQuery();

                foreach (var command in insertCommands)
                    command.ExecuteNonQuery();

                connection.Close();
            }

            return true;
        }

        private SymmetricPowerNet CreateSymmetricPowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var singlePhasePowerNet = new Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable(Data.Frequency, new PowerNetFactory(nodeVoltageCalculator), new NodeGraph());
            var symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);

            foreach (var node in Data.Nodes)
                node.AddTo(symmetricPowerNet);

            foreach (var element in Data.NetElements)
                element.AddTo(symmetricPowerNet);

            return symmetricPowerNet;
        }
    }
}
