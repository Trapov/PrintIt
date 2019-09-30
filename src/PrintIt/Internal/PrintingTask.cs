namespace PrintIt
{
    internal sealed class PrintingTask
    {
        internal PrintingTask(Document document)
        {
            Document = document;
            DocumentStatus = DocumentStatuses.Awaiting;
        }

        internal Document Document { get; }

        internal void ToPrinted() => DocumentStatus = DocumentStatuses.Printed;
        internal void ToFaulted() => DocumentStatus = DocumentStatuses.Faulted;

        internal DocumentStatuses DocumentStatus { get; private set; }

    }
}
