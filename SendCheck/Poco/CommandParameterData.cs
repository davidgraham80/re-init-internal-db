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

        ///// <summary>
        ///// The first message will send a list of all DAT files along with their expected CRC
        ///// Navbox will save this file to the temp folder as a json file
        ///// </summary>
        //public List<InternalDbFileManifestItem> Manifest { get; set; }
        public bool IsFirstMessage { get; set; }

        public ManifestEnvelope Manifest { get; set; }
    }

    public sealed class ManifestEnvelope
    {
        public ManifestHeader Header { get; set; }
        public List<InternalDbFileManifestItem> Items { get; set; }
    }

    public sealed class ManifestHeader
    {
        public int SchemaVersion { get; set; } = 1;
        public DateTime GeneratedUtc { get; set; }

        // If item.UrlToFile is relative (like "6N/2025-51/NavSync001.DAT")
        public string BaseUrl { get; set; }

        public int ItemCount { get; set; }
        public int DatCount { get; set; }
        public int? MaxDatIndex { get; set; }

        // Integrity of Items (canonicalized)
        public string ItemsSha256 { get; set; }
    }

    public sealed class InternalDbFileManifestItem
    {
        public string FileName { get; set; }
        public string UrlToFile { get; set; }
        public long ExpectedFileSize { get; set; }
        public int Crc { get; set; }
    }
}
