using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Middleware for handling Telegram games' scores requests
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public class TelegramGameScoreMiddleware<TBot>
        where TBot : BotBase<TBot>
    {
        private readonly RequestDelegate _next;

        private readonly UpdateManager<TBot> _updateManager;

        private readonly ILogger<TelegramGameScoreMiddleware<TBot>> _logger;

        /// <summary>
        /// Initializes an instance of middleware
        /// </summary>
        /// <param name="next">Instance of request delegate</param>
        /// <param name="updateManager"></param>
        /// <param name="logger">Logger for this middleware</param>
        public TelegramGameScoreMiddleware(RequestDelegate next,
            IUpdateManager<TBot> updateManager,
            ILogger<TelegramGameScoreMiddleware<TBot>> logger)
        {
            _next = next;
            _logger = logger;
            _updateManager = (UpdateManager<TBot>)updateManager;
        }

        /// <summary>
        /// Gets invoked to handle the incoming request
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            string path = context.Request.Path.Value;

            string gameShortname = _updateManager.GamesOptions
                .SingleOrDefault(g =>
                    _updateManager.ReplaceGameUrlTokens(g.ScoresUrl, g.ShortName).EndsWith(path)
                )
                ?.ShortName;

            if (string.IsNullOrWhiteSpace(gameShortname) ||
                !new[] { HttpMethods.Post, HttpMethods.Get }.Contains(context.Request.Method))
            {
                await _next.Invoke(context);
                return;
            }

            var gameHandlerTuple = _updateManager.TryFindGameHandler(gameShortname);
            if (!gameHandlerTuple.Success)
            {
                await _next.Invoke(context);
                return;
            }

            IGameHandler gameHandler = gameHandlerTuple.GameHandler;

            if (context.Request.Method == HttpMethods.Get)
            {
                string playerid = context.Request.Query["id"];
                if (string.IsNullOrWhiteSpace(playerid) || playerid.Length < 20)
                {
                    _logger.LogError("Invalid player id passed. id=`{0}`", playerid);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var highScores = await gameHandler.GetHighestScoresAsync(_updateManager.Bot, playerid);

                var responseData = JsonConvert.SerializeObject(highScores);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(responseData);
            }
            else if (context.Request.Method == HttpMethods.Post)
            {
                string dataContent;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    dataContent = await reader.ReadToEndAsync();
                }

                SetGameScoreDto scoreData;
                try
                {
                    scoreData = JsonConvert.DeserializeObject<SetGameScoreDto>(dataContent);
                    if (scoreData == null)
                        throw new NullReferenceException();
                }
                catch (Exception e)
                   when (e is JsonSerializationException || e is NullReferenceException)
                {
                    _logger.LogError("Unable to deserialize score data. {0}.{1}Content: `{2}`",
                        e.Message, Environment.NewLine, dataContent);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                await gameHandler.SetGameScoreAsync(_updateManager.Bot, scoreData.PlayerId, scoreData.Score);
                context.Response.StatusCode = StatusCodes.Status201Created;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}