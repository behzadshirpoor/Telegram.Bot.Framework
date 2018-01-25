using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace EchoBot.Net45Console
{
    class EchoBot : BotBase<EchoBot>
    {
        public EchoBot(BotOptions<EchoBot> options)
            : base(options)
        {
        }

        public override Task HandleUnknownUpdate(Update update)
        {
            throw new NotImplementedException();
        }

        public override Task HandleFaultedUpdate(Update update, Exception e)
        {
            throw new NotImplementedException();
        }
    }
}
