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
        public double AveragePrintTime => Printed.DefaultIfEmpty().Average(d => d != null ? d.TimeToPrint.TotalSeconds : 0);
        public IEnumerable<Document> Awaiting => _printingTaskQueue.Select(pt => pt.Document);
        public IEnumerable<Document> Printed => _doneDocuments.Where(pt => pt.DocumentStatus == DocumentStatuses.Printed).Select(pt => pt.Document);
        public IEnumerable<Document> Faulted => _doneDocuments.Where(pt => pt.DocumentStatus == DocumentStatuses.Faulted).Select(pt => pt.Document);

        private const string ThreadTaskMetricsFormat = "ThreadId[{0}], TaskId[{1}] -> \n {2}";
        
        private readonly ConcurrentQueue<PrintingTask> _printingTaskQueue;
        private readonly ConcurrentBag<PrintingTask> _doneDocuments;
        private readonly IPrinter _printer;

        private CancellationTokenSource _currentTaskTokenSource;

        public DefaultPrintDispatcher(IPrinter printer)
        {
            _printer = printer;
            _printingTaskQueue = new ConcurrentQueue<PrintingTask>();
            _doneDocuments = new ConcurrentBag<PrintingTask>();

            _currentTaskTokenSource = new CancellationTokenSource();
        }

        public async Task StartDispatching(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                PrintingTask taskToPrint;
                do
                {
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                } while (!_printingTaskQueue.TryDequeue(out taskToPrint));

                try
                {
                    if (_currentTaskTokenSource.IsCancellationRequested)
                    {
                        var newTaskTokenSource = new CancellationTokenSource();
                        Interlocked.Exchange(ref _currentTaskTokenSource, newTaskTokenSource);
                    }

                    await _printer.Print(
                        document: taskToPrint.Document,
                        onSuccess: () => 
                        {
                            taskToPrint.ToPrinted();
                            _doneDocuments.Add(taskToPrint);
                        },
                        onFailure: () =>
                        {
                            taskToPrint.ToPrinted();
                            _doneDocuments.Add(taskToPrint);
                        },
                        cancellationToken: _currentTaskTokenSource.Token
                    ).ConfigureAwait(false);
                }

                catch (TaskCanceledException taskCanc)
                {
                    taskToPrint.ToFaulted();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write("Failed -> ");
                    Console.ResetColor();

                    Console.Error.WriteLine(ThreadTaskMetricsFormat, Thread.CurrentThread.ManagedThreadId, Task.CurrentId, taskCanc.Message);
                }
            }
        }
        
        public void QueuePrintFor(Document document) => _printingTaskQueue.Enqueue(new PrintingTask(document));

        public void CancelCurrentPrint() => _currentTaskTokenSource?.Cancel();
        public void CancelAll()
        {
            while (_printingTaskQueue.TryDequeue(out var printingTask))
            {
                _currentTaskTokenSource?.Cancel();
                printingTask.ToFaulted();
                _doneDocuments.Add(printingTask);
            }
        }

        public void Dispose()
        {
            _currentTaskTokenSource?.Dispose();
            _printingTaskQueue?.Clear();
        }
    }
}
