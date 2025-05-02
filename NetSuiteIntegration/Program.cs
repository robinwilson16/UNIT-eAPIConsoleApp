using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

//Starter application template using existing design patterns and existing Unit-e Web API code. DI and automapper etc
//is possibly overkill but added for consistency.

//Base logger information
using var log = new LoggerConfiguration()
.WriteTo.Console()
.WriteTo.MSSqlServer(
connectionString: "Server=uk-btn-sql8;Initial Catalog=NetSuite;TrustServerCertificate=True;Integrated Security=True",
sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
.CreateLogger();

//set up automapper. 
var mapper = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Setting, ApplicationSettings>();
}).CreateMapper();

//setup our DI container with concrete mappings. 
var serviceProvider = new ServiceCollection()
.AddSingleton<ApplicationSettings>()
.AddSingleton<ILogger>(log)
.AddSingleton<IProcessService, ProcessService>()
.AddSingleton<ISRSWebServicecs, UniteWebService>()
.AddSingleton<IFinanceWebService, NetSuiteWebService>()
.AddDbContextFactory<NetsuiteContext>() //Configurtion and datamart source
.AddSingleton<IMapper>(mapper)
.BuildServiceProvider();

//Get the settings from the database
IDbContextFactory<NetsuiteContext>? dbContextFactory = serviceProvider.GetService<IDbContextFactory<NetsuiteContext>>();
if (dbContextFactory == null)
{
    log.Error("Database context not found, aborting");
    return;
}
using NetsuiteContext dbContext = dbContextFactory.CreateDbContext();
//Get the currently enabled enviroment
//Netsuite has a sandbox and unite has a test database with it's own endpoint.
var settings = await (from c in dbContext.Settings where c.Enabled == true select c).FirstOrDefaultAsync();

//Populate the application settings to a singleton to avoid repeated database calls
ApplicationSettings? appSettings = serviceProvider.GetService<ApplicationSettings>();
if (appSettings == null)
{
    log.Error("Application settings not found, aborting");
    return;
}

mapper.Map(settings, appSettings);

//Main coordination service, mainly exists so that Program.cs is just setup
IProcessService? process = serviceProvider.GetService<IProcessService>();
if (process == null)
{
    log.Error("Process service not found");
    return;
}

log.Information("Start");

//await process.DoSomething();

log.Information("End");
