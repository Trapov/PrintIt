namespace PrintIt
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class PrinterEmulator : IPrinter
    {
        public async Task<bool> TryPrint(Document document, CancellationToken cancellationToken)
        {
            if(cancellationToken.IsCancellationRequested)
                return false;

            await Task.Delay(document.TimeToPrint, cancellationToken).ConfigureAwait(false);
            Console.WriteLine(
                $"Document [{document.DocumentType}] " +
                $"with page size [{document.PageSize}] " +
                $"was printed for [{document.TimeToPrint}]"
            );
            return true;
        }
    }
}
