﻿using System.IO;
using Misc;

namespace SincalConnector
{
    public class Terminal
    {
        public Terminal(ISafeDatabaseRecord record)
        {
            var connectionType = record.Parse<int>("Flag_Terminal");
            var physicalSwitch = record.Parse<int>("Flag_Switch");
            var switchState = record.Parse<int>("Flag_State");

            if (connectionType != 7)
                throw new InvalidDataException(("only three phase nets are supported"));

            Closed = physicalSwitch == 0 || switchState == 1;
            Id = record.Parse<int>("Terminal_ID");
            ElementId = record.Parse<int>("Element_ID");
            NodeId = record.Parse<int>("Node_ID");
        }

        public int Id { get; private set; }

        public int ElementId { get; private set; }

        public int NodeId { get; private set; }

        public bool Closed { get; private set; }
    }
}
