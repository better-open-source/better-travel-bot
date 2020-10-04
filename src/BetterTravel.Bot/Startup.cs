using System.Collections.Concurrent;
using BetterTravel.Bot.Bots;
using BetterTravel.Bot.Dialogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BetterTravel.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            services.AddScoped<IStorage, MemoryStorage>();
            services.AddScoped<IStateService, StateService>();

            services.AddTransient<UserState>();
            services.AddTransient<ConversationState>();
            services.AddTransient<DialogState>();

            services.AddTransient<Dialog, MainDialog>();
            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
