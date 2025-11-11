using SendCheck.ENCSyncClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WsFile = SendCheck.ENCSyncClient.InternalDBFile;

namespace SendCheck.Poco
{
    public sealed class InternalDbService : IInternalDbService
    {
        private readonly string _url;
        public InternalDbService(string url) => _url = url;

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
                    string navSyncVersion = "4.14.1.1024"; // TODO: supply real value
                    //string usbSerialNumber = "76AE97654B41"; //MS MARINSCHEK
                    string usbSerialNumber = "12A456798A7D"; //MS DAVID

                    try
                    {
                        WsFile[] files;
                        string producerCode;

                        ReinitializeDatabaseResult result = client.GetInternalDBFiles(
                            navSyncVersion,
                            usbSerialNumber,
                            out files,
                            out producerCode);

                        if (result != ReinitializeDatabaseResult.Succeded || files == null)
                            throw new InvalidOperationException($"GetInternalDBFiles failed: {result}");

                        return (IReadOnlyList<WsFile>)files?.ToList() ?? new List<WsFile>();
                    }
                    catch (FaultException fe)
                    {
                        throw;
                    }
                    catch (CommunicationException ce)
                    {
                        throw;
                    }

                }, ct);
            }
        }
        
        public Task SendAsync(IEnumerable<InternalDBFile> files, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }

}
