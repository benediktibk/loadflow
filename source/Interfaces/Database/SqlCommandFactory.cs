using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Calculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics;

namespace Database
{
    public static class SqlCommandFactory
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

        public static SqlCommand CreateCommandToCreateTransmissionLineTable()
        {
            return new SqlCommand(
                "CREATE TABLE transmissionlines " +
                "(LineId INTEGER NOT NULL IDENTITY, NodeOne INTEGER REFERENCES nodes (NodeId), NodeTwo INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "LineName TEXT NOT NULL, SeriesResistancePerUnitLength REAL NOT NULL, SeriesInductancePerUnitLength REAL NOT NULL, ShuntConductancePerUnitLength REAL NOT NULL, " +
                "ShuntCapacityPerUnitLength REAL NOT NULL, Length REAL NOT NULL, TransmissionEquationModel INTEGER NOT NULL, " +
                "PRIMARY KEY(LineId));");
        }

        public static SqlCommand CreateCommandToFetchAllTransmssionLines(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM transmissionlines WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static SqlCommand CreateCommandToCreateTransformerTable()
        {
            return new SqlCommand(
                "CREATE TABLE transformers " +
                "(TransformerId INTEGER NOT NULL IDENTITY, UpperSideNode INTEGER REFERENCES nodes (NodeId), LowerSideNode INTEGER REFERENCES nodes (NodeId), " +
                "PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), TransformerName TEXT NOT NULL, NominalPower REAL NOT NULL, " +
                "RelativeShortCircuitVoltage REAL NOT NULL, CopperLosses REAL NOT NULL, IronLosses REAL NOT NULL, RelativeNoLoadCurrent REAL NOT NULL, Ratio REAL NOT NULL, " +
                "PRIMARY KEY(TransformerId));");
        }

        public static SqlCommand CreateCommandToFetchAllTransformers(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM transformers WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static SqlCommand CreateCommandToCreateNodeTable()
        {
            return new SqlCommand(
                "CREATE TABLE nodes " +
                "(NodeId INTEGER NOT NULL IDENTITY, PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), NodeName TEXT NOT NULL, NominalVoltage REAL NOT NULL, " +
                "NodeVoltageReal REAL NOT NULL, NodeVoltageImaginary REAL NOT NULL, " +
                "PRIMARY KEY(NodeId));");
        }

        public static SqlCommand CreateCommandToFetchAllNodes(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM nodes WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static SqlCommand CreateCommandToCreateLoadTable()
        {
            return new SqlCommand(
                "CREATE TABLE loads " +
                "(LoadId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "LoadName TEXT NOT NULL, LoadReal REAL NOT NULL, LoadImaginary REAL NOT NULL, " +
                "PRIMARY KEY(LoadId));");
        }

        public static SqlCommand CreateCommandToFetchAllLoads(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM loads WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static SqlCommand CreateCommandToCreateGeneratorTable()
        {
            return new SqlCommand(
                "CREATE TABLE generators " +
                "(GeneratorId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "GeneratorName TEXT NOT NULL, VoltageMagnitude REAL NOT NULL, RealPower REAL NOT NULL, " +
                "PRIMARY KEY(GeneratorId));");
        }

        public static SqlCommand CreateCommandToFetchAllGenerators(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM generators WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static SqlCommand CreateCommandToCreateFeedInTable()
        {
            return new SqlCommand(
                "CREATE TABLE feedins " +
                "(FeedInId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "FeedInName TEXT NOT NULL, VoltageReal REAL NOT NULL, VoltageImaginary REAL NOT NULL, ShortCircuitPower REAL NOT NULL, C REAL NOT NULL, RealToImaginary REAL NOT NULL, " +
                "PRIMARY KEY(FeedInId));");
        }

        public static SqlCommand CreateCommandToFetchAllFeedIns(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM feedins WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        public static List<SqlCommand> CreateCommandsToCreateAdmittanceMatrixTables()
        {
            return new List<SqlCommand>
            {
                new SqlCommand(
                    "CREATE TABLE admittances " +
                    "(Id INTEGER NOT NULL IDENTITY, PowerNet TEXT NOT NULL, NodeCount INTEGER NOT NULL, PowerBase REAL NOT NULL, " +
                    "PRIMARY KEY(Id));"),
                new SqlCommand(
                    "CREATE TABLE admittancenodenames " +
                    "(Id INTEGER NOT NULL IDENTITY, Matrix INTEGER NOT NULL REFERENCES admittances (Id), " +
                    "[Index] INTEGER NOT NULL, Name TEXT NOT NULL, " +
                    "PRIMARY KEY(Id));"),
                new SqlCommand(
                    "CREATE TABLE admittancevalues " +
                    "(Id INTEGER NOT NULL IDENTITY, Matrix INTEGER NOT NULL REFERENCES admittances (Id), " +
                    "Row INTEGER NOT NULL, [Column] INTEGER NOT NULL, ValueReal REAL NOT NULL, ValueImaginary REAL NOT NULL, " +
                    "PRIMARY KEY(Id));")
            };
        }

        public static SqlCommand CreateCommandToAddAdmittanceMatrixHeader(
            IAdmittanceMatrix matrix, string powerNet, double powerBase)
        {
            var command = new SqlCommand("INSERT INTO admittances (PowerNet, NodeCount, PowerBase) OUTPUT INSERTED.Id VALUES(@PowerNet, @NodeCount, @PowerBase);");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Text) { Value = powerNet });
            command.Parameters.Add(new SqlParameter("NodeCount", SqlDbType.Int) { Value = matrix.NodeCount });
            command.Parameters.Add(new SqlParameter("PowerBase", SqlDbType.Real) { Value = powerBase });
            return command;
        }

        public static List<SqlCommand> CreateCommandsToAddContent(IAdmittanceMatrix matrix,
            IReadOnlyList<string> nodeNames, int matrixId)
        {
            var commands = new List<SqlCommand>();
            commands.AddRange(CreateCommandsToAddAdmittanceNodeNames(nodeNames, matrixId));
            commands.AddRange(CreateCommandsToAddAdmittanceValues(matrix, matrixId));
            return commands;
        }

        private static IEnumerable<SqlCommand> CreateCommandsToAddAdmittanceNodeNames(IReadOnlyList<string> nodeNames, int matrixId)
        {
            var commands = new List<SqlCommand>();

            for (var i = 0; i < nodeNames.Count; ++i)
            {
                var command = new SqlCommand("INSERT INTO admittancenodenames (Matrix, [Index], Name) VALUES(@Matrix, @Index, @Name);");
                command.Parameters.Add(new SqlParameter("Matrix", SqlDbType.Int) { Value = matrixId });
                command.Parameters.Add(new SqlParameter("Index", SqlDbType.Int) { Value = i });
                command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = nodeNames[i] });
                commands.Add(command);
            }

            return commands;
        }

        private static IEnumerable<SqlCommand> CreateCommandsToAddAdmittanceValues(IAdmittanceMatrix matrix, int matrixId)
        {
            var commands = new List<SqlCommand>();

            for (var row = 0; row < matrix.NodeCount; ++row)
                for (var column = 0; column < matrix.NodeCount; ++column)
                {
                    var value = matrix[row, column];

                    if (value.MagnitudeSquared() == 0)
                        continue;

                    var command = new SqlCommand("INSERT INTO admittancevalues (Matrix, Row, [Column], ValueReal, ValueImaginary) VALUES(@Matrix, @Row, @Column, @ValueReal, @ValueImaginary);");
                    command.Parameters.Add(new SqlParameter("Matrix", SqlDbType.Int) { Value = matrixId });
                    command.Parameters.Add(new SqlParameter("Row", SqlDbType.Int) { Value = row });
                    command.Parameters.Add(new SqlParameter("Column", SqlDbType.Int) { Value = column });
                    command.Parameters.Add(new SqlParameter("ValueReal", SqlDbType.Real) { Value = value.Real });
                    command.Parameters.Add(new SqlParameter("ValueImaginary", SqlDbType.Real) { Value = value.Imaginary });
                    commands.Add(command);
                }

            return commands;
        }
    }
}
