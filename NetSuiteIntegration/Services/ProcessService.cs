using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Serilog;

namespace NetSuiteIntegration.Services
{
    public class ProcessService(ISRSWebServicecs unite, IFinanceWebService netsuite, ILogger logger) : IProcessService
    {
        ISRSWebServicecs? _unite = unite;
        IFinanceWebService? _netsuite = netsuite;
        ILogger? _log = logger;

        public async Task<bool> Process(string? _repGen)
        {
            //Steps
            //1. Get UNIT-e Enrolments in Scope

            // Check all parameters have values
            if (_log == null)
            {
                //Need to create a new logger instance if the logger is null to return the error
                var logConfig = new LoggerConfiguration();
                _log = logConfig.CreateLogger(); // Correctly create a logger instance
                _log?.Error("Logger is null/empty. This should reference the logger used to log messages.");
                return false;
            }
            if (_unite == null)
            {
                _log?.Error("UNIT-e Web Service is null/empty. This should reference the UNIT-e Web Service used to extract the data.");
                return false;
            }
            if (_netsuite == null)
            {
                _log?.Error("NetSuite Web Service is null/empty. This should reference the NetSuite Web Service used to extract the data.");
                return false;
            }

            if (_repGen == null)
            {
                _log?.Error("UNIT-e Report Reference is null/not specified. This should reference the RepGen Report used to extract the data.");
                return false;
            }

            try
            {
                Console.WriteLine("\nLoading UNIT-e Enrolments...");

                List<UNITeEnrolment>? uniteEnrolments = await _unite.ExportReport<List<UNITeEnrolment>>(_repGen ?? "");
                Console.WriteLine("\nLoading UNIT-e Enrolments after...");
                if (uniteEnrolments == null)
                {
                    _log?.Error("No UNIT-e Enrolments found.");
                    return false;
                }
                else if (uniteEnrolments?.Count == 0)
                {
                    _log?.Error("No UNIT-e Enrolments To Be Imported Currently.");
                    return false;
                }
                else
                {
                    Console.WriteLine($"Found {uniteEnrolments?.Count} UNIT-e Enrolments To Be Imported.");

                    foreach (UNITeEnrolment? uniteEnrolment in uniteEnrolments!)
                    {
                        Console.WriteLine($"\nUNIT-e Enrolment: {uniteEnrolment?.StudentRef} - {uniteEnrolment?.Surname} {uniteEnrolment?.Forename}");
                    }
                }

                NetSuiteCustomer? netSuiteCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", 111005);
                Console.WriteLine($"\nNetSuite Customer: {netSuiteCustomer?.EntityID} - {netSuiteCustomer?.FirstName} {netSuiteCustomer?.LastName}");

                if (netSuiteCustomer != null)
                {
                    //Was Nilsson
                    netSuiteCustomer.FirstName = "RobinTest";
                    netSuiteCustomer.LastName = "WilsonTest";

                    //If adding clear out IDs
                    netSuiteCustomer.ID = null;
                    netSuiteCustomer.ExternalID = "999999";
                    netSuiteCustomer.EntityID = "999999";
                }

                //Update a record
                //NetSuiteCustomer? updatedNetSuiteCustomer = await _netsuite.Update<NetSuiteCustomer>("customer", 5753, netSuiteCustomer);

                //Insert a record
                //NetSuiteCustomer? insertedNetSuiteCustomer = await _netsuite.Add<NetSuiteCustomer>("customer", netSuiteCustomer);

                //Delete a record
                //bool? isDeleted = await _netsuite.Delete<NetSuiteCustomer>("customer", 111005);

                //List all records
                //NetSuiteCustomerList? netSuiteCustomerList = await _netsuite.GetAll<NetSuiteCustomerList>("customer");

                //if (netSuiteCustomerList != null && netSuiteCustomerList?.Items?.Count > 0)
                //{
                //    foreach (NetSuiteCustomerListItem? customer in netSuiteCustomerList!.Items)
                //    {
                //        Console.WriteLine($"\nNetSuite Customer: {customer?.ID}");
                //    }
                //}

                //End successful return
                return true;
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in Process: {ex.Message}");
                Console.WriteLine($"Error in Process: {ex.Message}");
                return false;
            }
        }
    }
}
