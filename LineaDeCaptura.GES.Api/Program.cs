using LineaDeCaptura.GES.Api.Data;
using LineaDeCaptura.GES.Api.Middleware;
using LineaDeCaptura.GES.Api.Options;
using LineaDeCaptura.GES.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
builder.Services.Configure<GesApiOptions>(builder.Configuration.GetSection(GesApiOptions.SectionName));

builder.Services.AddHttpClient<IGesApiClient, GesApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GesApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IGesRepository, GesRepository>();
builder.Services.AddScoped<IPosService, PosService>();
builder.Services.AddScoped<IReconciliationCsvService, ReconciliationCsvService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
