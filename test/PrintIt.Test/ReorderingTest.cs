using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PrintIt.Test
{
    public class ReorderingTest
    {
        public sealed class MockPrinter : IPrinter
        {
            public bool Done { get; set; }

            public void WhenDone()
            {
                while (!Done)
                {

                }
            }

            public Task<bool> TryPrint(Document document, CancellationToken cancellationToken)
            {
                Done = true;
                return Task.FromResult(true);
            }
        }

        [Fact]
        public void CorrectMutationTest()
        {
            var mockPrint = new MockPrinter();
            using var dispatcher = new DefaultPrintDispatcher(mockPrint);
            var documentOne = new Document(
                documentType: "Договор ГПХ",
                pageSize: PageSizes.A2,
                timeToPrint: TimeSpan.FromSeconds(2)
            );

            dispatcher.StartDispatching(CancellationToken.None);

            dispatcher.QueuePrintFor(documentOne);
            mockPrint.WhenDone();

            var printed = dispatcher.Printed;

            Assert.Single(printed);
        }
    }
}
