using Navtor.Message;
using SendCheck.ENCSyncClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WsFile = SendCheck.ENCSyncClient.InternalDBFile;

namespace SendCheck.Poco
{
    public sealed class InternalDbService : IInternalDbService
    {
        private readonly string _url;
        private readonly string _usbSerialNumber;
        public InternalDbService(string url, string usbSerialNumber)
        {
            _url = url;
            _usbSerialNumber = usbSerialNumber;
        }

        public async Task<IReadOnlyList<WsFile>> GetFilesAsync(CancellationToken ct = default)
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 64 * 1024 * 1024,
                ReaderQuotas = { MaxArrayLength = int.MaxValue },
                Security = { Mode = BasicHttpSecurityMode.None} //http
            };

            using (var client = new ENCSyncClient.ENCSyncClient(binding, new EndpointAddress(_url)))
            {
                return await Task.Run(() =>
                {
                    // inputs
                    string navSyncVersion = "4.14.1.1024";

                    try
                    {
                        WsFile[] files;
                        string producerCode;

                        ReinitializeDatabaseResult result = client.GetInternalDBFiles(
                            navSyncVersion,
                            _usbSerialNumber,
                            out files,
                            out producerCode);

                        if (result != ReinitializeDatabaseResult.Succeded || files == null || files.Length == 0)
                            return (IReadOnlyList<WsFile>)Array.Empty<WsFile>();
                            //throw new InvalidOperationException($"GetInternalDBFiles failed: {result}");

                        return (IReadOnlyList<WsFile>)files?.ToList() ?? new List<WsFile>();
                    }
                    catch (FaultException fe)
                    {
                        return new List<WsFile>();
                    }
                    catch (CommunicationException ce)
                    {
                        return new List<WsFile>();
                    }

                }, ct);
            }
        }
        
        public Task SendAsync(IEnumerable<InternalDBFile> files, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendAsync(NavtorMessage m, string navbox_serial_number, string originator, string email, CancellationToken ct = default)
        {
            if (String.IsNullOrEmpty(navbox_serial_number))
                throw new Exception("Missing navbox_serial_number");
            if (String.IsNullOrEmpty(originator))
                throw new Exception("Missing originator");
            byte[] message = m.ToByteArray();
            WebRequest request2 = WebRequest.Create("https://navserver2.navtor.com/NavTorMessagesR.svc/SendMessageToNavBox/" + navbox_serial_number + ";" + originator + ";" + email);
            request2.Method = "POST";
            request2.ContentLength = message.Length;
            Stream serverStream = request2.GetRequestStream();
            serverStream.Write(message, 0, message.Length);
            serverStream.Close();
            using (HttpWebResponse response = request2.GetResponse() as HttpWebResponse)
            {
                int statusCode = (int)response.StatusCode;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string xml = reader.ReadToEnd();
                int retcode = DeserializeFromString<int>(RemoveAllXmlNamespace(xml));
                return retcode == 1;
            }
        }

        private static T DeserializeFromString<T>(string xml)
        {
            xml = RemoveAllXmlNamespace(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private static string RemoveAllXmlNamespace(string xmlData)
        {
            string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            MatchCollection matchCol = Regex.Matches(xmlData, xmlnsPattern);

            foreach (Match m in matchCol)
            {
                xmlData = xmlData.Replace(m.ToString(), "");
            }
            return xmlData;
        }
    }

}
