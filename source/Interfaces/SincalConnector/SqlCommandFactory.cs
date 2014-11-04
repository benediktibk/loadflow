using System;
using System.Data.OleDb;
using Misc;

namespace SincalConnector
{
    class SqlCommandFactory
    {
        private readonly OleDbConnection _connection;

        public SqlCommandFactory(OleDbConnection connection)
        {
            _connection = connection;
        }

        public OleDbCommand CreateCommandToFetchAllTwoWindingTransformers()
        {
            return new OleDbCommand("SELECT Element_ID,Sn,uk,ur,Vfe,i0,VecGrp,roh,Un1,Un2,AddRotate FROM TwoWindingTransformer;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllTerminals()
        {
            return new OleDbCommand("SELECT Terminal_ID, Element_ID, Node_ID, Flag_Switch, Flag_Terminal FROM Terminal;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllThreeWindingTransformers()
        {
            return new OleDbCommand("SELECT Element_ID,Sn12,Sn23,Sn31,uk12,uk23,uk31,ur12,ur23,ur31,Vfe,i0,VecGrp1,VecGrp2,VecGrp3,roh1,roh2,roh3,Un1,Un2,Un3,AddRotate1,AddRotate2,AddRotate3 FROM ThreeWindingTransformer;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllTransmissionLines()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_LineTyp,Flag_Ll,l,ParSys,fr,r,x,c,va,fn,Un FROM Line;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllSlackGenerators()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Machine,Un,Flag_Lf,u,Ug,xi,delta FROM SynchronousMachine WHERE Flag_Lf = 3 OR Flag_Lf = 5;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllFrequencies()
        {
            return new OleDbCommand("SELECT f FROM VoltageLevel GROUP BY f;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllElementIdsSorted()
        {
            return new OleDbCommand("SELECT Element_ID FROM Element ORDER BY Element_ID ASC;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllNodeResults()
        {
            return new OleDbCommand("SELECT Node_ID,U,phi_rot,P,Q FROM LFNodeResult;", _connection);
        }

        public OleDbCommand CreateCommandToDeleteAllNodeResults()
        {
            return new OleDbCommand("DELETE FROM LFNodeResult;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllNodeResultTableEntries()
        {
            return new OleDbCommand("SELECT U,U_Un,phi,P,Q,S,Flag_Result,Flag_State,Uph,Uph_Unph,phi_ph,phi_rot,phi_ph_rot FROM LFNodeResult ORDER BY Result_ID;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllNodes()
        {
            return new OleDbCommand("SELECT Node.Node_ID AS Id,Node.Name AS Name,VoltageLevel.Un AS Un FROM Node INNER JOIN VoltageLevel ON VoltageLevel.VoltLevel_ID = Node.VoltLevel_ID;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllLoads()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Lf,P,Q FROM Load WHERE (Flag_LoadType = 2 OR Flag_LoadType = 4) AND Flag_Load = 1;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllImpedanceLoads()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Lf,P,Q,u,Ul FROM Load WHERE Flag_LoadType = 1 AND Flag_Load = 1;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllGenerators()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Machine,Un,Flag_Lf,P,u,Ug,xi,fP FROM SynchronousMachine WHERE Flag_Lf = 6 OR Flag_Lf = 7 OR Flag_Lf = 11 OR Flag_Lf = 12;", _connection);
        }

        public OleDbCommand CreateCommandToFetchAllFeedIns()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Typ,Flag_Lf,delta,u,Ug,xi FROM Infeeder;", _connection);
        }

        public OleDbCommand CreateCommandToAddResult(NodeResult nodeResult, double nominalVoltage, Angle phaseShift, Angle slackPhaseShift)
        {
            var voltagePhase = new Angle(nodeResult.Voltage.Phase);
            var voltagePhaseShifted = voltagePhase - phaseShift;
            var voltagePhaseSlackShifted = voltagePhase - slackPhaseShift;

            var command = new OleDbCommand("INSERT INTO LFNodeResult (Node_ID,Result_ID,Variant_ID,Flag_Result,Flag_State,P,Q,S,U,U_Un,Uph,Uph_Unph,phi,phi_rot,phi_ph,phi_ph_rot) " +
                                           "VALUES (@Node_ID,@Result_ID,@Variant_ID,@Flag_Result,@Flag_State,@P,@Q,@S,@U,@U_Un,@Uph,@Uph_Unph,@phi,@phi_rot,@phi_ph,@phi_ph_rot);",
                                           _connection);
            command.Parameters.AddWithValue("@Node_ID", nodeResult.NodeId);
            command.Parameters.AddWithValue("@Result_ID", nodeResult.NodeId);
            command.Parameters.AddWithValue("@Variant_ID", 1);
            command.Parameters.AddWithValue("@Flag_Result", 0);
            command.Parameters.AddWithValue("@Flag_State", 1);
            command.Parameters.AddWithValue("@P", nodeResult.Power.Real * 1e-6);
            command.Parameters.AddWithValue("@Q", nodeResult.Power.Imaginary * 1e-6);
            command.Parameters.AddWithValue("@S", nodeResult.Power.Magnitude * 1e-6);
            command.Parameters.AddWithValue("@U", nodeResult.Voltage.Magnitude * 1e-3);
            command.Parameters.AddWithValue("@U_Un", nodeResult.Voltage.Magnitude / nominalVoltage * 100);
            command.Parameters.AddWithValue("@Uph", nodeResult.Voltage.Magnitude / Math.Sqrt(3) * 1e-3);
            command.Parameters.AddWithValue("@Uph_Unph", nodeResult.Voltage.Magnitude / nominalVoltage * 100);
            command.Parameters.AddWithValue("@phi", voltagePhaseSlackShifted.DegreeAroundZero);
            command.Parameters.AddWithValue("@phi_rot", voltagePhaseShifted.DegreeAroundZero);
            command.Parameters.AddWithValue("@phi_ph", voltagePhaseSlackShifted.DegreeAroundZero);
            command.Parameters.AddWithValue("@phi_ph_rot", voltagePhaseShifted.DegreeAroundZero);
            return command;
        }
    }
}
