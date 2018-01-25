// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    public class BotOptions<TBot> : BotOptionsBase
        where TBot : class, IBot
    {
        /// <summary>
        /// Url to be used for webhook
        /// </summary>
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Path to TLS certificate file. The .pem public key file used for encrypting and authenticating webhooks
        /// </summary>
        public string PathToCertificate { get; set; }

        public GameOptions[] Games { get; set; }
    }
}
