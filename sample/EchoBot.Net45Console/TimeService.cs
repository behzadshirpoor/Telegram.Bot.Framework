using System;

namespace EchoBot.Net45Console
{
    public class TimeService
    {
        public string Time => DateTime.Now.ToLongTimeString();
    }
}
