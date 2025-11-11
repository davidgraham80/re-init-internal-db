using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SendCheck.Poco
{
    public static class EncSyncClientFactory
    {
        public static ENCSyncClient.ENCSyncClient Create(string url)
        {
            var binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 64 * 1024 * 1024,
                ReaderQuotas = { MaxArrayLength = int.MaxValue }
            };

            var address = new EndpointAddress(url);
            return new ENCSyncClient.ENCSyncClient(binding, address);
        }
    }
}
