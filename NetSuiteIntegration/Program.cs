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

            /*
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

            List<UNITeEnrolment>? uniteEnrolments = await uniteWebService.ExportReport<List<UNITeEnrolment>>(UNITeRepGenReportReference ?? "");

            if (uniteEnrolments != null)
            {
                foreach (UNITeEnrolment? uniteEnrolment in uniteEnrolments)
                {
                    Console.WriteLine($"\nUNIT-e Enrolment: {uniteEnrolment?.StudentRef} - {uniteEnrolment?.Surname} {uniteEnrolment?.Forename}");
                }
            }



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
            */

            ////Not used - code for finding lists of students
            //StudentHESAParameter studentHESAParameter = new StudentHESAParameter
            //{
            //    Surname = "Wilson"
            //};

            //List<StudentHESA> students = await uniteWebService.Find<StudentHESA, StudentHESAParameter>(studentHESAParameter);

            //UNIT-e HTTP Client
            HttpClient httpClientUNITe = new HttpClient();
            httpClientUNITe.BaseAddress = new Uri(appSettings.UniteBaseURL ?? "");

            //NetSuite HTTP Client
            HttpClient httpClientNetSuite = new HttpClient(new OAuth1Handler(appSettings))
            {
                BaseAddress = new Uri(appSettings.NetSuiteURL ?? "")
            };
            //httpClientNetSuite.DefaultRequestHeaders.Add("realm", appSettings.NetSuiteAccountID + "_SB1" ?? "");

            //string nonce = Guid.NewGuid().ToString("N");
            //string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            ////string signature = GenerateSignature(appSettings.NetSuiteConsumerSecret, appSettings.NetSuiteTokenSecret, nonce, timestamp, appSettings.NetSuiteURL ?? "", "GET", appSettings.NetSuiteConsumerKey ?? "", appSettings.NetSuiteAccountID ?? "" + "_SB1", "HMAC-SHA256", "1.0");
            //var netSuiteParameters = new SortedDictionary<string, string>
            //{
            //    { "oauth_consumer_key", appSettings.NetSuiteConsumerKey ?? "" },
            //    { "oauth_consumer_secret", appSettings.NetSuiteConsumerSecret ?? "" },
            //    { "oauth_token", appSettings.NetSuiteTokenID ?? "" },
            //    { "oauth_token_secret", appSettings.NetSuiteTokenSecret ?? "" },
            //    { "oauth_realm", appSettings.NetSuiteAccountID + "_SB1" ?? "" },
            //    { "oauth_nonce", nonce },
            //    { "oauth_timestamp", timestamp },
            //    { "oauth_signature_method", "HMAC-SHA256" },
            //    { "oauth_version", "1.0" }
            //};
            //string parameterString = string.Join("&", netSuiteParameters.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));


            //HttpClient httpClientNetSuite = new HttpClient();
            //httpClientNetSuite.BaseAddress = new Uri(appSettings.NetSuiteURL ?? "");

            Console.WriteLine($"\nObtaining Access Token from {appSettings?.UniteTokenURL} for UNIT-e API using API Key {appSettings?.UniteAPIKey}");

            if (appSettings != null)
                UNITeAPIToken = await GetUNITeAPIToken(httpClientUNITe, appSettings);

            if (!string.IsNullOrEmpty(UNITeAPIToken))
                Console.WriteLine($"\nObtained Access Token: {UNITeAPIToken}");

            List<UNITeEnrolment>? uniteEnrolments = await GetUNITeRepGenReport<UNITeEnrolment>(httpClientUNITe, appSettings, UNITeRepGenReportReference);

            if (uniteEnrolments != null)
            {
                foreach (UNITeEnrolment? uniteEnrolment in uniteEnrolments)
                {
                    Console.WriteLine($"\nUNIT-e Enrolment: {uniteEnrolment?.StudentRef} - {uniteEnrolment?.Surname} {uniteEnrolment?.Forename}");
                }
            }

            NetSuiteCustomer? netSuiteCustomer = await GetNetSuiteRecord<NetSuiteCustomer>(httpClientNetSuite, appSettings, "customer", 5753);
            Console.WriteLine($"\nNetSuite Customer: {netSuiteCustomer?.EntityID} - {netSuiteCustomer?.FirstName} {netSuiteCustomer?.LastName}");


            //Invalidate Access Token for UNIT-e
            if (appSettings != null && UNITeSessionIsValid == true)
                if (await InvalidateUNITeSession(httpClientUNITe, appSettings) == true)
                    UNITeSessionIsValid = false;
                else
                    UNITeSessionIsValid = true;

            if (UNITeSessionIsValid == false)
                Console.WriteLine($"\nUNIT-e API Session Successfully Invalidated (Logged Out)");
            else
                Console.WriteLine($"\nError: UNIT-e API Session Could Not Be Invalidated (it may have expired already)");

            log.Information("End");

            return 0;
        }


        #region UNIT-e Functions
        public static async Task<string> GetUNITeAPIToken(HttpClient httpClient, ApplicationSettings appSettings)
        {
            string apiToken = string.Empty;

            try
            {
                //Add API Key to Request
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", appSettings.UniteAPIKey);

                apiToken = await httpClient.GetStringAsync(appSettings.UniteTokenURL);

                UNITeSessionIsValid = true;
            }
            catch (HttpRequestException e)
            {

                Console.WriteLine(EndpointException(e, null));
                return string.Empty;
            }

            return apiToken ?? string.Empty;
        }

        public static async Task<List<T>?> GetUNITeRepGenReport<T>(HttpClient httpClient, ApplicationSettings appSettings, string? repGenReportName)
        {
            List<T>? reportData;

            try
            {
                // Add API Key to Request
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", UNITeAPIToken);
                string? reportURL = $"report/export/json/{repGenReportName}";

                reportData = await httpClient.GetFromJsonAsync<List<T>>(reportURL);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(EndpointException(e, null));
                return new List<T>();
            }

            return reportData;
        }

        public static async Task<bool> InvalidateUNITeSession(HttpClient httpClient, ApplicationSettings appSettings)
        {
            bool IsLoggedOut = false;
            string IsLoggedOutString = string.Empty;
            string? invalidateSessionEndpoint = $"InvalidateSession";

            try
            {
                //Add API Token to Request
                //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", UNITeAPIToken);

                IsLoggedOutString = await httpClient.GetStringAsync(invalidateSessionEndpoint);
            }
            catch (HttpRequestException e)
            {

                Console.WriteLine(EndpointException(e, null));
            }

            bool.TryParse(IsLoggedOutString, out IsLoggedOut);

            return IsLoggedOut;
        }
        #endregion

        #region NetSuite Functions
        public static async Task<T?> GetNetSuiteRecord<T>(HttpClient httpClient, ApplicationSettings appSettings, string? recordType, int? recordID)
        {
            T? reportData = default;

            try
            {
                string? recordURL = $"record/v1/{recordType}/{recordID}";
                //string? recordURL = $"https://7383276-sb1.suitetalk.api.netsuite.com/services/rest/record/v1/customer/{recordID}";
                reportData = await httpClient.GetFromJsonAsync<T>(recordURL);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(EndpointException(e, null));
                return reportData;
            }

            return reportData;
        }

        public static async Task<NetSuiteCustomer?> GetNetSuiteCustomer(HttpClient httpClient, FormUrlEncodedContent formParams, ApplicationSettings appSettings, int? customerID)
        {
            NetSuiteCustomer netSuiteCustomer = new NetSuiteCustomer();

            try
            {
                string? customerURL = $"record/v1/customer/{customerID}";
                
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                netSuiteCustomer = await httpClient.GetFromJsonAsync<NetSuiteCustomer>(customerURL, options);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(EndpointException(e, null));
                return new NetSuiteCustomer();
            }

            return netSuiteCustomer;
        }

        #endregion

        private static string EndpointException(Exception ex, int? recordID)
        {
            string errorMsg = "";
            if (ex.Message.Contains("The input does not contain any JSON tokens"))
            {
                //This is valid and the API returns 204 No Content which is eroneously logged as an error when it is not
            }
            else
            {
                CanConnect = false;

                if (ex.Message.Contains(HttpStatusCode.Unauthorized.ToString()))
                {
                    errorMsg = $"You are not authorised to view this page";
                }
                else if (ex.Message.Contains("404 (Not Found)"))
                {
                    if (recordID != null)
                    {
                        errorMsg = $"The record \"{recordID}\" requested does not exist";
                    }
                    else
                    {
                        errorMsg = $"The record does not exist";
                    }
                }
                else if (ex.Message.Contains("400 (Bad Request)"))
                {
                    if (recordID != null)
                    {
                        errorMsg = $"The record \"{recordID}\" requested is invalid";
                    }
                    else
                    {
                        errorMsg = $"The record does not exist";
                    }
                }
                else errorMsg = $"Error: {ex.Message}";
            }

            return errorMsg;
        }
        
    }
}

