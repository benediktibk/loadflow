using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Transformer : INetElement
    {
        #region variables

        private string _name;
        private Node _upperSideNode;
        private Node _lowerSideNode;
        private double _nominalPower;
        private double _relativeShortCircuitVoltage;
        private double _copperLosses;
        private double _ironLosses;
        private double _relativeNoLoadCurrent;
        private double _ratio;
        private int _phaseShift;

        #endregion

        #region constructor

        public Transformer()
        {
            NominalPower = 10;
            RelativeShortCircuitVoltage = 0.01;
            CopperLosses = 0.01;
            IronLosses = 0.01;
            RelativeNoLoadCurrent = 0.01;
            Ratio = 1;
            Name = "";
            PhaseShift = 0;
        }

        #endregion

        #region properties

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

        public Node UpperSideNode
        {
            get { return _upperSideNode; }
            set
            {
                if (_upperSideNode == value) return;

                _upperSideNode = value;
                NotifyPropertyChanged();
            }
        }

        public Node LowerSideNode
        {
            get { return _lowerSideNode; }
            set
            {
                if (_lowerSideNode == value) return;

                _lowerSideNode = value;
                NotifyPropertyChanged();
            }
        }

        public double NominalPower
        {
            get { return _nominalPower; }
            set
            {
                if (_nominalPower == value) return;

                _nominalPower = value;
                NotifyPropertyChanged();
            }
        }

        public double RelativeShortCircuitVoltage
        {
            get { return _relativeShortCircuitVoltage; }
            set
            {
                if (_relativeShortCircuitVoltage == value) return;

                _relativeShortCircuitVoltage = value;
                NotifyPropertyChanged();
            }
        }

        public double CopperLosses
        {
            get { return _copperLosses; }
            set
            {
                if (_copperLosses == value) return;

                _copperLosses = value;
                NotifyPropertyChanged();
            }
        }

        public double IronLosses
        {
            get { return _ironLosses; }
            set
            {
                if (_ironLosses == value) return;

                _ironLosses = value;
                NotifyPropertyChanged();
            }
        }

        public double RelativeNoLoadCurrent
        {
            get { return _relativeNoLoadCurrent; }
            set
            {
                if (_relativeNoLoadCurrent == value) return;

                _relativeNoLoadCurrent = value;
                NotifyPropertyChanged();
            }
        }

        public double Ratio
        {
            get { return _ratio; }
            set
            {
                if (_ratio == value) return;

                _ratio = value;
                NotifyPropertyChanged();
            }
        }

        public object UpperSideNodeForeignKey
        {
            get
            {
                if (UpperSideNode == null)
                    return DBNull.Value;

                return UpperSideNode.Id;
            }
        }

        public object LowerSideNodeForeignKey
        {
            get
            {
                if (LowerSideNode == null)
                    return DBNull.Value;

                return LowerSideNode.Id;
            }
        }

        public int PhaseShift
        {
            get { return _phaseShift; }
            set
            {
                if (_phaseShift == value) return;

                _phaseShift = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INetElement

        public SqlCommand CreateCommandToAddToDatabase(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO transformers (TransformerName, PowerNet, NominalPower, RelativeShortCircuitVoltage, CopperLosses, IronLosses, RelativeNoLoadCurrent, Ratio, PhaseShift) " +
                    "OUTPUT INSERTED.TransformerId " +
                    "VALUES(@Name, @PowerNet, @NominalPower, @RelativeShortCircuitVoltage, @CopperLosses, @IronLosses, @RelativeNoLoadCurrent, @Ratio, @PhaseShift);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("NominalPower", SqlDbType.Real) { Value = NominalPower });
            command.Parameters.Add(new SqlParameter("RelativeShortCircuitVoltage", SqlDbType.Real) { Value = RelativeShortCircuitVoltage });
            command.Parameters.Add(new SqlParameter("CopperLosses", SqlDbType.Real) { Value = CopperLosses });
            command.Parameters.Add(new SqlParameter("IronLosses", SqlDbType.Real) { Value = IronLosses });
            command.Parameters.Add(new SqlParameter("RelativeNoLoadCurrent", SqlDbType.Real) { Value = RelativeNoLoadCurrent });
            command.Parameters.Add(new SqlParameter("Ratio", SqlDbType.Real) { Value = Ratio });
            command.Parameters.Add(new SqlParameter("PhaseShift", SqlDbType.Int) { Value = PhaseShift });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE transformers SET " +
                    "UpperSideNode=@UpperSideNode, LowerSideNode=@LowerSideNode, TransformerName=@Name, NominalPower=@NominalPower, " +
                    "RelativeShortCircuitVoltage=@RelativeShortCircuitVoltage, CopperLosses=@CopperLosses, IronLosses=@IronLosses, " +
                    "RelativeNoLoadCurrent=@RelativeNoLoadCurrent, Ratio=@Ratio, PhaseShift=@PhaseShift " +
                    "WHERE TransformerId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("UpperSideNode", SqlDbType.Int) { Value = UpperSideNodeForeignKey });
            command.Parameters.Add(new SqlParameter("LowerSideNode", SqlDbType.Int) { Value = LowerSideNodeForeignKey });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("NominalPower", SqlDbType.Real) { Value = NominalPower });
            command.Parameters.Add(new SqlParameter("RelativeShortCircuitVoltage", SqlDbType.Real) { Value = RelativeShortCircuitVoltage });
            command.Parameters.Add(new SqlParameter("CopperLosses", SqlDbType.Real) { Value = CopperLosses });
            command.Parameters.Add(new SqlParameter("IronLosses", SqlDbType.Real) { Value = IronLosses });
            command.Parameters.Add(new SqlParameter("RelativeNoLoadCurrent", SqlDbType.Real) { Value = RelativeNoLoadCurrent });
            command.Parameters.Add(new SqlParameter("Ratio", SqlDbType.Real) { Value = Ratio });
            command.Parameters.Add(new SqlParameter("PhaseShift", SqlDbType.Int) { Value = PhaseShift });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM transformers WHERE TransformerId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            return command;
        }

        public bool UsesNode(Node node)
        {
            return node == UpperSideNode || node == LowerSideNode;
        }

        #endregion
    }
}
