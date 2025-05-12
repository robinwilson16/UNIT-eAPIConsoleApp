using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Reflection;
using System.Net;
using System.Globalization;
using System.Net.Http.Json;
using UNITe.Business.Helper;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Reflection.Metadata;

namespace NetSuiteIntegration
{
    class Program
    {
        public static bool CanConnect { get; set; }
        public static string? UNITeAPIToken { get; set; }
        public static bool? UNITeSessionIsValid { get; set; } = false;
        public static string? UNITeRepGenReportReference { get; set; } = "NetSuiteExport";

        static async Task<int> Main(string[] args)
        {
            string? locale = "en-GB";

            Console.WriteLine($"\nNetSuite Integration Utility\n");
            Console.WriteLine($"=========================================\n");

            string? productVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"Version {productVersion}");
            Console.WriteLine($"Copyright BIMM");


            Console.WriteLine($"\nSetting Locale To {locale}");

            //Set locale to ensure dates and currency are correct
            CultureInfo culture = new CultureInfo(locale ?? "en-GB");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;


            Console.WriteLine($"\nLoading Configuration Settings for APIs");
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
            .AddSingleton<ISRSWebServicecs, UniteWebServiceOld>()
            .AddSingleton<IFinanceWebService, NetSuiteWebService>()
            .AddDbContextFactory<NetsuiteContext>() //Configurtion and datamart source
            .AddSingleton<IMapper>(mapper)
            .BuildServiceProvider();

            //Get the settings from the database
            IDbContextFactory<NetsuiteContext>? dbContextFactory = serviceProvider.GetService<IDbContextFactory<NetsuiteContext>>();
            if (dbContextFactory == null)
            {
                log.Error("Database context not found, aborting");
                return 1;
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
                return 1;
            }

            mapper.Map(settings, appSettings);

            //Main coordination service, mainly exists so that Program.cs is just setup
            IProcessService? process = serviceProvider.GetService<IProcessService>();
            if (process == null)
            {
                log.Error("Process service not found");
                return 1;
            }

            log.Information("Start");
            //await process.DoSomething();

            
            UniteWebService uniteWebService = new UniteWebService(log, appSettings);
            NetSuiteWebService netSuiteWebService = new NetSuiteWebService(log, appSettings);

            //Get Access Token
            Console.WriteLine($"\nObtaining Access Token from {appSettings?.UniteTokenURL} for UNIT-e API using API Key {appSettings?.UniteAPIKey}");
            UNITeAPIToken = await uniteWebService.GetGuid();

            if (!string.IsNullOrEmpty(UNITeAPIToken))
                UNITeSessionIsValid = true;
            else
                UNITeSessionIsValid = false;

            if (UNITeSessionIsValid == true)
                Console.WriteLine($"\nObtained Access Token: {UNITeAPIToken}");
            else
                Console.WriteLine($"\nError: Could not obtain access token from UNIT-e API. Check API Key and URL are correct");

            List<UNITeEnrolment>? uniteEnrolments = await uniteWebService.ExportReport<List<UNITeEnrolment>>(UNITeRepGenReportReference ?? "", UNITeAPIToken);

            if (uniteEnrolments != null)
            {
                foreach (UNITeEnrolment? uniteEnrolment in uniteEnrolments)
                {
                    Console.WriteLine($"\nUNIT-e Enrolment: {uniteEnrolment?.StudentRef} - {uniteEnrolment?.Surname} {uniteEnrolment?.Forename}");
                }
            }

            //NetSuite HTTP Client
            HttpClient httpClientNetSuite = new HttpClient(new OAuth1Handler(appSettings))
            {
                BaseAddress = new Uri(appSettings.NetSuiteURL ?? "")
            };

            NetSuiteCustomer? netSuiteCustomer = await netSuiteWebService.GetNetSuiteRecord<NetSuiteCustomer>("customer", 5753);
            Console.WriteLine($"\nNetSuite Customer: {netSuiteCustomer?.EntityID} - {netSuiteCustomer?.FirstName} {netSuiteCustomer?.LastName}");

            //Invalidate Access Token
            if (UNITeSessionIsValid == true)
            {
                if (await uniteWebService.InvalidateSession(UNITeAPIToken ?? ""))
                {
                    UNITeSessionIsValid = false;
                    Console.WriteLine($"\nUNIT-e API Session Successfully Invalidated (Logged Out)");
                }
                else
                {
                    UNITeSessionIsValid = true;
                    Console.WriteLine($"\nError: UNIT-e API Session Could Not Be Invalidated (it may have expired already)");
                }
            }

            ////Not used - code for finding lists of students
            //StudentHESAParameter studentHESAParameter = new StudentHESAParameter
            //{
            //    Surname = "Wilson"
            //};

            //List<StudentHESA> students = await uniteWebService.Find<StudentHESA, StudentHESAParameter>(studentHESAParameter);

            return 0;
        }
    }
}

