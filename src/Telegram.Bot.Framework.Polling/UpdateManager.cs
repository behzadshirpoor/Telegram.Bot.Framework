using System.Threading;
using System.Threading.Tasks;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Manages bot and sends updates to handlersProvider
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public class UpdateManager<TBot> : UpdateManagerBase<TBot>
        where TBot : BotBase<TBot>
    {
        /// <summary>
        /// Initializes a new Bot UpdateManager
        /// </summary>
        /// <param name="bot">Bot to be managed</param>
        /// <param name="handlersProvider">List of update parsers for the bot</param>
        public UpdateManager(TBot bot, IUpdateHandlersProvider<TBot> handlersProvider)
            : base(bot, handlersProvider)
        {
        }

        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await base.InitAsync(cancellationToken);
            await Bot.Client.DeleteWebhookAsync(cancellationToken);
        }
    }
}