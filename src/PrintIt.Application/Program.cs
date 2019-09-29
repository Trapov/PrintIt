using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrintIt.Application
{
    public static class Program
    {
        public static void Main()
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

            Console.WriteLine("M -> Metrics");

            Console.WriteLine("A -> Add #1 Document");
            Console.WriteLine("S -> Add #2 Document");

            Console.WriteLine("C -> Cancel current print");
            Console.WriteLine("V -> Cancel all");

            Console.WriteLine("Escape -> to exit");

            var source = new CancellationTokenSource();

            var dispatchingTask = dispatcher.StartDispatching(source.Token);

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

                    Console.WriteLine($"AvgTime: [{avgTime}]");

                    Console.WriteLine($"Dispatching Task: [{dispatchingTask.Status}]");

                    Console.WriteLine($"Printed: [{dispatcher.Printed.Count()}]");
                    Console.WriteLine($"Faulted: [{dispatcher.Faulted.Count()}]");
                    Console.WriteLine($"Awaiting: [{dispatcher.Awaiting.Count()}]");
                }

                if (key.Key == ConsoleKey.C)
                    dispatcher.CancelCurrentPrint();

                if (key.Key == ConsoleKey.V)
                    dispatcher.CancelAll();
            }
            
            source.Cancel();
        }
    }
}
