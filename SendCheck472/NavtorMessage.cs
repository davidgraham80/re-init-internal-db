using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip;

namespace Navtor.Message
{
    public interface ImessageData
    {
        string GetName();
        string GetXML();
        byte[] GetBinary();
        void FromXML(string xml);
        void FromBinary(byte[] bin);
    }

    [Serializable]
    public class NavtorMessage
    {
        private const string _XML_tag = "<?xml";
        private MessageData _message;

        [Serializable]
        public class MessageData
        {
            public string messageName;
            public Guid id; //Id of Message
            public Guid parentId; //Id of NavtorMessage triggering this message, may be null if message is start of chain
            public int crc32;
            public DateTime creationTime;
            public string data;
            public byte[] bindata;
            public int mailPacketMaxSize;
        }

        static NavtorMessage() { MailPacketMaxSize = 0; }

        private void Init(ImessageData msg, Guid parentGuid)
        {
            if (msg == null)
                throw new Exception("Illegal object type for NavtorMessage");
            _message = new MessageData
            {
                messageName = msg.GetName(),
                creationTime = DateTime.UtcNow,
                id = Guid.NewGuid(),
                parentId = parentGuid,
                mailPacketMaxSize = MailPacketMaxSize
            };
            if ((_message.data = msg.GetXML()) != null)
                _message.crc32 = CRC32.Compute(_message.data);
            else
            {
                _message.bindata = msg.GetBinary();
                _message.crc32 = CRC32.Compute(_message.bindata);
            }
        }

        private void InitBin(ImessageData msg, Guid parentGuid)
        {
            if (msg == null)
                throw new Exception("Illegal object type for NavtorMessage");
            _message = new MessageData
            {
                messageName = msg.GetName(),
                creationTime = DateTime.UtcNow,
                id = Guid.NewGuid(),
                parentId = parentGuid,
                mailPacketMaxSize = MailPacketMaxSize
            };
            if ((_message.bindata = msg.GetBinary()) != null)
                _message.crc32 = CRC32.Compute(_message.bindata);
            else
            {
                _message.data = msg.GetXML();
                _message.crc32 = CRC32.Compute(_message.data);
            }
        }

        public NavtorMessage()
        {
            _message = new MessageData() { messageName = "EmptyMessage", creationTime = DateTime.UtcNow };
        }

        public NavtorMessage(ImessageData iMsg) : this(iMsg, new Guid(), false) { }
        public NavtorMessage(ImessageData iMsg, Guid id) : this(iMsg, id, false) { }
        public NavtorMessage(ImessageData iMsg, Guid id, bool PreferBinary)
        {
            if (PreferBinary)
                InitBin(iMsg, id);
            else
                Init(iMsg, id);
        }

        protected NavtorMessage(byte[] data)
        {
            string tag = Encoding.UTF8.GetString(data, 0, _XML_tag.Length);
            if (tag == _XML_tag)
                _message = Tools.Serializer.DeserializeFromString<MessageData>(Encoding.UTF8.GetString(data, 0, data.Length));
            else
                _message = Tools.Serializer.DeserializeFromBytearray<MessageData>(data);
        }

        private void SetData(MessageData data) { _message = data; }

        public static NavtorMessage FromByteArray(byte[] data)
        {
            NavtorMessage msg = new NavtorMessage();
            String DataDescription = System.Text.Encoding.UTF8.GetString(data, 100, 37);
            if (DataDescription.IndexOf("NavtorMessage") != -1)
                msg.SetData(Tools.Serializer.DeserializeFromBytearray<MessageData>(data));
            else
            {
                byte[] buf = SevenZipHelper.ExtractBytes(data);
                msg.SetData(Tools.Serializer.DeserializeFromString<MessageData>(Encoding.UTF8.GetString(buf, 0, buf.Length)));
            }
            if (!msg.IsValid)
                throw new Exception("Bad CRC in message " + msg.Name);
            return msg;
        }

        public byte[] ToByteArray()
        {
            byte[] b;
            if (_message.data != null)
                b = SevenZipHelper.CompressBytes(Encoding.ASCII.GetBytes(Tools.Serializer.SerializeToString(_message)));
            else
            {
                if (_message.bindata.Length > Tools.Serializer.RAMtoDISKswitchSize)
                    b = Tools.Serializer.SerializeToBytearrayViaDisk(_message);
                else
                    b = Tools.Serializer.SerializeToBytearray(_message, _message.bindata.Length + 1024);
            }
            GC.Collect();
            return b;
        }

        public static NavtorMessage FromFile(string FilePath)
        {
            if (File.Exists(FilePath))
                return FromByteArray(File.ReadAllBytes(FilePath));
            else
                return null;
        }

        public void ToFile(string FilePath) { File.WriteAllBytes(FilePath, ToByteArray()); }

        public T GetMessage<T>() where T : new()
        {
            T message = new T();
            ImessageData imessage = message as ImessageData;
            if (imessage == null)
                throw new Exception("Invalid object type for GetMessage");
            if (_message.data != null)
                imessage.FromXML(_message.data);
            else
                imessage.FromBinary(_message.bindata);
            return message;
        }

        public static int MailPacketMaxSize { get; set; }
        public string Name { get { return _message.messageName; } }

        public bool IsValid
        {
            get
            {
                if (_message.data != null)
                    return (CRC32.Compute(_message.data) == _message.crc32);
                if (_message.bindata != null)
                    return (CRC32.Compute(_message.bindata) == _message.crc32);
                return false;
            }
        }
        public DateTime CreationTime { get { return _message.creationTime; } }
        public Guid ID { get { return _message.id; } }
        public Guid ParentID { get { return _message.parentId; } }

        public string Description()
        {
            if (Name == "NavBoxCommand")
            {
                NavBoxCommand comm = GetMessage<NavBoxCommand>();
                return "NavBoxCommand " + comm.GetCommandDescription();
            }
            else
                return Name;
        }

        public List<byte[]> ToBinary()
        {
            List<byte[]> bin = new List<byte[]>();
            if (MailPacketMaxSize == 0)
                bin.Add(ToByteArray());
            else
            {
                using (MemoryStream data = new MemoryStream(ToByteArray()))
                {
                    long bytestoRead = data.Length;
                    while (bytestoRead > 0)
                    {
                        byte[] chunk = new byte[bytestoRead > MailPacketMaxSize ? MailPacketMaxSize : bytestoRead];
                        data.Read(chunk, 0, chunk.Length);
                        bin.Add(chunk);
                        bytestoRead -= MailPacketMaxSize;
                    }
                }
            }
            return bin;
        }

        public static NavtorMessage FromBinary(List<byte[]> bin)
        {
            long totalsize = bin.Aggregate<byte[], long>(0, (current, chunk) => current + chunk.Length);
            byte[] mergedBin = new byte[totalsize];
            using (MemoryStream data = new MemoryStream(mergedBin))
                foreach (byte[] chunk in bin)
                    data.Write(chunk, 0, chunk.Length);
            return FromByteArray(mergedBin);
        }
    }
}
