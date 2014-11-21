using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Misc;

namespace Database
{
    public class TransmissionLine : INetElement
    {
        private string _name;
        private Node _nodeOne;
        private Node _nodeTwo;
        private double _seriesResistancePerUnitLength;
        private double _seriesInductancePerUnitLength;
        private double _shuntConductancePerUnitLength;
        private double _shuntCapacityPerUnitLength;
        private double _length;
        private bool _transmissionEquationModel;

        public TransmissionLine()
        {
            SeriesResistancePerUnitLength = 1;
            SeriesInductancePerUnitLength = 0;
            Length = 1;
            ShuntCapacityPerUnitLength = 0;
            ShuntConductancePerUnitLength = 0;
            TransmissionEquationModel = true;
            Name = "";
        }

        public TransmissionLine(IReadOnlyDictionary<int, Node> nodeIds, ISafeDatabaseRecord reader)
        {
            var nodeOneId = reader.Parse<int>("NodeOne");
            var nodeOne = nodeIds[nodeOneId];
            var nodeTwoId = reader.Parse<int>("NodeTwo");
            var nodeTwo = nodeIds[nodeTwoId];
            Id = reader.Parse<int>("LineId");
            Name = reader.Parse<string>("LineName");
            SeriesResistancePerUnitLength = reader.Parse<double>("SeriesResistancePerUnitLength");
            SeriesInductancePerUnitLength = reader.Parse<double>("SeriesInductancePerUnitLength");
            ShuntConductancePerUnitLength = reader.Parse<double>("ShuntConductancePerUnitLength");
            ShuntCapacityPerUnitLength = reader.Parse<double>("ShuntCapacityPerUnitLength");
            Length = reader.Parse<double>("Length");
            TransmissionEquationModel = reader.Parse<int>("TransmissionEquationModel") != 0;
            NodeOne = nodeOne;
            NodeTwo = nodeTwo;
        }

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;

                _name = value;
                NotifyPropertyChanged();
            }
        }

        public Node NodeOne
        {
            get { return _nodeOne; }
            set
            {
                if (_nodeOne == value) return;

                _nodeOne = value;
                NotifyPropertyChanged();
            }
        }

        public Node NodeTwo
        {
            get { return _nodeTwo; }
            set
            {
                if (_nodeTwo == value) return;

                _nodeTwo = value;
                NotifyPropertyChanged();
            }
        }

        public double SeriesResistancePerUnitLength
        {
            get { return _seriesResistancePerUnitLength; }
            set
            {
                if (_seriesResistancePerUnitLength == value) return;

                _seriesResistancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double SeriesInductancePerUnitLength
        {
            get { return _seriesInductancePerUnitLength; }
            set
            {
                if (_seriesInductancePerUnitLength == value) return;

                _seriesInductancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double ShuntConductancePerUnitLength
        {
            get { return _shuntConductancePerUnitLength; }
            set
            {
                if (_shuntConductancePerUnitLength == value) return;

                _shuntConductancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double ShuntCapacityPerUnitLength
        {
            get { return _shuntCapacityPerUnitLength; }
            set
            {
                if (_shuntCapacityPerUnitLength == value) return;

                _shuntCapacityPerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double Length
        {
            get { return _length; }
            set
            {
                if (_length == value) return;

                _length = value;
                NotifyPropertyChanged();
            }
        }

        public bool TransmissionEquationModel
        {
            get { return _transmissionEquationModel; }
            set
            {
                if (_transmissionEquationModel == value) return;

                _transmissionEquationModel = value;
                NotifyPropertyChanged();
            }
        }

        public object NodeOneForeignKey
        {
            get
            {
                if (NodeOne == null)
                    return DBNull.Value;

                return NodeOne.Id;
            }
        }

        public object NodeTwoForeignKey
        {
            get
            {
                if (NodeTwo == null)
                    return DBNull.Value;

                return NodeTwo.Id;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SqlCommand CreateCommandToAddToDatabase(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO transmissionlines (LineName, PowerNet, Length, SeriesResistancePerUnitLength, SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength, TransmissionEquationModel) " +
                    "OUTPUT INSERTED.LineId " +
                    "VALUES(@Name, @PowerNet, @Length, @SeriesResistancePerUnitLength, @SeriesInductancePerUnitLength, @ShuntConductancePerUnitLength, @ShuntCapacityPerUnitLength, @TransmissionEquationModel);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("Length", SqlDbType.Real) { Value = Length });
            command.Parameters.Add(new SqlParameter("SeriesResistancePerUnitLength", SqlDbType.Real) { Value = SeriesResistancePerUnitLength });
            command.Parameters.Add(new SqlParameter("SeriesInductancePerUnitLength", SqlDbType.Real) { Value = SeriesInductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntConductancePerUnitLength", SqlDbType.Real) { Value = ShuntConductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntCapacityPerUnitLength", SqlDbType.Real) { Value = ShuntCapacityPerUnitLength });
            command.Parameters.Add(new SqlParameter("TransmissionEquationModel", SqlDbType.Int) { Value = TransmissionEquationModel ? 1 : 0 });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE transmissionlines SET NodeOne=@NodeOne, NodeTwo=@NodeTwo, LineName=@Name, Length=@Length, SeriesResistancePerUnitLength=@SeriesResistancePerUnitLength, SeriesInductancePerUnitLength=@SeriesInductancePerUnitLength, ShuntConductancePerUnitLength=@ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength=@ShuntCapacityPerUnitLength, TransmissionEquationModel=@TransmissionEquationModel WHERE LineId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("NodeOne", SqlDbType.Int) { Value = NodeOneForeignKey });
            command.Parameters.Add(new SqlParameter("NodeTwo", SqlDbType.Int) { Value = NodeTwoForeignKey });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("Length", SqlDbType.Real) { Value = Length });
            command.Parameters.Add(new SqlParameter("SeriesResistancePerUnitLength", SqlDbType.Real) { Value = SeriesResistancePerUnitLength });
            command.Parameters.Add(new SqlParameter("SeriesInductancePerUnitLength", SqlDbType.Real) { Value = SeriesInductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntConductancePerUnitLength", SqlDbType.Real) { Value = ShuntConductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntCapacityPerUnitLength", SqlDbType.Real) { Value = ShuntCapacityPerUnitLength });
            command.Parameters.Add(new SqlParameter("TransmissionEquationModel", SqlDbType.Int) { Value = TransmissionEquationModel ? 1 : 0 });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM transmissionlines WHERE LineId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            return command;
        }

        public bool UsesNode(Node node)
        {
            return node == NodeOne || node == NodeTwo;
        }
    }
}
