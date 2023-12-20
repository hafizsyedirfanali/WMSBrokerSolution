using Microsoft.Extensions.Configuration;
using WMSBrokerProject.ConfigModels;
using WMSBrokerProject.Interfaces;

//using WMSBrokerProject.Logging;
using WMSBrokerProject.Repositories;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Configuration.AddJsonFile("wmssettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"wmssettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddJsonFile("goEfficientSettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"goEfficientSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddJsonFile("WMSBeheerderAttributesSettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"WMSBeheerderAttributesSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddJsonFile("wmsBeheerderMapping.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"wmsBeheerderMapping.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddJsonFile("wmsOrderProgressSettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"wmsOrderProgressSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

//builder.Configuration.AddJsonFile("OrderProgressMapping.json", optional: true, reloadOnChange: true);
//builder.Configuration.AddJsonFile($"OrderProgressMapping.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);


builder.Services.Configure<Dictionary<string, ActionConfiguration>>(configuration.GetSection("Actions"));
builder.Services.Configure<GoEfficientCredentials>(configuration.GetSection("GoEfficientCredentials"));
builder.Services.Configure<OrderProgressConfigurationModel>(configuration.GetSection("OrderProgressTemplates"));
//builder.Services.Configure<OrderProgressMappingOptions>(configuration.GetSection("OrderProgressMapping"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IGoEfficientService, GoEfficientServiceImplementation>();
builder.Services.AddTransient<IWMSBeheerderService, WMSBeheerderImplementation>();
builder.Services.AddTransient<IOrderProgressService, WMSOrderProgressImplementation>();
builder.Services.AddSingleton<ICorrelationServices, CorrelationImplementation>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}


app.UseHttpsRedirection();
//app.UseStaticFiles();


app.UseAuthorization();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
    c.RoutePrefix = string.Empty;
});
app.UseRouting();
app.MapControllers();
app.Run();
