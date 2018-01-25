using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Telegram.Bot.Framework;
using Telegram.Bot.Requests;
#pragma warning disable 4014

namespace EchoBot.Net45Console
{
    class Program
    {
        static readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

        static void Main()
        {
            try
            {
                MainAsync().GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                if (!CancellationSource.IsCancellationRequested)
                    throw;
            }
        }

        static async Task MainAsync()
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("Press Enter to stop execution..." + Environment.NewLine);
            Console.ResetColor();

            Console.WriteLine("Setting up...");

            var cancellation = CancellationSource.Token;

            var containerBuilder = new ContainerBuilder();

            Console.WriteLine("Registering services with IoC container...");
            containerBuilder
                .RegisterType<TimeService>()
                .AsSelf();
            containerBuilder
                .RegisterType<TextEchoHandler>()
                .AsSelf();
            containerBuilder
                .Register(c => new BotOptions<EchoBot> { ApiToken = "token" });
            containerBuilder
                .RegisterType<EchoBot>()
                .AsSelf();
            containerBuilder
                .Register(context =>
                {
                    var echoHandler = context.Resolve<TextEchoHandler>();
                    return new UpdateHanldersProvider<EchoBot>(
                        () => new SayHelloHandler(),
                        () => echoHandler,
                        () => new GameOneHandler()
                    );
                })
                .As<IUpdateHandlersProvider<EchoBot>>();
            containerBuilder
                .RegisterType<UpdateManager<EchoBot>>()
                .As<IUpdateManager<EchoBot>>();

            var container = containerBuilder.Build();

            Task.Run(() =>
            {
                Console.ReadLine();
                Console.WriteLine("Stopping...");
                CancellationSource.Cancel();
            }, cancellation);

            using (var scope = container.BeginLifetimeScope())
            {
                var mgr = scope.Resolve<IUpdateManager<EchoBot>>();
                await mgr.InitAsync(cancellation);

                while (!cancellation.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} Long polling bot updates...");
                    await mgr.GetHandleUpdatesAsync(new GetUpdatesRequest { Timeout = 50 }, cancellation);

                    await Task.Delay(1_000, cancellation);
                }
            }
        }

        /*
        private static void CreateManagerManually()
        {
            var bot = new EchoBot(new BotOptions<EchoBot> { ApiToken = "token" });

            // setup handlers in ctor:
            var upHandlersAccessor = new UpdateHanldersProvider<EchoBot>(
                () => new TextEchoHandler(new TimeService()),
                () => new GameOneHandler()
            );
            // or add one by one:
            upHandlersAccessor.AddHandler(() => new SayHelloHandler());

            var handlersProvider = new UpdateHanldersProvider<EchoBot>(upHandlersAccessor);
            var mgr = new UpdateManager<EchoBot>(bot, handlersProvider);

            mgr.GetHandleUpdatesAsync().GetAwaiter().GetResult();
        }
        */
    }
}