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

namespace NetSuiteIntegration
{
    class Program
    {
        public static bool CanConnect { get; set; }
        public static string? UNITeAPIToken { get; set; }
        public static bool? UNITeSessionIsValid { get; set; } = false;

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

            log.Information("Start");
            //await process.DoSomething();

            UniteWebService uniteWebService = new UniteWebService(log, appSettings);

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

            StudentHESAParameter studentHESAParameter = new StudentHESAParameter
            {
                Surname = "Wilson"
            };

            List<StudentHESA> students = await uniteWebService.Find<StudentHESA, StudentHESAParameter>(studentHESAParameter);



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


            //HttpClient httpClient = new HttpClient();

            //Console.WriteLine($"\nObtaining Access Token from {appSettings?.UniteTokenURL} for UNIT-e API using API Key {appSettings?.UniteAPIKey}");

            //if (appSettings != null)
            //    UNITeAPIToken = await GetUNITeAPIToken(httpClient, appSettings);

            //if (!string.IsNullOrEmpty(UNITeAPIToken))
            //    Console.WriteLine($"\nObtained Access Token: {UNITeAPIToken}");

            //if (appSettings != null && UNITeSessionIsValid == true)
            //    if (await InvalidateUNITeSession(httpClient, appSettings) == true)
            //        UNITeSessionIsValid = false;
            //    else
            //        UNITeSessionIsValid = true;

            //if (UNITeSessionIsValid == false)
            //        Console.WriteLine($"\nUNIT-e API Session Successfully Invalidated (Logged Out)");
            //    else
            //        Console.WriteLine($"\nError: UNIT-e API Session Could Not Be Invalidated (it may have expired already)");

            log.Information("End");

            return 0;
        }


        #region Custom Properties I Created But Are Not Needed
        //public static async Task<string> GetUNITeAPIToken(HttpClient httpClient, ApplicationSettings appSettings)
        //{
        //    string apiToken = string.Empty;

        //    try
        //    {
        //        //Add API Key to Request
        //        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", appSettings.UniteAPIKey);

        //        apiToken = await httpClient.GetStringAsync(appSettings.UniteTokenURL);

        //        UNITeSessionIsValid = true;
        //    }
        //    catch (HttpRequestException e)
        //    {

        //        Console.WriteLine(EndpointException(e, null));
        //        return string.Empty;
        //    }

        //    return apiToken ?? string.Empty;
        //}

        //public static async Task<bool> InvalidateUNITeSession(HttpClient httpClient, ApplicationSettings appSettings)
        //{
        //    bool IsLoggedOut = false;
        //    string IsLoggedOutString = string.Empty;
        //    string? invalidateSessionEndpoint = $"{appSettings.UniteBaseURL}/InvalidateSession";

        //    try
        //    {
        //        //Add API Token to Request
        //        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", UNITeAPIToken);

        //        IsLoggedOutString = await httpClient.GetStringAsync(invalidateSessionEndpoint);
        //    }
        //    catch (HttpRequestException e)
        //    {

        //        Console.WriteLine(EndpointException(e, null));
        //    }

        //    bool.TryParse(IsLoggedOutString, out IsLoggedOut);

        //    return IsLoggedOut;
        //}

        //private static string EndpointException(Exception ex, int? recordID)
        //{
        //    string errorMsg = "";
        //    if (ex.Message.Contains("The input does not contain any JSON tokens"))
        //    {
        //        //This is valid and the API returns 204 No Content which is eroneously logged as an error when it is not
        //    }
        //    else
        //    {
        //        CanConnect = false;

        //        if (ex.Message.Contains(HttpStatusCode.Unauthorized.ToString()))
        //        {
        //            errorMsg = $"You are not authorised to view this page";
        //        }
        //        else if (ex.Message.Contains("404 (Not Found)"))
        //        {
        //            if (recordID != null)
        //            {
        //                errorMsg = $"The record \"{recordID}\" requested does not exist";
        //            }
        //            else
        //            {
        //                errorMsg = $"The record does not exist";
        //            }
        //        }
        //        else if (ex.Message.Contains("400 (Bad Request)"))
        //        {
        //            if (recordID != null)
        //            {
        //                errorMsg = $"The record \"{recordID}\" requested is invalid";
        //            }
        //            else
        //            {
        //                errorMsg = $"The record does not exist";
        //            }
        //        }
        //        else errorMsg = $"Error: {ex.Message}";
        //    }

        //    return errorMsg;
        //}
        #endregion
    }
}

