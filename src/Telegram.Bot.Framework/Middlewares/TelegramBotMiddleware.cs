using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Middleware for handling Telegram bot's webhook requests
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public class TelegramBotMiddleware<TBot>
        where TBot : BotBase<TBot>
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<TelegramBotMiddleware<TBot>> _logger;

        /// <summary>
        /// Initializes an instance of middleware
        /// </summary>
        /// <param name="next">Instance of request delegate</param>
        /// <param name="logger">Logger for this middleware</param>
        public TelegramBotMiddleware(RequestDelegate next, ILogger<TelegramBotMiddleware<TBot>> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Gets invoked to handle the incoming request
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            var manager = context.RequestServices.GetRequiredService<UpdateManager<TBot>>();
            await manager.InitAsync();

            if (!(
                context.Request.Method == HttpMethods.Post &&
                manager.WebhookUrl.EndsWith(context.Request.Path)
                ))
            {
                await _next.Invoke(context);
                return;
            }

            string data;
            using (var reader = new StreamReader(context.Request.Body))
            {
                data = await reader.ReadToEndAsync();
            }

            _logger.LogTrace($"Update Data:`{data}`");

            Update update = null;
            try
            {
                update = JsonConvert.DeserializeObject<Update>(data);
            }
            catch (JsonException e)
            {
                _logger.LogWarning($"Unable to deserialize update payload. {e.Message}");
            }
            if (update == null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            try
            {
                await manager.HandleUpdateAsync(update);
                context.Response.StatusCode = StatusCodes.Status200OK;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while handling update `{update.Id}`. {e.Message}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}
