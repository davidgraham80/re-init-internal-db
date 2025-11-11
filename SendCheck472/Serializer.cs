using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Navtor.Tools
{
    public static class Serializer
    {
        public const int RAMtoDISKswitchSize = 100 * 1024 * 1024;

        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString().Replace("\r", "");
            }
        }

        public static T DeserializeFromString<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static T DeserializeFromBytearray<T>(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                BinaryFormatter formatter = new BinaryFormatter { Binder = new CrossAppBinder() };
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static byte[] SerializeToBytearray<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public static byte[] SerializeToBytearray<T>(T obj, int buffer_size)
        {
            byte[] buf = new byte[buffer_size];
            using (MemoryStream stream = new MemoryStream(buf))
            {
                BinaryFormatter formatter = new BinaryFormatter { Binder = new CrossAppBinder() };
                formatter.Serialize(stream, obj);
                buf = stream.ToArray();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return buf;
        }

        public static byte[] SerializeToBytearrayViaDisk<T>(T obj)
        {
            string fn = Path.GetTempFileName();
            using (FileStream stream = new FileStream(fn, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter { Binder = new CrossAppBinder() };
                formatter.Serialize(stream, obj);
            }
            byte[] res = File.ReadAllBytes(fn);
            File.Delete(fn);
            return res;
        }
    }

    #region Cross-module binary serialization
    sealed class CrossAppBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(String.Format("{0}, {1}", typeName, Assembly.GetExecutingAssembly().FullName));
        }
    }
    #endregion
}
