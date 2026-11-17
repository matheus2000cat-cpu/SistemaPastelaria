using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

if (string.IsNullOrWhiteSpace(supabaseUrl) ||
    string.IsNullOrWhiteSpace(supabaseKey) ||
    supabaseKey == "******" ||
    supabaseKey.Contains("placeholder", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Configure Supabase:Url e Supabase:Key no appsettings local ou nas variáveis de ambiente do serviço.");
}

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(renderPort, out var port) && port is > 0 and <= 65535)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Adicionar serviços de Sessão
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".Pastelaria.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Adicionar serviços ao container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var options = new Supabase.SupabaseOptions
{
    AutoConnectRealtime = false
};

builder.Services.AddScoped(provider =>
    new Supabase.Client(supabaseUrl, supabaseKey, options)
);

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapPost("/api/caixa/abrir", async (HttpContext httpContext, Supabase.Client supabase) =>
{
    var operarioId = httpContext.Session.GetInt32("IdOperario");
    if (operarioId is null)
    {
        return Results.Redirect("/Auth/Login");
    }

    var form = await httpContext.Request.ReadFormAsync();
    var valorTexto = form["valorInicial"].ToString();
    if (!decimal.TryParse(valorTexto, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var valorInicial) &&
        !decimal.TryParse(valorTexto, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.CurrentCulture, out valorInicial))
    {
        return Results.BadRequest("Informe um valor inicial válido.");
    }

    if (valorInicial < 0)
    {
        return Results.BadRequest("O valor inicial não pode ser negativo.");
    }

    var caixasAbertos = await supabase.From<SistemaPastelaria.Models.Caixa>()
        .Where(caixa => caixa.Status == "Aberto")
        .Get();
    if (caixasAbertos.Models.Count > 0)
    {
        return Results.Redirect("/Caixa/Index");
    }

    await supabase.From<SistemaPastelaria.Models.Caixa>().Insert(new SistemaPastelaria.Models.Caixa
    {
        IdOperarioAbertura = operarioId.Value,
        ValorInicial = valorInicial,
        AberturaEm = DateTime.Now,
        Status = "Aberto"
    });

    return Results.Redirect("/Caixa/Index");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();