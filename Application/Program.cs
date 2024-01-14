var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
       .Services.AddHsts(options =>
       {
           options.Preload = true;
           options.IncludeSubDomains = true;
           options.MaxAge = TimeSpan.FromDays(60);
       });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{

    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();