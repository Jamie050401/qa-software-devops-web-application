using Application.Data;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddRazorPages()
    .Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(60);
    })
    .AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
    })
    .AddMemoryCache()
    .AddSerilog(options =>
    {
        options.WriteTo.Console();
    })
    .AddNotyf(options =>
    {
        options.DurationInSeconds = 3;
        options.IsDismissable = true;
        options.Position = NotyfPosition.BottomRight;
    });

var app = builder.Build();

DatabaseManager.InitialiseDatabase();

if (app.Environment.IsDevelopment())
{
    // TODO - Need to update GitHub actions and Dockerfile/docker-compose.yml to ensure this environment variable is set
    Environment.SetEnvironmentVariable("QAWA-Cookie-Secret", "_zM374b@C9N_v-8?R-5?$-1J");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseSerilogRequestLogging();
app.UseNotyf();
app.MapRazorPages();
app.Run();