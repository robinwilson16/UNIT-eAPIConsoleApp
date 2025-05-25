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
using System.Text.Json;
using System.Reflection.Metadata;

namespace NetSuiteIntegration
{
    class Program
    {
        public static bool CanConnect { get; set; }
        public static string? UNITeAPIToken { get; set; }
        public static bool? UNITeSessionIsValid { get; set; } = false;
        public static ICollection<UNITeRepGen>? UNITeRepGens { get; set; } = new List<UNITeRepGen>();
        //API must be granted access to these reports in Security and Settings
        public static string? UNITeRepGenForEnrolments { get; set; } = "NetSuiteExportCustomers";
        public static string? UNITeRepGenForCourses { get; set; } = "NetSuiteExportCourses";
        public static string? UNITeRepGenForFees { get; set; } = "NetSuiteExportFees";
        public static string? UNITeRepGenForRefunds { get; set; } = "NetSuiteExportRefunds";
        public static bool? ReadOnly = true;
        public static bool? FirstRecordOnly = true;

        static async Task<int> Main(string[] args)
        {
            //Get list of RepGen Reports which will be used in the Process Service
            UNITeRepGens = GetRepGenReports();

            string? locale = "en-GB";
            string? productVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            #region Setup and Logging
            //Set locale to ensure dates and currency are correct
            CultureInfo culture = new CultureInfo(locale ?? "en-GB");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            //Starter application template using existing design patterns and existing Unit-e Web API code. DI and automapper etc
            //is possibly overkill but added for consistency.

            //Base logger information
            using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
            connectionString: "Server=uk-btn-sql8;Initial Catalog=NetSuite;TrustServerCertificate=True;Integrated Security=True",
            sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
            .CreateLogger();

            log.Information($"NetSuite Integration Utility");
            log.Information($"=========================================");
            log.Information($"Version {productVersion}");
            log.Information($"Copyright BIMM");
            log.Information($"Setting Locale To {locale}");
            log.Information($"\nLoading Configuration Settings for APIs");

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
            #endregion

            #region Main Logic and Process
            log.Information("Start");

            //Run main process
            await process!.Process(UNITeRepGens, ReadOnly, FirstRecordOnly);


            #endregion

            ////Not used - code for finding lists of students
            //StudentHESAParameter studentHESAParameter = new StudentHESAParameter
            //{
            //    Surname = "Wilson"
            //};

            //List<StudentHESA> students = await uniteWebService.Find<StudentHESA, StudentHESAParameter>(studentHESAParameter);

            return 0;
        }

        public static ICollection<UNITeRepGen>? GetRepGenReports()
        {
            ICollection<UNITeRepGen>? uniteRepGens = new List<UNITeRepGen>();

            //Set up list of RepGens defined above
            if (UNITeRepGenForEnrolments != null)
                UNITeRepGens?.Add(new UNITeRepGen
                {
                    Type = UNITeRepGenType.Enrolment,
                    Reference = UNITeRepGenForEnrolments,
                    Name = "UNITe RepGen for Enrolments"
                });

            if (UNITeRepGenForCourses != null)
                UNITeRepGens?.Add(new UNITeRepGen
                {
                    Type = UNITeRepGenType.Course,
                    Reference = UNITeRepGenForCourses,
                    Name = "UNITe RepGen for Courses"
                });

            if (UNITeRepGenForFees != null)
                UNITeRepGens?.Add(new UNITeRepGen
                {
                    Type = UNITeRepGenType.Fee,
                    Reference = UNITeRepGenForFees,
                    Name = "UNITe RepGen for Fees"
                });

            if (UNITeRepGenForRefunds != null)
                UNITeRepGens?.Add(new UNITeRepGen
                {
                    Type = UNITeRepGenType.Refund,
                    Reference = UNITeRepGenForRefunds,
                    Name = "UNITe RepGen for Refunds"
                });

            return UNITeRepGens;
        }
    }
}

