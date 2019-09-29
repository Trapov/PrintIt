namespace PrintIt
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPrintDispatcher
    {
        double AveragePrintTime { get; }

        IEnumerable<Document> Awaiting { get; }
        IEnumerable<Document> Faulted { get; }
        IEnumerable<Document> Printed { get; }

        Task StartDispatching(CancellationToken cancellationToken);
        void QueuePrintFor(Document document);

        #region Cancellations

        void CancelAll();
        void CancelCurrentPrint();

        #endregion
    }
}
