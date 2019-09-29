namespace PrintIt
{
    using System;

    public sealed class Document
    {
        public Document(string documentType, PageSizes pageSize, TimeSpan timeToPrint)
        {
            DocumentType = documentType;
            PageSize = pageSize;
            TimeToPrint = timeToPrint;
        }

        public string DocumentType { get; }
        public PageSizes PageSize { get; }
        public TimeSpan TimeToPrint { get; }
    }
}
