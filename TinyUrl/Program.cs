using TinyUrl.Middleware;
using TinyUrl.Models;
using TinyUrl.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));

builder.Services.AddSingleton<DbService>();
builder.Services.AddSingleton(sp =>
    new TinyUrlService(
        sp.GetRequiredService<ILogger<TinyUrlService>>(),
        sp.GetRequiredService<DbService>(),
        builder.Configuration.GetValue<int>("CacheSettings:CacheSize")
        ));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
