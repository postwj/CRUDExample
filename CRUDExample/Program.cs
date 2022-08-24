var configuration = GetConfiguration();
Log.Logger = CreateSeriLogger(configuration);

Log.Information($"information {Program.AppName}");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog(CreateSeriLogger(configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ItemCatalogContext>(opt => opt.UseInMemoryDatabase("items"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IConfiguration GetConfiguration()
{
    var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
    return configurationBuilder.Build();
}

Serilog.ILogger CreateSeriLogger(IConfiguration configuration)
{
    var segServerUrl = configuration["Serilog:SeqServerUrl"];
    var logStashUrl = configuration["Serilog:LogstashgUrl"];

    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(segServerUrl) ? "http://seq" : segServerUrl)
        .WriteTo.Http(string.IsNullOrWhiteSpace(logStashUrl) ? "http://logstash:8080" : logStashUrl, queueLimitBytes: null)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

public partial class Program
{
    public static string Namespace = typeof(Item).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}
