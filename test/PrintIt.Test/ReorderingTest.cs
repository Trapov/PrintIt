using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace PrintIt.Test
{
    public class ReorderingTest
    {
        public sealed class RepeatAttribute : Xunit.Sdk.DataAttribute
        {
            private readonly int count;

            public RepeatAttribute(int count)
            {
                if (count < 1)
                {
                    throw new System.ArgumentOutOfRangeException(
                        paramName: nameof(count),
                        message: "Repeat count must be greater than 0."
                        );
                }
                this.count = count;
            }

            public override System.Collections.Generic.IEnumerable<object[]> GetData(System.Reflection.MethodInfo testMethod)
            {
                foreach (var iterationNumber in Enumerable.Range(start: 1, count: this.count))
                {
                    yield return new object[] { iterationNumber };
                }
            }
        }
        public sealed class MockPrinter : IPrinter
        {
            public bool Done { get; set; }

            public async Task Print(Document document, Action onSuccess, Action onFailure, CancellationToken cancellationToken)
            {
                onSuccess();
                Done = true;
            }

            public void WhenDone()
            {
                while (!Done)
                {

                }
            }
        }

    [Theory(DisplayName = "It should work")]
    [Repeat(250)]
    public void CorrectMutationTest(int itNumber)
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
