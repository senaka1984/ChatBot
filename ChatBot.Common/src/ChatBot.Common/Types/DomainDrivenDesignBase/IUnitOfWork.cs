
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Common.Domain.Base
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        //https://stackoverflow.com/questions/15067865/how-to-use-the-cancellationtoken-property
        //are we using cancellation tokens?
        //var cancelToken = new CancellationTokenSource();
    }
}