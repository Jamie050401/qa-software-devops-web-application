var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
       .Services.AddHsts(options =>
       {
           options.Preload = true;
           options.IncludeSubDomains = true;
           options.MaxAge = TimeSpan.FromDays(60);
       });

var app = builder.Build();

app.Urls.Clear();
if (!app.Environment.IsDevelopment())
{
    app.Urls.Add("https://*:5000");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.Urls.Add("http://*:5000");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();