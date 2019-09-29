namespace PrintIt
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class DefaultPrintDispatcher : IPrintDispatcher, IDisposable
    {
        public double AveragePrintTime { get; private set; } = 0;
        public IEnumerable<Document> Awaiting => _documentQueue;
        public IEnumerable<Document> Printed => _printed;
        public IEnumerable<Document> Faulted => _faulted;

        private readonly IPrinter _printer;
        private readonly IList<Document> _printed;
        private readonly IList<Document> _faulted;

        private CancellationTokenSource _currentTaskTokenSource;
        
        private BlockingCollection<Document> _documentQueue;

        public DefaultPrintDispatcher(IPrinter printer)
        {
            _printer = printer;

            _documentQueue = new BlockingCollection<Document>();
            _printed = new List<Document>();
            _faulted = new List<Document>();
        }

        public void QueuePrintFor(Document document)
        {
            while (!_documentQueue.TryAdd(document)){}
        }

        public void CancelAll()
        {
            while (_documentQueue.TryTake(out var document))
            {
                _currentTaskTokenSource?.Cancel();
                _faulted.Add(document);
            }
        }

        public async Task StartDispatching(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Document document;
                do
                {
                    await Task.Delay(100, cancellationToken);
                } while (!_documentQueue.TryTake(out document, 100, cancellationToken));

                try
                {
                    var newTaskTokenSource = new CancellationTokenSource();
                    Interlocked.Exchange(ref _currentTaskTokenSource, newTaskTokenSource);

                    await _printer.Print(
                                    document: document,
                                    onFailure: () =>
                                    {
                                        _faulted.Add(document);
                                    },
                                    onSuccess: () =>
                                    {
                                        _printed.Add(document);
                                        AveragePrintTime = _printed.Average(d => d.TimeToPrint.TotalSeconds);
                                    },
                                    cancellationToken: _currentTaskTokenSource.Token);
                }

                catch (TaskCanceledException taskCanc)
                {
                    Console.WriteLine("Task was cancelled");
                    _faulted.Add(document);
                }
            }
        }

        public void CancelCurrentPrint()
        {
            _currentTaskTokenSource?.Cancel();
        }

        public void Dispose()
        {
            _currentTaskTokenSource?.Dispose();
            _documentQueue?.Dispose();
        }
    }
}
