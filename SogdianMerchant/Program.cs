using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SogdianMerchant;
using SogdianMerchant.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IRandomGenerator, RandomGenerator>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IComputerDecisionService, ComputerDecisionService>();
builder.Services.AddScoped<IGameService, GameService>();

await builder.Build().RunAsync();