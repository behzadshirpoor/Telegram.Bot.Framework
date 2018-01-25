using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Base class for implementing Bots
    /// </summary>
    /// <typeparam name="TBot">Type of Bot</typeparam>
    public abstract class BotBase<TBot> : IBot
        where TBot : class, IBot
    {
        public User User { get; internal set; }

        /// <summary>
        /// Instance of Telegram bot client
        /// </summary>
        public ITelegramBotClient Client { get; }

        /// <summary>
        /// Options used to the configure the bot instance
        /// </summary>
        public BotOptionsBase Options { get; }

        internal GameOptionsBase[] GameOptions => Options?.GameOptions;

        /// <summary>
        /// Initializes a new Bot
        /// </summary>
        /// <param name="options">Options used to configure the bot</param>
        protected BotBase(BotOptionsBase options)
        {
            Options = options;
            Client = new TelegramBotClient(Options.ApiToken);
        }

        /// <summary>
        /// Responsible for handling bot updates that don't have any handler
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public abstract Task HandleUnknownUpdate(Update update);

        /// <summary>
        /// Receives the update when the handling process throws an exception for the update
        /// </summary>
        /// <param name="update"></param>
        /// <param name="e">Exception thrown while processing the update</param>
        /// <returns></returns>
        public abstract Task HandleFaultedUpdate(Update update, Exception e);
    }
}