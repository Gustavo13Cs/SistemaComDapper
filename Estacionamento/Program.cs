using System.Data;
using System.Globalization;
using Estacionamento.Repositorios;
using Estacionamento.Servicos;
using Microsoft.AspNetCore.Localization;
using MySqlConnector;
using SeuProjeto.Controllers;


var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = new[] { cultureInfo };
    options.SupportedUICultures = new[] { cultureInfo };
});
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddScoped<IDbConnection>((sp) =>
new MySqlConnection(connectionString));

builder.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioDapper<>));
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<IntentionService>();
builder.Services.AddScoped<TarifaService>();

builder.Services.AddHttpClient<ChatbotController>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
});
builder.Services.AddHttpClient<IntentionService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
});

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var intentionService = scope.ServiceProvider.GetRequiredService<IntentionService>();
    await intentionService.InitializeAsync();
}


app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
