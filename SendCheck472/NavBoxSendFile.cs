using System;
using System.Collections.Generic;
using System.IO;
using NavBox.Files;

namespace Navtor.Message
{
    [Serializable]
    public class NavBoxFileBase
    {
        #region Path constants
        public const string _pathmask_ENC = "%%ENC";
        public const string _pathmask_softwareDB = "%%softwareDB";
        public const string _pathmask_Commands = "%%Commands";
        public const string _pathmask_Messages = "%%Messages";
        public const string _pathmask_Executables = "%%Executables";
        public const string _pathmask_Config = "%%Config";
        public const string _pathmask_NavBox = "%%NavBox";
        public const string _pathmask_NavStation = "%%NavStation";
        public const string _pathmask_Log = "%%Log";
        public const string _pathmask_Root = "%%Root";
        public const string _pathmask_Routes = "%%Routes";
        public const string _pathmask_PassagePlans = "%%PassagePlans";
        #endregion

        [Serializable]
        public class NavBoxSendFileData
        {
            public string Filename;
            public string TargetPath;
            public byte[] FileData;
        }

        protected NavBoxSendFileData _data;

        public string Filename { get { return _data.Filename; } }
        public string Targetpath { get { return UnpackPath(_data.TargetPath); } }
        public byte[] FileData { get { return _data.FileData; } }

        protected string PackPath(string path)
        {
            Locator loc = new Locator();
            if (path.IndexOf(loc.ENC, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.ENC, _pathmask_ENC);
            if (path.IndexOf(loc.SoftwareDatabases, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.SoftwareDatabases, _pathmask_softwareDB);
            if (path.IndexOf(loc.Commands, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Commands, _pathmask_Commands);
            if (path.IndexOf(loc.Messages, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Messages, _pathmask_Messages);
            if (path.IndexOf(loc.Executables, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Executables, _pathmask_Executables);
            if (path.IndexOf(loc.Config, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Config, _pathmask_Config);
            if (path.IndexOf(loc.NavBox, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.NavBox, _pathmask_NavBox);
            if (path.IndexOf(loc.NavStation, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.NavStation, _pathmask_NavStation);
            if (path.IndexOf(loc.Log, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Log, _pathmask_Log);
            if (path.IndexOf(loc.Root, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Root, _pathmask_Root);
            if (path.IndexOf(loc.Routes, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.Routes, _pathmask_Routes);
            if (path.IndexOf(loc.PassagePlans, System.StringComparison.OrdinalIgnoreCase) != -1)
                return path.Replace(loc.PassagePlans, _pathmask_PassagePlans);
            return path;
        }

        protected string UnpackPath(string path)
        {
            string[] PathParts = path.Split('\\');
            if (PathParts.Length == 0)
                return path;
            Locator loc = new Locator();
            switch (PathParts[0])
            {
                case _pathmask_ENC:
                    return path.Replace(_pathmask_ENC, loc.ENC);
                case _pathmask_softwareDB:
                    return path.Replace(_pathmask_softwareDB, loc.SoftwareDatabases);
                case _pathmask_Commands:
                    return path.Replace(_pathmask_Commands, loc.Commands);
                case _pathmask_Messages:
                    return path.Replace(_pathmask_Messages, loc.Messages);
                case _pathmask_Executables:
                    return path.Replace(_pathmask_Executables, loc.Executables);
                case _pathmask_Config:
                    return path.Replace(_pathmask_Config, loc.Config);
                case _pathmask_NavBox:
                    return path.Replace(_pathmask_NavBox, loc.NavBox);
                case _pathmask_NavStation:
                    return path.Replace(_pathmask_NavStation, loc.NavStation);
                case _pathmask_Log:
                    return path.Replace(_pathmask_Log, loc.Log);
                case _pathmask_Root:
                    return path.Replace(_pathmask_Root, loc.Root);
                case _pathmask_Routes:
                    return path.Replace(_pathmask_Routes, loc.Routes);
                case _pathmask_PassagePlans:
                    return path.Replace(_pathmask_PassagePlans, loc.PassagePlans);
                default:
                    return path;
            }
        }

        public string GetXML() { return Tools.Serializer.SerializeToString(_data); }

        public byte[] GetBinary()
        {
            if (_data.FileData.Length > Tools.Serializer.RAMtoDISKswitchSize)
                return Tools.Serializer.SerializeToBytearrayViaDisk(_data);
            return Tools.Serializer.SerializeToBytearray(_data, _data.FileData.Length + 1024);
        }

        public void FromXML(string xml) { _data = Tools.Serializer.DeserializeFromString<NavBoxSendFileData>(xml); }
        public void FromBinary(byte[] bin) { _data = Tools.Serializer.DeserializeFromBytearray<NavBoxSendFileData>(bin); }
    }

    [Serializable]
    public class NavBoxSendFile : NavBoxFileBase, ImessageData

    {
        public const string ClassName = "NavBoxSendFile";

        public NavBoxSendFile(string path, string name, byte[] data)
        {
            _data = new NavBoxSendFileData() { TargetPath = PackPath(path), Filename = name, FileData = data };
        }

        public NavBoxSendFile() { _data = new NavBoxSendFileData(); }

        // implementation of ImessageData Interface
        public string GetName() { return ClassName; }
    }
}