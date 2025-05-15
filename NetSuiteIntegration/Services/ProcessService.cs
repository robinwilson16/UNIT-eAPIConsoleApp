using System.Globalization;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetSuiteIntegration.Services
{
    public class ProcessService(ISRSWebServicecs unite, IFinanceWebService netsuite, ILogger logger) : IProcessService
    {
        ISRSWebServicecs? _unite = unite;
        IFinanceWebService? _netsuite = netsuite;
        ILogger? _log = logger;

        public async Task<bool> Process(string? _enrolmentRepGen)
        {
            //Steps
            //1. Get UNIT-e Enrolments in Scope
            //2. Map to Distinct UNIT-e Students
            //3. Map to NetSuite Customers

            // Check all parameters have values
            if (_log == null)
            {
                //Output to console incase cannot initialise a new logger
                Console.WriteLine("Logger is null/empty. This should reference the logger used to log messages.");
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

            if (_enrolmentRepGen == null)
            {
                _log?.Error("UNIT-e Enrolment Report Reference is null/not specified. This should reference the RepGen Report used to extract the data.");
                return false;
            }

            //Set up lists of students and enrolments
            IList<UNITeStudent>? uniteStudents = new List<UNITeStudent>();
            IList<UNITeEnrolment>? uniteEnrolments = new List<UNITeEnrolment>();
            IList<NetSuiteCustomer>? netSuiteCustomers = new List<NetSuiteCustomer>();

            try
            {
                _log?.Information("\nLoading UNIT-e Enrolments...");

                uniteEnrolments = await _unite.ExportReport<List<UNITeEnrolment>>(_enrolmentRepGen ?? "");


                _log?.Information("\nLoading UNIT-e Enrolments after...");
                if (uniteEnrolments == null)
                {
                    _log?.Information("No UNIT-e Enrolments found.");
                    return false;
                }
                else if (uniteEnrolments?.Count == 0)
                {
                    _log?.Information("No UNIT-e Enrolments To Be Imported Currently.");
                    return false;
                }
                else
                {
                    _log?.Information($"Loaded {uniteEnrolments?.Count} UNIT-e Enrolments");

                    //Map UNIT-e Enrolments to UNIT-e Students
                    uniteStudents = uniteEnrolments?.DistinctBy(e => e.StudentID)
                        .Select(stu => new UNITeStudent { 
                            StudentID = stu.StudentID, 
                            StudentRef = stu.StudentRef,
                            ERPID = stu.ERPID,
                            Surname = stu.Surname,
                            Forename = stu.Forename,
                            PreferredName = stu.PreferredName,
                            TitleCode = stu.TitleCode,
                            TitleName = stu.TitleName,
                            GenderCode = stu.GenderCode,
                            GenderName = stu.GenderName,
                            DateOfBirth = stu.DateOfBirth,
                            UCASPersonalID = stu.UCASPersonalID,
                            ULN = stu.ULN,
                            Address = stu.Address,
                            PostCode = stu.PostCode,
                            EmailAddress = stu.EmailAddress,
                            Mobile = stu.Mobile,
                            HomePhone = stu.HomePhone,
                            AcademicYearCode = stu.AcademicYearCode,
                            AcademicYearName = stu.AcademicYearName
                        }).ToList<UNITeStudent>();

                    _log?.Information($"Loaded {uniteStudents?.Count} Distinct UNIT-e Students");

                    //Map UNIT-e Students to NetSuite Customers
                    netSuiteCustomers = uniteStudents?.Select(cus => new NetSuiteCustomer
                    {
                        CustentityclientStudentno = cus.StudentRef,
                        ExternalID = cus.ERPID,
                        LastName = cus.Surname,
                        FirstName = cus.Forename,
                        Email = cus.EmailAddress,
                        IsPerson = true,
                        IsInactive = false,
                        DepositBalance = Convert.ToDouble(cus.FeeGross, CultureInfo.InvariantCulture)
                        //Add any other mappings here
                    }).ToList<NetSuiteCustomer>();

                    int rowNumber = 0;
                    foreach (NetSuiteCustomer? netSuiteCustomer in netSuiteCustomers!)
                    {
                        rowNumber++;
                        _log?.Information($"\nRecord {rowNumber} of {netSuiteCustomers.Count}: Checking {netSuiteCustomer?.LastName}, {netSuiteCustomer?.FirstName} ({netSuiteCustomer?.CustentityclientStudentno})");

                        //Check if the customer already exists in NetSuite by their 
                        IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
                        NetSuiteSearchParameter param = (new NetSuiteSearchParameter {
                            Operand = null,
                            FieldName = "CustentityclientStudentno",
                            Operator = Operator.IS,
                            Value = netSuiteCustomer?.CustentityclientStudentno,
                            IncludeOpeningParenthesis = false,
                            IncludeClosingParenthesis = false
                        });

                        searchParameters.Add(param);

                        NetSuiteCustomerList? customerList = await _netsuite.Search<NetSuiteCustomerList>("customer", searchParameters);

                        if (rowNumber > 1)
                            break;
                    }
                }

                //NetSuiteCustomer? netSuiteCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", 111005);
                //_log?.Information($"\nNetSuite Customer: {netSuiteCustomer?.EntityID} - {netSuiteCustomer?.FirstName} {netSuiteCustomer?.LastName}");



                //Testing
                //if (netSuiteCustomer != null)
                //{
                //    //Was Nilsson
                //    netSuiteCustomer.FirstName = "RobinTest";
                //    netSuiteCustomer.LastName = "WilsonTest";

                //    //If adding clear out IDs
                //    netSuiteCustomer.ID = null;
                //    netSuiteCustomer.ExternalID = "999999";
                //    netSuiteCustomer.EntityID = "999999";
                //}

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
                //        _log?.Information($"\nNetSuite Customer: {customer?.ID}");
                //    }
                //}

                //End successful return
                return true;
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in Process: {ex.Message}");
                return false;
            }
        }
    }
}
