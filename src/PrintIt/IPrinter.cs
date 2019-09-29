namespace PrintIt
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPrinter
    {
        Task Print(Document document, Action onSuccess, Action onFailure, CancellationToken cancellationToken);
    }
}
