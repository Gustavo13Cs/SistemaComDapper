using System.Data;
using System.Globalization;
using Estacionamento.Controllers;
using Estacionamento.Repositorios;
using Estacionamento.Servicos;
using MySqlConnector;
using SeuProjeto.Controllers;


var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// registra DI para conexão Dapper/MySQL
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddScoped<IDbConnection>((sp) =>
new MySqlConnection(connectionString));

builder.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioDapper<>));
builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<TarifaService>();
builder.Services.AddHttpClient<ChatbotController>();




// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

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
