using System.Diagnostics;
using System.Net.Http.Headers;
using Blazor.Components;
using Blazor.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using ServiceAccess;

namespace Blazor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Published/desktop-style run: pick a free loopback port so the app
            // never collides with something already listening on :5000.
            if (builder.Environment.IsProduction())
            {
                builder.WebHost.UseUrls("http://127.0.0.1:0");
            }

            var accessToken = builder.Configuration["TMDB:AccessToken"];

            if (string.IsNullOrWhiteSpace(accessToken) || accessToken.StartsWith("PASTE_"))
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: TMDB:AccessToken is not set in appsettings.json.");
                Console.WriteLine("Paste your TMDB API Read Access Token into appsettings.json,");
                Console.WriteLine("otherwise every TMDB request will fail.");
                Console.WriteLine();
            }

            // Register the TMDB API client.
            builder.Services.AddHttpClient<TMDBServiceAccess>(client =>
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            });

            // Per-circuit (per browser session) wishlist storage.
            builder.Services.AddScoped<WishlistService>();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Open the default browser once Kestrel is actually listening.
            if (app.Configuration.GetValue("App:OpenBrowser", true))
            {
                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    var url = app.Services.GetRequiredService<IServer>()
                        .Features.Get<IServerAddressesFeature>()?
                        .Addresses.FirstOrDefault();

                    if (string.IsNullOrEmpty(url))
                        return;

                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not open the browser: {ex.Message}");
                    }
                });
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

            // The publish/zip scenario serves plain HTTP on 127.0.0.1,
            // so HTTPS redirection would only produce a startup warning.
            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}