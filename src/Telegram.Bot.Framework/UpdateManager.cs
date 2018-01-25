using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

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
        /// Array of game options for this bot's games
        /// </summary>
        public GameOptions[] GamesOptions => _options.Games;

        internal string WebhookUrl { get; private set; }

        private readonly BotOptions<TBot> _options;

        /// <summary>
        /// Initializes a new Bot UpdateManager
        /// </summary>
        /// <param name="bot">Bot to be managed</param>
        /// <param name="handlersProvider">List of update parsers for the bot</param>
        public UpdateManager(TBot bot, IUpdateHandlersProvider<TBot> handlersProvider)
            : base(bot, handlersProvider)
        {
            _options = (BotOptions<TBot>)Bot.Options;
        }

        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await base.InitAsync(cancellationToken);
            if (_options.WebhookUrl != default)
            {
                WebhookUrl = ReplaceUrlTokens(_options.WebhookUrl);
            }
        }

        /// <summary>
        /// Handle the update
        /// </summary>
        /// <param name="update">Update to be handled</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
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

                    if (handler is GameHandlerBase gameHandler)
                    {
                        if (GamesOptions is default)
                            throw new ConfigurationException("No game options are configured");

                        var options = GamesOptions
                            .SingleOrDefault(opts =>
                                gameHandler.ShortName.Equals(opts.ShortName, StringComparison.OrdinalIgnoreCase))
                            ?? throw new ConfigurationException($@"No game options are configured for game ""{gameHandler.ShortName}"".");

                        ThrowIfInvalidGameOptions(options);
                        options.Url = ReplaceGameUrlTokens(options.Url, options.ShortName);
                        options.ScoresUrl = ReplaceGameUrlTokens(options.ScoresUrl, options.ShortName);
                        gameHandler.Options = options;
                    }

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

        public async Task SetWebhookStateAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            if (enabled)
            {
                if (string.IsNullOrWhiteSpace(_options.PathToCertificate))
                    await Bot.Client.SetWebhookAsync(WebhookUrl, cancellationToken: cancellationToken);
                else
                {
                    using (var stream = File.OpenRead(_options.PathToCertificate))
                    {
                        await Bot.Client.SetWebhookAsync(WebhookUrl, stream, cancellationToken: cancellationToken);
                    }
                }
            }
            else
                await Bot.Client.DeleteWebhookAsync(cancellationToken);
        }

        /// <summary>
        /// Finds a handler for game by its short name
        /// </summary>
        /// <param name="gameShortName">Game's short name</param>
        /// <returns>
        /// A tuple with Success indicating presence of a game handler, and GameHandler, instance of
        /// game handler for that game
        /// </returns>
        public (bool Success, IGameHandler GameHandler) TryFindGameHandler(string gameShortName)
        {
            if (string.IsNullOrWhiteSpace(gameShortName))
                throw new ArgumentNullException(nameof(gameShortName));

            Update gameUpdate = new Update
            {
                CallbackQuery = new CallbackQuery
                {
                    GameShortName = gameShortName,
                },
            };

            var gameHandler = HandlersProvider
                .UpdateHandlers
                .Where(x => x.CanHandleUpdate(Bot, gameUpdate))
                .SingleOrDefault(h =>
                    h is GameHandlerBase && h.CanHandleUpdate(Bot, gameUpdate)
                ) as IGameHandler;

            return (gameHandler != null, gameHandler);
        }

        /// <summary>
        /// Replaces tokens, if any, in a game url
        /// </summary>
        /// <param name="urlFormat">A url with possibly tokens such as {game}</param>
        /// <param name="gameShortName">Game's short name</param>
        /// <returns>A url with all tokens replaced by their respective values</returns>
        public string ReplaceGameUrlTokens(string urlFormat, string gameShortName) =>
            ReplaceUrlTokens(urlFormat)
                .Replace(Constants.Placeholders.GameShortNamePlaceholder, gameShortName);

        internal string ReplaceUrlTokens(string urlFormat) =>
            urlFormat
                .Replace(Constants.Placeholders.BotUserNamePlaceholder, Bot.User.Username)
                .Replace(Constants.Placeholders.ApiTokenPlaceholder, _options.ApiToken);

        private void ThrowIfInvalidGameOptions(GameOptions options)
        {
            if (new[] { options.ShortName, options.Url, options.ScoresUrl }
                .Any(string.IsNullOrWhiteSpace))
                throw new ConfigurationException("Invalid game options");
        }

        internal static class Constants
        {
            public static class Placeholders
            {
                public const string BotUserNamePlaceholder = "{bot}";

                public const string ApiTokenPlaceholder = "{token}";

                public const string GameShortNamePlaceholder = "{game}";
            }
        }
    }
}