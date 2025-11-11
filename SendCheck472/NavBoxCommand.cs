using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Navtor.Message;
using System.Xml.Serialization;
using System.IO;

namespace Navtor.Message
{
    [Serializable]
    public class NavBoxCommand : ImessageData
    {
        public const string ClassName = "NavBoxCommand";

        private CommandData _commandData;

        public string CommandString { get { return _commandData.commandString; } }
        public string CommandParameter { get { return _commandData.commandParameter; } }

        [Serializable]
        public class CommandData
        {
            public string commandString;
            public string commandParameter;
        }

        public NavBoxCommand() { _commandData = new CommandData(); }

        public NavBoxCommand(string command, string commandParameter)
        {
            _commandData = new CommandData
            {
                commandString = command,
                commandParameter = commandParameter
            };
        }

        public string GetCommandDescription()
        {
            if (String.IsNullOrEmpty(CommandParameter))
                return CommandString;
            return String.Format("{0} {1}", CommandString, CommandParameter);
        }

        // implementation of ImessageData Interface
        public string GetName() { return ClassName; }
        public string GetXML() { return Tools.Serializer.SerializeToString(_commandData); }
        public byte[] GetBinary() { return null; }
        public void FromXML(string xml) { _commandData = Tools.Serializer.DeserializeFromString<CommandData>(xml); }
        public void FromBinary(byte[] bin) { _commandData = Tools.Serializer.DeserializeFromBytearray<CommandData>(bin); }
    }
}
