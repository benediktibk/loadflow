using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Database
{
    public class SqlCommandFactory
    {
        public static SqlCommand CreateCommandToFetchAllPowerNets()
        {
            return new SqlCommand("SELECT * FROM powernets;");
        }

        public static SqlCommand CreateCommandToCreateTableForPowerNets()
        {
            return new SqlCommand(
                "CREATE TABLE powernets " +
                "(PowerNetId INTEGER NOT NULL IDENTITY, Frequency REAL NOT NULL, PowerNetName TEXT NOT NULL, CalculatorSelection INTEGER NOT NULL, " +
                "PRIMARY KEY(PowerNetId));");
        }

        public static SqlCommand CreateCommandToAddToDatabase(IReadOnlyPowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO powernets (PowerNetName, Frequency, CalculatorSelection) OUTPUT INSERTED.PowerNetId VALUES(@Name, @Frequency, @CalculatorSelection);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = powerNet.Name });
            command.Parameters.Add(new SqlParameter("Frequency", SqlDbType.Real) { Value = powerNet.Frequency });
            command.Parameters.Add(new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = powerNet.CalculatorSelection });
            return command;
        }

        public static SqlCommand CreateCommandToUpdateInDatabase(IReadOnlyPowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "UPDATE powernets SET PowerNetName=@Name, Frequency=@Frequency, CalculatorSelection=@CalculatorSelection WHERE PowerNetId=@Id;");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = powerNet.Name });
            command.Parameters.Add(new SqlParameter("Frequency", SqlDbType.Real) { Value = powerNet.Frequency });
            command.Parameters.Add(new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = powerNet.CalculatorSelection });
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            return command;
        }

        public static IEnumerable<SqlCommand> CreateCommandsToRemoveFromDatabase(IReadOnlyPowerNet powerNet)
        {
            var deletePowerNetCommand = new SqlCommand("DELETE FROM powernets WHERE PowerNetId=@Id;");
            var deleteNodesCommand = new SqlCommand("DELETE FROM nodes WHERE PowerNet=@Id;");
            var deleteLoadsCommand = new SqlCommand("DELETE FROM loads WHERE PowerNet=@Id;");
            var deleteFeedInsCommand = new SqlCommand("DELETE FROM feedins WHERE PowerNet=@Id;");
            var deleteGeneratorsCommand = new SqlCommand("DELETE FROM generators WHERE PowerNet=@Id;");
            var deleteTransformersCommand = new SqlCommand("DELETE FROM transformers WHERE PowerNet=@Id;");
            var deleteLinesCommand = new SqlCommand("DELETE FROM lines WHERE PowerNet=@Id;");
            var commands = new List<SqlCommand>
            {
                deleteFeedInsCommand,
                deleteGeneratorsCommand,
                deleteTransformersCommand,
                deleteLinesCommand,
                deleteLoadsCommand,
                deleteNodesCommand,
                deletePowerNetCommand
            };

            foreach (var command in commands)
                command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });

            return commands;
        }
    }
}
