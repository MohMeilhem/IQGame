using IQGame.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;
using IQGame.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<GameStateService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<FavoritesService>();

// Configure HttpClient with base address and JSON options
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri("https://localhost:7187/") };
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    return client;
});

// Configure JSON serialization options
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.ReferenceHandler = ReferenceHandler.Preserve;
});

await builder.Build().RunAsync();
