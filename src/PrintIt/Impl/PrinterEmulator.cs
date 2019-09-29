namespace PrintIt
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class PrinterEmulator : IPrinter
    {
        public async Task Print(Document document, Action onSuccess, Action onFailure, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                onFailure();
                return;
            }

            await Task.Delay(document.TimeToPrint, cancellationToken).ConfigureAwait(false);
            Console.WriteLine(
                $"Document [{document.DocumentType}] " +
                $"with page size [{document.PageSize}] " +
                $"was printed for [{document.TimeToPrint}]"
            );
            onSuccess();
        }
    }
}
