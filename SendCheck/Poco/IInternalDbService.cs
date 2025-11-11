using SendCheck.ENCSyncClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendCheck.Poco
{
    public interface IInternalDbService
    {
        Task<IReadOnlyList<InternalDBFile>> GetFilesAsync(CancellationToken ct = default);
        Task SendAsync(IEnumerable<InternalDBFile> files, CancellationToken ct = default);
    }

}
