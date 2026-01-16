using System;
using System.Collections.Generic;

namespace SendCheck.Poco
{
    public class CommandParameterData
    {
        public string File { get; set; }
        public int TotalFiles { get; set; }
        public string UrlToFile { get; set; }
        public long ExpectedFileSize { get; set; }
        public int Crc { get; set; }
        public bool Cancel { get; set; }
        
        /// <summary>
        /// Groups of messages will be associated with a run-id-guid
        /// </summary>
        public Guid RunId { get; set; }

        /// <summary>
        /// The first message will send a list of all DAT files along with their expected CRC
        /// Navbox will save this file to the temp folder as a json file
        /// </summary>
        public List<InternalDbFileManifestItem> Manifest { get; set; }
        public bool IsFirstMessage { get; set; }
        
    }

    public sealed class InternalDbFileManifestItem
    {
        public string FileName { get; set; }
        public string UrlToFile { get; set; }
        public long ExpectedFileSize { get; set; }
        public int Crc { get; set; }
        public bool Sent { get; set; }
    }
}
