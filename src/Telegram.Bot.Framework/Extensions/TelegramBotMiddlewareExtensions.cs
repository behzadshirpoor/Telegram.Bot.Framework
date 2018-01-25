using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Extensions for adding Telegram Bot framework to the ASP.NET Core middleware
    /// </summary>
    public static class TelegramBotMiddlewareExtensions
    {
        /// <summary>
        /// Add Telegram bot webhook handling functionality to the pipeline
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of IApplicationBuilder</param>
        /// <param name="ensureWebhookEnabled">Whether to set the webhook immediately by making a request to Telegram bot API</param>
        /// <returns>Instance of IApplicationBuilder</returns>
        public static IApplicationBuilder UseTelegramBotWebhook<TBot>(this IApplicationBuilder app, bool ensureWebhookEnabled = true)
            where TBot : BotBase<TBot>
        {
            var mgr = FindBotUpdateManager<TBot>(app);

            if (ensureWebhookEnabled)
            {
                mgr.InitAsync().GetAwaiter().GetResult();
                mgr.SetWebhookStateAsync(true).GetAwaiter().GetResult();
            }

            return app.UseMiddleware<TelegramBotMiddleware<TBot>>();
        }

        /// <summary>
        /// Removes and disables webhooks for bot
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of IApplicationBuilder</param>
        /// <param name="ensureWebhookDisabled">If true, a request is immediately made to delete webhook</param>
        /// <returns>Instance of IApplicationBuilder</returns>
        public static IApplicationBuilder UseTelegramBotLongPolling<TBot>(this IApplicationBuilder app, bool ensureWebhookDisabled = true)
            where TBot : BotBase<TBot>
        {
            var mgr = FindBotUpdateManager<TBot>(app);

            if (ensureWebhookDisabled)
            {
                mgr.SetWebhookStateAsync(false).GetAwaiter().GetResult();
            }

            Task.Run(async () =>
            {
                await mgr.InitAsync();
                while (string.IsNullOrWhiteSpace(""))
                {
                    await Task.Delay(3_000);
                    await mgr.GetHandleUpdatesAsync();
                }
            });

            return app;
        }

        /// <summary>
        /// Add a Telegram game score middleware to the app
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of IApplicationBuilder</param>
        /// <returns>Instance of IApplicationBuilder</returns>
        public static IApplicationBuilder UseTelegramGame<TBot>(this IApplicationBuilder app)
            where TBot : BotBase<TBot>
        {
            app.UseMiddleware<TelegramGameScoreMiddleware<TBot>>();

            return app;
        }

        private static UpdateManager<TBot> FindBotUpdateManager<TBot>(IApplicationBuilder app)
            where TBot : BotBase<TBot>
        {
            try
            {
                if (app.ApplicationServices.GetRequiredService<IUpdateManager<TBot>>()
                    is UpdateManager<TBot> mgr)
                {
                    return mgr;
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception)
            {
                throw new ConfigurationException(
                    "Bot UpdateManager service is not available", string.Format("Use services.{0}<{1}>()",
                        nameof(TelegramBotFrameworkIServiceCollectionExtensions.AddTelegramBot), typeof(TBot).Name));
            }
        }
    }
}
