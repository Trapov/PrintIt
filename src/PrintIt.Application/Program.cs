using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrintIt.Application
{
    public static class Program
    {
        public static async Task Main()
        {
            var printer = new PrinterEmulator();
            using var dispatcher = new DefaultPrintDispatcher(printer);

            var documentOne = new Document(
                documentType: "Договор ГПХ",
                pageSize: PageSizes.A2,
                timeToPrint: TimeSpan.FromSeconds(2)
            );

            var documentSecond = new Document(
                documentType: "Договор ГД",
                pageSize: PageSizes.A0,
                timeToPrint: TimeSpan.FromSeconds(1)
            );


            var key = new ConsoleKeyInfo();

            Console.Out.WriteLine("----------------------------------");
            Console.Out.WriteLine("M -> Metrics");
            Console.Out.WriteLine("A -> Add #1 Document");
            Console.Out.WriteLine("S -> Add #2 Document");
            Console.Out.WriteLine("C -> Cancel current print");
            Console.Out.WriteLine("V -> Cancel all");
            Console.Out.WriteLine("Escape -> to exit");
            Console.Out.WriteLine("----------------------------------");

            var source = new CancellationTokenSource();
            var dispatchingTask = dispatcher.StartDispatching(source.Token);

            try
            {
                while (key.Key != ConsoleKey.Escape)
                {
                    key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.A)
                        dispatcher.QueuePrintFor(documentOne);
                    if (key.Key == ConsoleKey.S)
                        dispatcher.QueuePrintFor(documentSecond);

                    if (key.Key == ConsoleKey.M)
                    {
                        var avgTime = dispatcher.AveragePrintTime;
                        var stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine("----------------------------------");

                        stringBuilder.AppendLine($"AvgTime: [{avgTime}]");
                        stringBuilder.AppendLine($"Dispatching Task: [{dispatchingTask.Status}]");
                        stringBuilder.AppendLine($"Printed: [{dispatcher.Printed.Count()}]");
                        stringBuilder.AppendLine($"Faulted: [{dispatcher.Faulted.Count()}]");
                        stringBuilder.AppendLine($"Awaiting: [{dispatcher.Awaiting.Count()}]");
                        stringBuilder.AppendLine("----------------------------------");

                        Console.Out.WriteLine(stringBuilder);
                    }

                    if (key.Key == ConsoleKey.C)
                        dispatcher.CancelCurrentPrint();

                    if (key.Key == ConsoleKey.V)
                        dispatcher.CancelAll();
                }

                Console.Out.WriteLine("Trying to exit gracefully...");
                source.Cancel();
                await dispatchingTask;
            } 
            catch (TaskCanceledException taskException)
            {
                Console.Out.WriteLine("... and done!");
            }
        }
    }
}
