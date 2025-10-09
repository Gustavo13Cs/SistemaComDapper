using System.Data;
using System.Globalization;
using Estacionamento.Repositorios;
using Estacionamento.Services;
using Estacionamento.Servicos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using MySqlConnector;


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
builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient("OllamaClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434/");
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/conta/login";
        options.AccessDeniedPath = "/conta/acesso-negado";
    });
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();
builder.Services.AddAuthorization();

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
app.UseSession();


app.UseAuthorization();

app.MapStaticAssets();

app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/conta"))
    {
        context.Response.Redirect("/conta/login");
        return;
    }
    await next();
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
