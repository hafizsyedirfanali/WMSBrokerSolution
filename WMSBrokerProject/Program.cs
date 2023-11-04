using Microsoft.Extensions.Configuration;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;
using WMSBrokerProject.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Configuration.AddJsonFile("wmssettings.json", optional: true, reloadOnChange: true);
//builder.Configuration.AddJsonFile("WMSBeheerderAttributesSettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("WMSBeheerderAttributesSettings-Copy.json", optional: true, reloadOnChange: true);
builder.Services.Configure<Dictionary<string, ActionConfiguration>>(configuration.GetSection("Actions"));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IGoEfficientService, GoEfficientServiceImplementation>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
