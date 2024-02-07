using Application.Data;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddRazorPages(options =>
    {
        // NOTE: For some reason enabling this route causes the dashboard button in the nav bar to route to /Register
        //options.Conventions.AddPageRoute("/Dashboard", "/");
    })
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

if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

DatabaseManager.InitialiseDatabase();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseSerilogRequestLogging();
app.UseNotyf();
app.MapRazorPages();
app.Run();