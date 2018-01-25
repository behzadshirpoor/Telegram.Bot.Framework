using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Framework;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding a Telegram Bot to an Microsoft.Extensions.DependencyInjection.IServiceCollection
    /// </summary>
    public static class TelegramBotFrameworkIServiceCollectionExtensions
    {
        private static IServiceCollection _services;

        /// <summary>
        /// Adds a Telegram bot to the service collection using the bot's options
        /// </summary>
        /// <typeparam name="TBot">Type of Telegram bot</typeparam>
        /// <param name="services">Instance of IServiceCollection</param>
        /// <param name="options">Options for configuring the bot</param>
        /// <returns>Instance of bot framework builder</returns>
        public static ITelegramBotFrameworkBuilder<TBot> AddTelegramBot<TBot>
            (this IServiceCollection services, BotOptions<TBot> options)
            where TBot : BotBase<TBot>
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            ThrowIfOptionsInvalid<TBot>(options);

            _services = services;
            return new TelegramBotFrameworkBuilder<TBot>(options);
        }

        /// <summary>
        /// Adds a Telegram bot to the service collection using configurations
        /// </summary>
        /// <typeparam name="TBot">Type of Telegram bot</typeparam>
        /// <param name="services">Instance of IServiceCollection</param>
        /// <param name="config">Configuring for the bot</param>
        /// <returns>Instance of bot framework builder</returns>
        public static ITelegramBotFrameworkBuilder<TBot> AddTelegramBot<TBot>
            (this IServiceCollection services, IConfiguration config)
            where TBot : BotBase<TBot>
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ThrowIfOptionsInvalid<TBot>(new BotOptions<TBot>
            {
                ApiToken = config[nameof(BotOptions<TBot>.ApiToken)],
                WebhookUrl = config[nameof(BotOptions<TBot>.WebhookUrl)],
                PathToCertificate = config[nameof(BotOptions<TBot>.PathToCertificate)]
            });

            _services = services;
            return new TelegramBotFrameworkBuilder<TBot>(config);
        }

        /// <summary>
        /// Responsible for configuring services for the bot and adding them to the container
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        public interface ITelegramBotFrameworkBuilder<TBot>
            where TBot : BotBase<TBot>
        {
            /// <summary>
            /// Configures an update handler for the bot
            /// </summary>
            /// <typeparam name="T">Type of update handler</typeparam>
            /// <returns>Itself</returns>
            ITelegramBotFrameworkBuilder<TBot> AddUpdateHandler<T>()
                where T : class, IUpdateHandler;

            /// <summary>
            /// Completes the configuration for the bot and adds all the services
            /// </summary>
            /// <returns>Instance of IServiceCollection</returns>
            IServiceCollection Configure();
        }

        private static void ThrowIfOptionsInvalid<TBot>(BotOptions<TBot> options)
            where TBot : BotBase<TBot>
        {
            if (string.IsNullOrWhiteSpace(options.ApiToken))
                throw new ArgumentNullException(nameof(BotOptions<TBot>.ApiToken));

            if (options.ApiToken.Length < 25)
                throw new ConfigurationException($@"API token ""{options.ApiToken}"" is too short.",
                    "Check bot's token with BotFather");

            if (!string.IsNullOrWhiteSpace(options.WebhookUrl) &&
                !options.WebhookUrl.ToLower().StartsWith("https://"))
                throw new ConfigurationException($@"Webhook url ""{options.WebhookUrl}"" is not a HTTPS url");

            if (!string.IsNullOrWhiteSpace(options.PathToCertificate) && !File.Exists(options.PathToCertificate))
                throw new ConfigurationException($@"Certificate file ""{options.PathToCertificate}"" does not exist.");

            if (true == options.Games?.Any(g =>
                    string.IsNullOrWhiteSpace(g.ShortName) || string.IsNullOrWhiteSpace(g.Url)))
                throw new ConfigurationException($@"Game(s) options invalid");
        }

        /// <summary>
        /// Responsible for configuring services for the bot and adding them to the container
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        public class TelegramBotFrameworkBuilder<TBot> : ITelegramBotFrameworkBuilder<TBot>
            where TBot : BotBase<TBot>
        {
            private readonly List<Type> _handlerTypes = new List<Type>();

            private readonly BotOptions<TBot> _botOptionsBase;

            private readonly IConfiguration _configuration;

            /// <summary>
            /// Initializes and instance of this class with the options provided
            /// </summary>
            /// <param name="botOptionsBase">Options for the bot</param>
            public TelegramBotFrameworkBuilder(BotOptions<TBot> botOptionsBase)
            {
                _botOptionsBase = botOptionsBase;
            }

            /// <summary>
            /// Initializes and instance of this class with the configuration provided
            /// </summary>
            /// <param name="configuration">Configuration for the bot</param>
            public TelegramBotFrameworkBuilder(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Configures an update handler for the bot
            /// </summary>
            /// <typeparam name="T">Type of update handler</typeparam>
            /// <returns>Itself</returns>
            public ITelegramBotFrameworkBuilder<TBot> AddUpdateHandler<T>()
                where T : class, IUpdateHandler
            {
                _handlerTypes.Add(typeof(T));
                return this;
            }

            /// <summary>
            /// Completes the configuration for the bot and adds all the services
            /// </summary>
            /// <returns>Instance of IServiceCollection</returns>
            public IServiceCollection Configure()
            {
                EnsureValidConfiguration();

                if (_botOptionsBase != null)
                {
                    _services.Configure<BotOptions<TBot>>(x =>
                    {
                        x.ApiToken = _botOptionsBase.ApiToken;
                        x.WebhookUrl = _botOptionsBase.WebhookUrl;
                        x.PathToCertificate = _botOptionsBase.PathToCertificate;
                        x.GameOptions = _botOptionsBase.GameOptions;
                    });
                }
                else
                {
                    _services.Configure<BotOptions<TBot>>(_configuration);
                }

                _services.AddScoped<TBot>();

                _handlerTypes.ForEach(x => _services.AddTransient(x));

                _services.AddScoped<IUpdateHandlersProvider<TBot>>(factory =>
                    new UpdateHanldersProviderAspNetCore<TBot>(factory, _handlerTypes)
                );

                _services.AddScoped<UpdateManager<TBot>>();
                _services.AddScoped<IUpdateManager<TBot>, UpdateManager<TBot>>();

                return _services;
            }

            private void EnsureValidConfiguration()
            {
                if (!_handlerTypes.Any())
                {
                    throw new ConfigurationException("No update handler is provided", $"Use {nameof(AddUpdateHandler)} method");
                }
                // ToDo: Validate others
            }
        }
    }
}
