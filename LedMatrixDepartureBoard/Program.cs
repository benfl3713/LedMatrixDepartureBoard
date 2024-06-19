using System.Reflection;
using BdfFontParser;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using LedMatrixDepartureBoard;
using LedMatrixDepartureBoard.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Serilog;
using Serilog.Events;

Console.WriteLine("Starting test load of font");
var test = new BdfFont($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Fonts/7x14.bdf");
test.GetMapOfString("Hello");

Console.WriteLine("Font loaded successfully");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("server.log")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var appConfig = builder.Configuration.Get<AppConfig>();
builder.Services.AddSingleton(appConfig!);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/home/ben/departureboard/.aspnet/DataProtection"));

builder.Services.Configure<HostOptions>(hostOptions => { hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; });
builder.Services.AddHttpClient();
builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
builder.Services.AddSingleton<DepartureCacheService>();
builder.Services.AddSingleton<DepartureDataService>();
builder.Services.AddHostedService<DepartureDataService>(p => p.GetRequiredService<DepartureDataService>());

builder.Services.AddSingleton<RgbMatrixFactory>();
builder.Services.AddSingleton<ILedMatrix, LedMatrix>();
builder.Services.AddSingleton<RenderingService>();
builder.Services.AddHostedService<RenderingService>(p => p.GetRequiredService<RenderingService>());

builder.Services.AddSingleton<UserConfigService>();
builder.Services.AddSingleton<StationInformationService>();
builder.Services.AddHostedService<ServiceManager>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    } )
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

var app = builder.Build();

//app.UseHttpsRedirection();

app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//app.UseHsts();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();