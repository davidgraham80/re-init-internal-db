using SendCheck.ENCSyncClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Navtor.Message;

namespace SendCheck.Poco
{
    public interface IInternalDbService
    {
        Task<IReadOnlyList<InternalDBFile>> GetFilesAsync(CancellationToken ct = default);
        Task SendAsync(IEnumerable<InternalDBFile> files, CancellationToken ct = default);
        Task<bool> SendAsync(NavtorMessage m, string navbox_serial_number, string originator, string email, CancellationToken ct = default);

    }

}
