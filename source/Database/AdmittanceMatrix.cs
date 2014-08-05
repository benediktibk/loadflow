using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Calculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics;

namespace Database
{
    public static class AdmittanceMatrix
    {
        #region public functions

        public static List<SqlCommand> CreateCommandsToCreateTables()
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

        public static SqlCommand CreateCommandToAddHeader(
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
            commands.AddRange(CreateCommandsToAddNodeNames(nodeNames, matrixId));
            commands.AddRange(CreateCommandsToAddValues(matrix, matrixId));
            return commands;
        }

        #endregion

        #region private functions

        private static IEnumerable<SqlCommand> CreateCommandsToAddNodeNames(IReadOnlyList<string> nodeNames, int matrixId)
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

        private static IEnumerable<SqlCommand> CreateCommandsToAddValues(IAdmittanceMatrix matrix, int matrixId)
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

        #endregion
    }
}
