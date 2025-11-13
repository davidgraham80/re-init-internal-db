using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
