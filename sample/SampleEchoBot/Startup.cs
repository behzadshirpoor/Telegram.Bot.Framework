using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework;

namespace SampleEchoBot
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTelegramBot<EchoBot>(_configuration.GetSection("EchoBot"))
                .AddUpdateHandler<SayHelloHandler>()
                .AddUpdateHandler<EchoCommand>()
                .Configure();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(_configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            ILogger logger = loggerFactory.CreateLogger<Startup>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseTelegramBotLongPolling<EchoBot>();
                logger.LogInformation("Update getting task is scheduled for bot " + nameof(EchoBot));
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                    appBuilder.Run(context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return Task.CompletedTask;
                    })
                );

                logger.LogInformation($"Setting webhook for {nameof(EchoBot)}...");
                app.UseTelegramBotWebhook<EchoBot>();
                logger.LogInformation("Webhook is set for bot " + nameof(EchoBot));
            }

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
