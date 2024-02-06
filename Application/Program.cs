using Application.Data;

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
        options.Cookie.Name = "QAWebApplication";
        options.IdleTimeout = TimeSpan.FromMinutes(30);
    })
    .AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

DatabaseManager.InitialiseDatabase();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.Run();