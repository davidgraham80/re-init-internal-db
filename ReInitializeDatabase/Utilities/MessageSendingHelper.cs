using Newtonsoft.Json;
using SendCheck.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Navtor.Message;
using SendCheck.ENCSyncClient;

namespace ReInitializeDatabase.Utilities
{
    internal class MessageSendingHelper
    {
        private static NavBoxCommand c;
        private static NavtorMessage m;
        private List<string> filesToSendToVessel = new List<string>();
        private List<NavtorMessage> messageList = new List<NavtorMessage>();
        private int totalFiles;
        IReadOnlyList<InternalDBFile> _filesDetailsFromServer;


        private void VerifyMessages()
        {
            int countReInitDb = messageList.Count(x => x.GetMessage<NavBoxCommand>().CommandString == "ReInitDb");
            int countCancelReInitDb = messageList.Count(x => x.GetMessage<NavBoxCommand>().CommandString == "CancelReInitDb");

            if (countReInitDb > 0 && countCancelReInitDb > 0)
                throw new ArgumentException("Please do not mix the sending of ReInitDb and CancelReinitDb messages, either one or the other.");

            foreach (NavtorMessage navtorMessage in messageList)
            {
                NavBoxCommand msg = navtorMessage.GetMessage<NavBoxCommand>();
                CommandParameterData data = JsonConvert.DeserializeObject<CommandParameterData>(msg.CommandParameter);

                if (data.TotalFiles != countReInitDb)
                    throw new ArgumentException("the count between the total number of messages and the number specified in the JSON of the msg.CommandParameter.");
            }
        }

        internal async Task<bool> SendViaWcf(string macAddress, List<InternalDBFile> internalFiles, IReadOnlyList<InternalDBFile> filesDetailsFromServer,
                                             Action<int, int> progressCallback = null)
        {
            try
            {
                CreateNavtorMessageList(internalFiles, true, filesDetailsFromServer);

                string navSyncVersion = "4.14.1.1024";

                var svc = new InternalDbService(
                    "http://navserver2.navtor.com/ENCSync.svc",
                    macAddress);

                int total = messageList.Count;
                int sent = 0;

                foreach (NavtorMessage message in messageList)
                {
                    sent++;
                    await Task.Delay(500);
                    //await svc.SendAsync(message, macAddress, "david.graham@navtor.com", "", new CancellationToken());
                    progressCallback?.Invoke(sent, total);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed:\n{ex.Message}");
                return false;
            }
            return true;
        }

        private void CreateNavtorMessageList(List<InternalDBFile> selectedFiles, bool skipSend, IReadOnlyList<InternalDBFile> _filesDetailsFromServer)
        {
            messageList.Clear();

            int preferredNumOfFilesToSend = 0;

            if (selectedFiles.Count > 0)
                preferredNumOfFilesToSend = selectedFiles.Count;

            if (preferredNumOfFilesToSend > _filesDetailsFromServer.Count/* Length*/)
                throw new ArgumentException($"ERROR : preferredNumOfFilesToSend file count {preferredNumOfFilesToSend} is greater than the _filesDetailsFromServer file count {_filesDetailsFromServer.Count}");

            if (!selectedFiles.Any() && preferredNumOfFilesToSend + 1 > _filesDetailsFromServer.Count/* Length*/)
                throw new ArgumentException($"ERROR : If not including the database file then set the count preferredNumOfFilesToSend file count of: {preferredNumOfFilesToSend} to be one less than the _filesDetailsFromServer file count of: {_filesDetailsFromServer.Count}");

            totalFiles = preferredNumOfFilesToSend;

            foreach (var fileToSend in selectedFiles)
            {
                if (string.IsNullOrEmpty(fileToSend.FileName))
                    continue;

                string cmdParameter = JsonConvert.SerializeObject(new
                {
                    file = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).FileName,
                    urlToFile = "https://navstorage.navtor.com/navsyncdbfiles/" + _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).Url,
                    expectedFileSize = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).FileSize,
                    crc = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).Crc,
                    totalFiles = totalFiles,
                });
                c = new NavBoxCommand("ReInitDb", cmdParameter);
                m = new NavtorMessage(c);

                messageList.Add(m);
            }
            VerifyMessages();
        }
    }
}
