using DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;

using DB.Data.Repositories;
using AGRICORE_ABM_object_relational_mapping.Services;
using DB.Data;

using Serilog;

using Serilog.Events;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

Log.Logger = new LoggerConfiguration()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);



// Adding serilog to the builder
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var configuration = builder.Configuration;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTIONSTRING") ?? "";
Log.Logger.Information("Using Connection String: " + connectionString);

bool enableAnyOriginCors = Environment.GetEnvironmentVariable("ENABLE_ALL_ORIGINS_CORS") == null ? false : true;

if (enableAnyOriginCors)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "AllowAll",
            policy =>
            {
                policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
            });
    });
}

var enableSensitive = (Environment.GetEnvironmentVariable("ENABLE_SENSITIVE") == null ? false : true);

builder.Services.AddDbContextFactory<AgricoreContext>(
                options =>
                {
                    options.UseNpgsql(connectionString);
                    if (enableSensitive)
                    {
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    }
                    //}, ServiceLifetime.Scoped); // This doesn't work.
                });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddRequestDecompression();

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAgriculturalProductionExtendedRepository, AgriculturalProductionExtendedRepository>();
builder.Services.AddScoped<ILivestockProductionExtendedRepository, LivestockProductionExtendedRepository>();
builder.Services.AddScoped<IDataImporterService, DataImporterService>();
builder.Services.AddScoped<IArableService, ArableService>();
builder.Services.AddTransient<ISimulationTasksService, SimulationTasksService>();
builder.Services.AddTransient<IJsonObjService, JsonObjService>();
builder.Services.AddTransient<IPopulationDuplicationService, PopulationDuplicationService>();

var app = builder.Build();

app.UseResponseCompression();
app.UseRequestDecompression();

app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    // Emit debug-level events instead of the defaults
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

using (var scope = app.Services.CreateScope())
{
    //var dataContext = scope.ServiceProvider.GetRequiredService<AgricoreContext>();
    //dataContext.Database.Migrate();

    var servicesProvider = scope.ServiceProvider;
    var logger = servicesProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initialising DB");
        DbInitializer.Initialize(servicesProvider);
        logger.LogInformation("Initialisation complete");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw;
    }
}

if (enableAnyOriginCors)
{
    app.UseCors("AllowAll");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} else
{
    if (Environment.GetEnvironmentVariable("FORCE_HTTPS") == null ? false : true)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (enableAnyOriginCors)
{
    app.UseCors("AllowAll");
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
