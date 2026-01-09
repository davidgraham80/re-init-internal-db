using Navtor.Message;
using Newtonsoft.Json;
using SendCheck.ENCSyncClient;
using SendCheck.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        private readonly string navSyncVersion = "4.14.1.1024";

        private void VerifyMessages()
        {
            int countReInitDb = messageList.Count(x => x.GetMessage<NavBoxCommand>().CommandString == "ReInitDb");
            int countCancelReInitDb = messageList.Count(x => x.GetMessage<NavBoxCommand>().CommandString == "CancelReInitDb");

            if (countReInitDb > 0 && countCancelReInitDb > 0)
                throw new ArgumentException("Please do not mix the sending of ReInitDb and CancelReinitDb messages, either one or the other.");

            List<CommandParameterData> parsed = messageList
                                                .Select(m =>
                                                {
                                                    var cmd = m.GetMessage<NavBoxCommand>();
                                                    return JsonConvert.DeserializeObject<CommandParameterData>(cmd.CommandParameter);
                                                }).ToList();

            // totalFiles must match count
            if (parsed.Any(p => p.TotalFiles != countReInitDb))
                throw new ArgumentException("TotalFiles mismatch between JSON and message count.");

            // runId must be present and consistent
            if (parsed.Any(p => p.RunId == Guid.Empty))
                throw new ArgumentException("RunId is Guid.Empty in one or more messages.");
            var runId = parsed.First().RunId;
            if (parsed.Any(p => p.RunId != runId))
                throw new ArgumentException("RunId differs between messages.");

            // first-message rules
            var firstFlags = parsed.Count(p => p.IsFirstMessage);
            if (firstFlags != 1)
                throw new ArgumentException($"Expected exactly 1 first message, found {firstFlags}.");

            var first = parsed.First(p => p.IsFirstMessage);
            if (first.Manifest == null || first.Manifest.Items.Count == 0)
                throw new ArgumentException("First message must include manifest.");


            foreach (NavtorMessage navtorMessage in messageList)
            {
                NavBoxCommand msg = navtorMessage.GetMessage<NavBoxCommand>();
                CommandParameterData data = JsonConvert.DeserializeObject<CommandParameterData>(msg.CommandParameter);

                if (data.TotalFiles != countReInitDb)
                    throw new ArgumentException("the count between the total number of messages and the number specified in the JSON of the msg.CommandParameter.");
            }
        }

        internal async Task<bool> SendViaWcf(IReadOnlyList<InternalDbFileManifestItem> _manifest, string macAddress, List<InternalDBFile> internalFiles, IReadOnlyList<InternalDBFile> filesDetailsFromServer,
                                             Action<int, int> progressCallback = null, bool skipChartUpdate = false)
        {
            try
            {
                CreateNavtorMessageList(_manifest, internalFiles, true, filesDetailsFromServer, skipChartUpdate);

                var svc = new InternalDbService(
                    "http://navserver2.navtor.com/ENCSync.svc",
                    macAddress);

                int total = messageList.Count;
                int sent = 0;

                foreach (NavtorMessage message in messageList)
                {
                    sent++;
                    await Task.Delay(500);
                    bool sendSuccess = await svc.SendAsync(message, macAddress, "david.graham@navtor.com", "", new CancellationToken());

                    if(!sendSuccess)
                        throw new Exception("Could not send files, please ensure you have the correct MAC address");

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

        internal async Task<bool> CancelReinitViaWcf(string macAddress, Action<int, int> progressCallback = null)
        {
            try
            {
                NavBoxCommand command = null;
                NavtorMessage message = null;
                var cmdParameter = JsonConvert.SerializeObject(new
                {
                    cancel = true
                });
                command = new NavBoxCommand("CancelReInitDb", cmdParameter);
                message = new NavtorMessage(command);

                var svc = new InternalDbService(
                    "http://navserver2.navtor.com/ENCSync.svc",
                    macAddress);

                bool cancelSuccess = await svc.SendAsync(message, macAddress, "david.graham@navtor.com", "", new CancellationToken());

                if (!cancelSuccess)
                    throw new Exception("CancelReinitViaWcf error, please ensure you have the correct MAC address");
                
                progressCallback?.Invoke(1, 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CancelReinitViaWcf: Send failed:\n{ex.Message}");
                return false;
            }
            return true;
        }

        private void CreateNavtorMessageList(
    IReadOnlyList<InternalDbFileManifestItem> _manifest, // (not needed anymore, but kept to avoid signature changes)
    List<InternalDBFile> selectedFiles,
    bool skipSend,
    IReadOnlyList<InternalDBFile> _filesDetailsFromServer,
    bool skipChartUpdate = false)
        {
            messageList.Clear();

            if (selectedFiles == null) throw new ArgumentNullException(nameof(selectedFiles));
            if (_filesDetailsFromServer == null) throw new ArgumentNullException(nameof(_filesDetailsFromServer));

            int preferredNumOfFilesToSend = selectedFiles.Count;
            totalFiles = preferredNumOfFilesToSend;

            // Base URL used by NavBox when it downloads individual files.
            const string baseUrl = "https://navstorage.navtor.com/navsyncdbfiles/";

            // Build a quick lookup so we don't keep calling First(...)
            var serverByName = _filesDetailsFromServer
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.FileName))
                .ToDictionary(x => x.FileName, StringComparer.OrdinalIgnoreCase);

            // Validate all selected files exist in server list
            foreach (var f in selectedFiles)
            {
                if (f == null || string.IsNullOrWhiteSpace(f.FileName))
                    continue;

                if (!serverByName.ContainsKey(f.FileName))
                    throw new ArgumentException($"Selected file '{f.FileName}' not present in _filesDetailsFromServer");
            }

            // Build manifest items from the server list (not from "selectedFiles")
            // so NavBox can validate / plan using full knowledge.
            var manifestItems = _filesDetailsFromServer
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.FileName))
                .Select(x => new InternalDbFileManifestItem
                {
                    FileName = x.FileName,
                    UrlToFile = x.Url,          // RELATIVE (e.g. "6N/2025-51/NavSync001.DAT")
                    ExpectedFileSize = x.FileSize,
                    Crc = x.Crc
                })
                .ToList();

            // Envelope (header + items) + serialize once.
            // Uses your ManifestBuilder implementation.
            var envelope = ManifestBuilder.Build(items: manifestItems, baseUrl: baseUrl);
            string manifestJson = JsonConvert.SerializeObject(envelope);

            Guid runId = Guid.NewGuid();

            int iterator = 0;
            foreach (var fileToSend in selectedFiles)
            {
                if (fileToSend == null || string.IsNullOrWhiteSpace(fileToSend.FileName))
                    continue;

                bool isFirstMessage = (iterator == 0);

                var serverItem = serverByName[fileToSend.FileName];

                string cmdParameter = JsonConvert.SerializeObject(new
                {
                    file = serverItem.FileName,
                    urlToFile = baseUrl + serverItem.Url,   // FULL URL for the specific file download
                    expectedFileSize = serverItem.FileSize,
                    crc = serverItem.Crc,

                    totalFiles = totalFiles,
                    skipChartUpdate = skipChartUpdate,
                    isFirstMessage = isFirstMessage,
                    runId = runId,

                    // IMPORTANT: only include on first message
                    manifestJson = isFirstMessage ? manifestJson : null
                });

                c = new NavBoxCommand("ReInitDb", cmdParameter);
                m = new NavtorMessage(c);
                messageList.Add(m);

                iterator++;
            }

            VerifyMessages();
        }


        //private void CreateNavtorMessageList(IReadOnlyList<InternalDbFileManifestItem> _manifest, List<InternalDBFile> selectedFiles, bool skipSend, 
        //                                     IReadOnlyList<InternalDBFile> _filesDetailsFromServer, bool skipChartUpdate = false)
        //{
        //    messageList.Clear();

        //    int preferredNumOfFilesToSend = 0;

        //    if (selectedFiles.Count > 0)
        //        preferredNumOfFilesToSend = selectedFiles.Count;

        //    if (preferredNumOfFilesToSend > _filesDetailsFromServer.Count/* Length*/)
        //        throw new ArgumentException($"ERROR : preferredNumOfFilesToSend file count {preferredNumOfFilesToSend} is greater than the _filesDetailsFromServer file count {_filesDetailsFromServer.Count}");

        //    if (!selectedFiles.Any() && preferredNumOfFilesToSend + 1 > _filesDetailsFromServer.Count)
        //        throw new ArgumentException($"ERROR : If not including the database file then set the count preferredNumOfFilesToSend file count of: {preferredNumOfFilesToSend} to be one less than the _filesDetailsFromServer file count of: {_filesDetailsFromServer.Count}");

        //    totalFiles = preferredNumOfFilesToSend;

        //    int iterator = 0; //when iterator is zero include the extra manifest in the send

        //    bool isFirstMessage = iterator == 0;
        //    Guid runId = Guid.NewGuid();
        //    string manifestText = JsonConvert.SerializeObject(_manifest);

        //    List<InternalDbFileManifestItem> manifest = null;
        //    if (isFirstMessage)
        //    {
        //        //original code
        //        manifest = _filesDetailsFromServer
        //                   .Select(x => new InternalDbFileManifestItem
        //                   {
        //                       FileName = x.FileName,
        //                       UrlToFile = x.Url,
        //                       ExpectedFileSize = x.FileSize,
        //                       Crc = x.Crc
        //                   }).ToList();

        //        var first = new CommandParameterData
        //        {
        //            RunId = runId,
        //            IsFirstMessage = true,
        //            TotalFiles = selectedFiles.Count,

        //            Manifest = ManifestBuilder.BuildEnvelope(
        //                items: manifest,
        //                baseUrl: "https://navstorage.navtor.com/navsyncdbfiles/"
        //            )
        //        };

        //    }

        //    foreach (var fileToSend in selectedFiles)
        //    {
        //        if (string.IsNullOrEmpty(fileToSend.FileName))
        //            continue;

        //        isFirstMessage = iterator == 0;

        //        string cmdParameter = JsonConvert.SerializeObject(new
        //        {
        //            file = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).FileName,
        //            urlToFile = "https://navstorage.navtor.com/navsyncdbfiles/" + _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).Url,
        //            expectedFileSize = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).FileSize,
        //            crc = _filesDetailsFromServer.First(x => x.FileName == fileToSend.FileName).Crc,
        //            totalFiles = totalFiles,
        //            skipChartUpdate = skipChartUpdate,
        //            isFirstMessage = isFirstMessage,
        //            runId = runId,
        //            manifest = isFirstMessage ? manifest : null
        //        });

        //        c = new NavBoxCommand("ReInitDb", cmdParameter);
        //        m = new NavtorMessage(c);

        //        messageList.Add(m);

        //        iterator++;
        //    }
        //    VerifyMessages();
        //}

    }
}
