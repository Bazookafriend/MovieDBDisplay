using Blazor.Components;
using ServiceAccess;
using System.Net.Http.Headers;

namespace Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Register API
            builder.Services.AddHttpClient<TMDBServiceAccess>(client =>
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        builder.Configuration["TMDB:AccessToken"]);
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
