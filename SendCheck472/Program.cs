using Navtor.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Navbox.MessageSender;
using SendCheck472.EncSyncService;
using System.Reflection;
using System.ServiceModel;
using Newtonsoft.Json.Linq;

namespace Navbox.MessageSender
{
    public class CommandParameterData
    {
        public string File { get; set; }
        public int TotalFiles { get; set; }
        public string UrlToFile { get; set; }
        public long ExpectedFileSize { get; set; }
        public int Crc { get; set; }
        public bool Cancel { get; set; }
    }

    public class MessageComm
    {

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

        public static bool Send(NavtorMessage m, string navbox_serial_number, string originator, string email)
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
    }
}

namespace SendCheck472
{
    class Program
    {
        private static List<NavtorMessage> messageList = new List<NavtorMessage>();
        private static ENCSyncClient _client;
        //private static string _usbSerialNumber = "12A456798A7D"; //'MS DAVID'
        private static string _usbSerialNumber = "76AE97654B41"; //MS MARINSCHEK
                                                                 
        private static string[] filesToSendToVessel;
        private static int totalFiles;
        private static InternalDBFile[] _filesDetailsFromServer;
        private static NavBoxCommand c;
        private static NavtorMessage m;


        /// <summary>
        /// Get the expected list of NavSync and .DAT database files from the NAVTOR API
        /// </summary>
        private static ReinitializeDatabaseResult PopulateInternalDbList(out InternalDBFile[] filesDetailsFromServer)
        {
            ReinitializeDatabaseResult res = _client.GetInternalDBFiles(out filesDetailsFromServer, out string _,
                                                                        "4.14.1.1024",
                                                                        _usbSerialNumber);
            return (ReinitializeDatabaseResult)res;
        }

        private static void CancelReinitDbOperation(string[] args)
        {
            NavBoxCommand c = null;
            NavtorMessage m = null;
            var cmdParameter = JsonConvert.SerializeObject(new
            {
                cancel = true
            });
            c = new NavBoxCommand("CancelReInitDb", cmdParameter);
            m = new NavtorMessage(c);
            MessageComm.Send(m, args[0], "david.graham@navtor.com", "");
            Environment.Exit( 0 );
        }

        private static void CreateUpdateCharts(string[] args)
        {
            NavBoxCommand c = null;
            NavtorMessage m = null;
            //var cmdParameter = JsonConvert.SerializeObject(new
            //{
            //    "NAVTOR"
            //});
            c = new NavBoxCommand("UpdateCharts", "NAVTOR");
            m = new NavtorMessage(c);
            MessageComm.Send(m, args[0], "david.graham@navtor.com", "");
            Environment.Exit(0);

            string xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<CommandData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <commandString>UpdateCharts</commandString>
  <commandParameter>NAVTOR</commandParameter>
</CommandData>
";
        }

        static void Main(string[] args)
        {
            //it is best to set this to true so that NavBox will download the database also.
            //howver if you do just want to send dat files then this should be ok also
            //however please note that best to have the database and DAT files in sync so I would prefer we set this variable to true
            bool includeDb3FileInTheUpdate = false;

            if (args.Length != 1)
                Console.WriteLine("Usage SendCheck472 <NavBoxSerialNumber>");
            else
            {
                var binding = new BasicHttpBinding();
                var endpoint = new EndpointAddress("http://navserver2.navtor.com/ENCSync.svc");
                _client = new ENCSyncClient(binding, endpoint);

                PopulateInternalDbList( out InternalDBFile[] filesDetailsFromServer);
                _filesDetailsFromServer = filesDetailsFromServer;

                messageList = new List<NavtorMessage>();

                ////uncomment the line below to send a Re-initDB cancellation request, you 
                //CancelReinitDbOperation(args);

                //how many files would you like to send
                int preferredNumOfFilesToSend = 0;
                if( includeDb3FileInTheUpdate )
                    preferredNumOfFilesToSend = _filesDetailsFromServer.Length;
                else
                    preferredNumOfFilesToSend = _filesDetailsFromServer.Length - 1;

                //preferredNumOfFilesToSend = 3;
                var listOfFilesToSend = new List<string>();
                //
                listOfFilesToSend = new List<string>() { "NavSync.db3", 
                    
                    "NavSync000.DAT",
                    "NavSync001.DAT",
                    "NavSync002.DAT",
                    /*
                    "NavSync003.DAT",
                    "NavSync004.DAT",
                    "NavSync005.DAT",
                    "NavSync006.DAT",
                    "NavSync007.DAT",
                    "NavSync008.DAT",
                    "NavSync009.DAT",
                    "NavSync010.DAT",
                    "NavSync011.DAT",
                    "NavSync012.DAT",
                    "NavSync013.DAT",
                    "NavSync014.DAT",
                    "NavSync015.DAT",
                    "NavSync016.DAT",
                    "NavSync017.DAT",
                    "NavSync018.DAT",
                    "NavSync019.DAT",
                    "NavSync020.DAT",
                    "NavSync021.DAT",
                    "NavSync022.DAT",
                    "NavSync023.DAT",
                    "NavSync024.DAT",
                    "NavSync025.DAT",
                    "NavSync026.DAT",
                    "NavSync027.DAT",
                    "NavSync028.DAT",
                    "NavSync029.DAT",
                    "NavSync030.DAT",
                    "NavSync031.DAT",
                    "NavSync032.DAT",
                    "NavSync033.DAT",
                    "NavSync034.DAT",
                    "NavSync035.DAT",
                    "NavSync036.DAT",
                    "NavSync037.DAT",
                    "NavSync038.DAT",
                    "NavSync039.DAT",
                    "NavSync040.DAT",
                    "NavSync041.DAT",
                    "NavSync042.DAT"
                   */
                };

                if (listOfFilesToSend.Count > 0)
                    preferredNumOfFilesToSend = listOfFilesToSend.Count;

                if (preferredNumOfFilesToSend > _filesDetailsFromServer.Length)
                    throw new ArgumentException($"ERROR : preferredNumOfFilesToSend file count {preferredNumOfFilesToSend} is greater than the _filesDetailsFromServer file count {_filesDetailsFromServer.Length}");

                if (!listOfFilesToSend.Any() && !includeDb3FileInTheUpdate && preferredNumOfFilesToSend + 1 > _filesDetailsFromServer.Length)
                    throw new ArgumentException($"ERROR : If not including the database file then set the count preferredNumOfFilesToSend file count of: {preferredNumOfFilesToSend} to be one less than the _filesDetailsFromServer file count of: {_filesDetailsFromServer.Length}");

                int fileCounter = 0;

                if (!_filesDetailsFromServer[0].FileName
                                                                 .EndsWith(
                                                                     "db3", StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new ArgumentException("the first item in the list must be a data files");
                }

                filesToSendToVessel = new string[_filesDetailsFromServer.Length];

                if( listOfFilesToSend.Any() )
                {
                    filesToSendToVessel = _filesDetailsFromServer.Where( f => listOfFilesToSend.Contains( Path.GetFileName( f.FileName ) ) ).Select(x => x.FileName  ).ToArray();
                }
                else
                {
                    for (int index = 0; index < _filesDetailsFromServer.Length; index++)
                    {
                        InternalDBFile se = null;

                        if (fileCounter == preferredNumOfFilesToSend)
                        {
                            break;
                        }

                        if (!listOfFilesToSend.Any() && !includeDb3FileInTheUpdate && index == 0)
                            continue;

                        se = _filesDetailsFromServer[index];
                        filesToSendToVessel[fileCounter] = se.FileName;
                        fileCounter++;
                    }
                }

                totalFiles = preferredNumOfFilesToSend;

                SendToVessel( args, true );
             
                VerifyMessages();

                SendToVessel(args, false);
            }
        }

        private static void SendToVessel( string[] args, bool skipSend )
        {
            foreach (string fileToSend in filesToSendToVessel)
            {
                if( string.IsNullOrEmpty( fileToSend ) )
                    continue;

                string cmdParameter = JsonConvert.SerializeObject(new
                {
                    file = _filesDetailsFromServer.First(x => x.FileName == fileToSend).FileName,
                    urlToFile = "https://navstorage.navtor.com/navsyncdbfiles/" + _filesDetailsFromServer.First(x => x.FileName == fileToSend).Url,
                    expectedFileSize = _filesDetailsFromServer.First(x => x.FileName == fileToSend).FileSize,
                    crc = _filesDetailsFromServer.First(x => x.FileName == fileToSend).Crc,
                    totalFiles = totalFiles,
                });
                c = new NavBoxCommand("ReInitDb", cmdParameter);
                m = new NavtorMessage(c);

                if (skipSend)
                    messageList.Add(m);

                if(!skipSend)
                {
                    //MessageComm.Send(m, args[0], "david.graham@navtor.com", "");
                    MessageComm.Send(m, args[0], "anton.marinschek@navtor.com", "");
                }
            }
        }

        private static void VerifyMessages()
        {
            int countReInitDb = messageList.Count( x => x.GetMessage<NavBoxCommand>().CommandString == "ReInitDb" );
            int countCancelReInitDb = messageList.Count( x => x.GetMessage<NavBoxCommand>().CommandString == "CancelReInitDb" );

            if (countReInitDb > 0 && countCancelReInitDb >0)
                throw new ArgumentException("Please do not mix the sending of ReInitDb and CancelReinitDb messages, either one or the other.");

            foreach (NavtorMessage navtorMessage in messageList)
            {
                NavBoxCommand msg = navtorMessage.GetMessage<NavBoxCommand>();
                CommandParameterData data = JsonConvert.DeserializeObject<CommandParameterData>(msg.CommandParameter);

                if( data.TotalFiles != countReInitDb )
                    throw new ArgumentException( "the count between the total number of messages and the number specified in the JSON of the msg.CommandParameter." );
            }
        }

        //private static void ForLoop()
        //{
        //    int totalDatFiles = 2;

        //    for (int i = 0; i < totalDatFiles; i++)
        //    {
        //        NavBoxCommand c = null;
        //        NavtorMessage m = null;

        //        if (i == 0)
        //        {
        //            var cmdParameter = JsonConvert.SerializeObject(new
        //            {
        //                file = $"NavSync.db3",
        //                urlToFile = "https://navtor.blob.core.windows.net/navsyncdbfiles/6N/2024-49/NavSync.db3",
        //                expectedFileSize = 3466624,
        //                totalFiles = 3
        //            });

        //            c = new NavBoxCommand("ReInitDb", cmdParameter);
        //            m = new NavtorMessage(c);
        //            //Navbox.MessageSender.MessageComm.Send(m, args[0], "david.graham@navtor.com", "");
        //        }

        //        var commandParameter = JsonConvert.SerializeObject(new
        //        {
        //            file = $"NavSync00{i}.DAT",
        //            //totalFiles = totalDatFiles + 1
        //            totalFiles = 3
        //        });

        //        c = new NavBoxCommand("ReInitDb", commandParameter);
        //        m = new NavtorMessage(c);
        //        //Navbox.MessageSender.MessageComm.Send(m, args[0], "david.graham@navtor.com", "");
        //    }
        //}
    }
}

