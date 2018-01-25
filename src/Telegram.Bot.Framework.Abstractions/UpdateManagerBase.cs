using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Manages bot and sends updates to handlers
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public abstract class UpdateManagerBase<TBot> : IUpdateManager<TBot>
        where TBot : BotBase<TBot>
    {
        protected internal readonly TBot Bot;

        protected readonly IUpdateHandlersProvider<TBot> HandlersProvider;

        private int _offset;

        /// <summary>
        /// Initializes a new Bot Manager
        /// </summary>
        /// <param name="bot">Bot to be managed</param>
        /// <param name="handlersProvider">List of update parsers for the bot</param>
        protected UpdateManagerBase(TBot bot, IUpdateHandlersProvider<TBot> handlersProvider)
        {
            Bot = bot;
            HandlersProvider = handlersProvider;
        }

        public virtual async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Bot.User = await Bot.Client.GetMeAsync(cancellationToken);
        }

        public virtual async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
        {
            bool anyHandlerExists = false;
            try
            {
                var handlers = HandlersProvider
                    .UpdateHandlers
                    .Where(x => x.CanHandleUpdate(Bot, update));

                foreach (IUpdateHandler handler in handlers)
                {
                    anyHandlerExists = true;

                    var result = await handler.HandleUpdateAsync(Bot, update);
                    if (result == UpdateHandlingResult.Handled)
                    {
                        return;
                    }
                }

                if (!anyHandlerExists)
                {
                    await Bot.HandleUnknownUpdate(update);
                }
            }
            catch (Exception e)
            {
                await Bot.HandleFaultedUpdate(update, e);
            }
        }

        /// <summary>
        /// Pulls the updates from Telegram if any and passes them to handlers
        /// </summary>
        /// <returns></returns>
        public async Task GetHandleUpdatesAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<Update> updates;
            do
            {
                updates = await Bot.Client.GetUpdatesAsync(_offset, timeout: 30, cancellationToken: cancellationToken);

                foreach (var update in updates)
                {
                    await HandleUpdateAsync(update, cancellationToken);
                }

                _offset = updates.LastOrDefault()?.Id + 1 ?? _offset;
            } while (updates.Any());
        }

        public async Task GetHandleUpdatesAsync(GetUpdatesRequest request, CancellationToken cancellationToken = default)
        {
            IEnumerable<Update> updates;
            do
            {
                updates = await Bot.Client.MakeRequestAsync(request, cancellationToken);

                foreach (var update in updates)
                {
                    await HandleUpdateAsync(update, cancellationToken);
                }

                _offset = updates.LastOrDefault()?.Id + 1 ?? _offset;
            } while (updates.Any());
        }
    }
}