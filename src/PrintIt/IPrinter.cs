namespace PrintIt
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPrinter
    {
        Task<bool> TryPrint(Document document, CancellationToken cancellationToken);
    }
}
