using System.Globalization;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> Process(string? _enrolmentRepGen, string? _courseRepGen)
        {
            bool? ReadOnly = true;
            bool? FirstRecordOnly = true;

            //Steps
            //1. Get UNIT-e Enrolments in Scope
            //2. Map to Distinct UNIT-e Students
            //3. Map to NetSuite Customers by Student Ref
            //3a. Map to NetSuite Customers by ERP No if Ref not found
            //3b. Map to NetSuite Customers by Name/Email/Mobile if Ref and ERP not found
            //4. Get NetSuite Postal Addresses and check if the primary ones needs updating or new ones need inserting (not currently deleting any)
            //5. Update NetSuite Customers with UNIT-e data
            //6. Get UNIT-e Courses in Scope

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

            //Set up lists of students and enrolments and courses
            IList<UNITeStudent>? uniteStudents = new List<UNITeStudent>();
            IList<UNITeEnrolment>? uniteEnrolments = new List<UNITeEnrolment>();
            IList<UNITeCourse>? uniteCourses = new List<UNITeCourse>();
            IList<NetSuiteCustomer>? uniteNetSuiteCustomers = new List<NetSuiteCustomer>();
            
            try
            {
                _log?.Information("\nLoading UNIT-e Enrolments...");

                uniteEnrolments = await _unite.ExportReport<List<UNITeEnrolment>>(_enrolmentRepGen ?? "");

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

                    if (uniteEnrolments != null)
                        uniteStudents = GetUNITeStudentsFromEnrolments(uniteEnrolments);

                    _log?.Information($"Loaded {uniteStudents?.Count} Distinct UNIT-e Students");

                    if (uniteStudents != null)
                        uniteNetSuiteCustomers = MapUNITeStudentsToNetSuiteCustomers(uniteStudents);

                    int rowNumber = 0;
                    foreach (NetSuiteCustomer? uniteNetSuiteCustomer in uniteNetSuiteCustomers!)
                    {
                        rowNumber++;
                        _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteCustomers.Count}: Searching for {uniteNetSuiteCustomer?.LastName}, {uniteNetSuiteCustomer?.FirstName} ({uniteNetSuiteCustomer?.CustEntityClientStudentNo}) in NetSuite");

                        #region Find Customer and Related Datasets
                        //Find this customer in NetSuite
                        NetSuiteCustomer? matchedCustomer = await GetCustomer(uniteNetSuiteCustomer ?? new NetSuiteCustomer());

                        if (matchedCustomer?.RecordMatchType == RecordMatchType.ByStudentRef)
                            _log?.Information($"Customer Found in NetSuite by Student Ref with NetSuite Customer ID: {matchedCustomer?.ID}");
                        else if (matchedCustomer?.RecordMatchType == RecordMatchType.ByERPID)
                            _log?.Information($"Customer Found in NetSuite by ERP ID with NetSuite Customer ID: {matchedCustomer?.ID}");
                        else if (matchedCustomer?.RecordMatchType == RecordMatchType.ByPersonalDetails)
                            _log?.Information($"Customer Found in NetSuite by Name/Email/Phone with NetSuite Customer ID: {matchedCustomer?.ID}");
                        else
                            _log?.Information($"Customer Not Found in NetSuite");

                        int? numUniteAddresses = uniteNetSuiteCustomer?.Addresses?.Where(a => a.Label != null).DistinctBy(a => a.Label).ToList().Count ?? 0;

                        if (matchedCustomer?.ID != null)
                        {
                            //Update the ID of the record that came from UNIT-e so it can be used to update the record in NetSuite
                            if (uniteNetSuiteCustomer != null)
                                uniteNetSuiteCustomer.ID = matchedCustomer?.ID;

                            //Get addresses
                            if (matchedCustomer != null && matchedCustomer?.ID != null)
                                matchedCustomer.Addresses = await GetAddresses(matchedCustomer);

                            _log?.Information($"\nFound {numUniteAddresses} addresses for customer in UNIT-e");
                            _log?.Information($"Found {matchedCustomer?.Addresses?.Count ?? 0} addresses for customer in NetSuite");

                            if (matchedCustomer?.Addresses?.Count > 0)
                            {

                            }
                        }

                        //_log?.Information($"Main Address Type {uniteNetSuiteCustomer?.Addresses?.Where(a => a.DefaultBilling == true).FirstOrDefault()?.Label}");

                        //Check if addresses are up to date and flag each one for insert or update
                        if (uniteNetSuiteCustomer != null)
                            uniteNetSuiteCustomer.Addresses = CheckAddresses(uniteNetSuiteCustomer, matchedCustomer);

                        #endregion

                        #region Perform Updates to NetSuite Customers
                        //Update or add the Customer record in NetSuite
                        NetSuiteCustomer updatedCustomer = await UpdateCustomer(uniteNetSuiteCustomer ?? new NetSuiteCustomer(), ReadOnly);

                        //Update the ID of the record that came from UNIT-e so it matches the newly inserted record if not updating an existing NetSuite record
                        if (uniteNetSuiteCustomer != null)
                        {
                            if (updatedCustomer?.RecordActionType == RecordActionType.Insert)
                            {
                                //Add the ID of the newly inserted record to the UNIT-e record
                                uniteNetSuiteCustomer.ID = updatedCustomer?.ID;
                                _log?.Information($"Inserted New NetSuite Customer: {uniteNetSuiteCustomer?.ID}");
                            }
                            else if (updatedCustomer?.RecordActionType == RecordActionType.Update)
                            {
                                _log?.Information($"Synced Existing NetSuite Customer: {uniteNetSuiteCustomer?.ID}");
                            }
                            else
                            {
                                _log?.Information($"No Changes Made to NetSuite Customer: {uniteNetSuiteCustomer?.ID}");
                            }
                        }

                        //Update the addresses in NetSuite
                        bool? addressesUpdated = false;
                        if (uniteNetSuiteCustomer != null)
                            addressesUpdated = await UpdateAddresses(uniteNetSuiteCustomer, ReadOnly);

                        #endregion

                        if (FirstRecordOnly == true)
                            break;
                    }
                }

                //NetSuiteCustomer? existingNetSuiteCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", 5753);
                //_log?.Information($"\nNetSuite Customer: {existingNetSuiteCustomer?.EntityID} - {existingNetSuiteCustomer?.FirstName} {existingNetSuiteCustomer?.LastName}");



                //Testing
                //if (existingNetSuiteCustomer != null)
                //{
                //    //Was Nilsson
                //    existingNetSuiteCustomer.FirstName = "RobinTest";
                //    existingNetSuiteCustomer.LastName = "WilsonTest";

                //    //If adding clear out IDs
                //    existingNetSuiteCustomer.ID = null;
                //    existingNetSuiteCustomer.ExternalID = "999999";
                //    existingNetSuiteCustomer.EntityID = "999999";

                //    //Clear out sub-elements that reference the other customer otherwise it will lead to an Invalid Value error
                //    existingNetSuiteCustomer.AddressBook = new NetSuiteCustomerAddressBook();
                //    existingNetSuiteCustomer.CurrencyList = new NetSuiteCustomerCurrencyList();
                //    existingNetSuiteCustomer.GroupPricing = new NetSuiteCustomerGroupPricing();
                //    existingNetSuiteCustomer.ItemPricing = new NetSuiteCustomerItemPricing();
                //    existingNetSuiteCustomer.SalesTeam = new NetSuiteCustomerSalesTeam();
                //}

                //Update a record
                //NetSuiteCustomer? updatedNetSuiteCustomer = await _netsuite.Update<NetSuiteCustomer>("customer", 5753, existingNetSuiteCustomer);

                //Insert a record
                //NetSuiteCustomer? insertedNetSuiteCustomer = await _netsuite.Add<NetSuiteCustomer>("customer", existingNetSuiteCustomer);

                //Delete a record
                //bool? isDeleted = await _netsuite.Delete<NetSuiteCustomer>("customer", 111005);

                //return true;

            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Enrolments: {ex.Message}");
                return false;
            }

            try
            {
                _log?.Information("\nLoading UNIT-e Courses...");

                uniteCourses = await _unite.ExportReport<List<UNITeCourse>>(_courseRepGen ?? "");

                if (uniteCourses == null)
                {
                    _log?.Information("No UNIT-e Courses found.");
                    return false;
                }
                else if (uniteCourses?.Count == 0)
                {
                    _log?.Information("No UNIT-e Courses To Be Imported Currently.");
                    return false;
                }
                else
                {
                    _log?.Information($"Loaded {uniteCourses?.Count} UNIT-e Courses");




                    return true;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Courses: {ex.Message}");
                return false;
            }
        }

        public IList<UNITeStudent> GetUNITeStudentsFromEnrolments(IList<UNITeEnrolment> uniteEnrolments)
        {
            IList<UNITeStudent>? uniteStudents = new List<UNITeStudent>();

            //Map UNIT-e Enrolments to UNIT-e Students
            uniteStudents = uniteEnrolments?.DistinctBy(e => e.StudentID)
                .Select(stu => new UNITeStudent
                {
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
                    AddressMain = stu.AddressMain,
                    PostCodeMain = stu.PostCodeMain,
                    AddressMainType = stu.AddressMainType,
                    AddressTermTime = stu.AddressTermTime,
                    PostCodeTermTime = stu.PostCodeTermTime,
                    AddressHome = stu.AddressHome,
                    PostCodeHome = stu.PostCodeHome,
                    AddressInvoice = stu.AddressInvoice,
                    PostCodeInvoice = stu.PostCodeInvoice,
                    EmailAddress = stu.EmailAddress,
                    Mobile = stu.Mobile,
                    HomePhone = stu.HomePhone,
                    AcademicYearCode = stu.AcademicYearCode,
                    AcademicYearName = stu.AcademicYearName
                }).ToList<UNITeStudent>();

            return uniteStudents ?? new List<UNITeStudent>();
        }

        public IList<NetSuiteCustomer> MapUNITeStudentsToNetSuiteCustomers(IList<UNITeStudent> uniteStudents)
        {
            IList<NetSuiteCustomer>? netSuiteCustomers = new List<NetSuiteCustomer>();

            //Map UNIT-e Students to NetSuite Customers
            netSuiteCustomers = uniteStudents?.Select(cus => new NetSuiteCustomer
            {
                CustEntityClientStudentNo = cus.StudentRef,
                CustEntityCRMApplicantID = cus.ERPID,
                LastName = cus.Surname,
                FirstName = cus.Forename,
                Email = cus.EmailAddress,
                Phone = cus.Mobile,
                IsPerson = true,
                IsInactive = false,
                DepositBalance = Convert.ToDouble(cus.FeeGross, CultureInfo.InvariantCulture),
                Addresses = new List<NetSuiteAddressBook>{
                    cus.PostCodeMain != null ? new NetSuiteAddressBook {
                        DefaultBilling = true,
                        DefaultShipping = true,
                        IsResidential = true,
                        Label = cus.AddressMainType,
                        Address = new NetSuiteAddress
                        {
                            Addr1 = cus.Address1Main,
                            Addr2 = cus.Address2Main,
                            Addressee = $"{cus.Forename} {cus.Surname}",
                            City = cus.Address3Main,
                            Country = new NetSuiteAddressCountry
                            {
                                RefName = cus.CountryNameMain
                            },
                            Override = false,
                            Zip = cus.PostCodeMain
                        }
                    } : new NetSuiteAddressBook(),
                    cus.PostCodeTermTime != null ? new NetSuiteAddressBook
                    {
                        DefaultBilling = false,
                        DefaultShipping = false,
                        IsResidential = false,
                        Label = "Term Time",
                        Address = new NetSuiteAddress
                        {
                            Addr1 = cus.Address1TermTime,
                            Addr2 = cus.Address2TermTime,
                            Addressee = $"{cus.Forename} {cus.Surname}",
                            City = cus.Address3TermTime,
                            Country = new NetSuiteAddressCountry
                            {
                                RefName = cus.CountryNameTermTime
                            },
                            Override = false,
                            Zip = cus.PostCodeTermTime
                        }
                    } : new NetSuiteAddressBook(),
                    cus.PostCodeHome != null ? new NetSuiteAddressBook
                    {
                        DefaultBilling = false,
                        DefaultShipping = false,
                        IsResidential = false,
                        Label = "Home",
                        Address = new NetSuiteAddress
                        {
                            Addr1 = cus.Address1Home,
                            Addr2 = cus.Address2Home,
                            Addressee = $"{cus.Forename} {cus.Surname}",
                            City = cus.Address3Home,
                            Country = new NetSuiteAddressCountry
                            {
                                RefName = cus.CountryNameHome
                            },
                            Override = false,
                            Zip = cus.PostCodeHome
                        }
                    } : new NetSuiteAddressBook(),
                    cus.PostCodeInvoice != null ? new NetSuiteAddressBook
                    {
                        DefaultBilling = false,
                        DefaultShipping = false,
                        IsResidential = false,
                        Label = "Invoice",
                        Address = new NetSuiteAddress
                        {
                            Addr1 = cus.Address1Invoice,
                            Addr2 = cus.Address2Invoice,
                            Addressee = $"{cus.Forename} {cus.Surname}",
                            City = cus.Address3Invoice,
                            Country = new NetSuiteAddressCountry
                            {
                                RefName = cus.CountryNameInvoice
                            },
                            Override = false,
                            Zip = cus.PostCodeInvoice
                        }
                    } : new NetSuiteAddressBook(),
                }
            }).ToList<NetSuiteCustomer>();

            return netSuiteCustomers ?? new List<NetSuiteCustomer>();
        }

        public NetSuiteSearchParameter AddSearchParameter(Operand? operand, string? fieldName, Operator? op, string? value)
        {
            NetSuiteSearchParameter param = new NetSuiteSearchParameter
            {
                Operand = null,
                FieldName = fieldName,
                Operator = op,
                Value = value,
                IncludeOpeningParenthesis = false,
                IncludeClosingParenthesis = false
            };
            return param;
        }

        public NetSuiteSearchParameter AddSearchParameter(Operand? operand, string? fieldName, Operator? op, string? value, bool? IncludeOpeningParenthesis, bool? IncludeClosingParenthesis)
        {
            NetSuiteSearchParameter param = new NetSuiteSearchParameter
            {
                Operand = null,
                FieldName = fieldName,
                Operator = op,
                Value = value,
                IncludeOpeningParenthesis = false,
                IncludeClosingParenthesis = false
            };
            return param;
        }

        public async Task<NetSuiteCustomer> GetCustomer(NetSuiteCustomer netSuiteCustomer)
        {
            NetSuiteCustomer? matchedCustomer = new NetSuiteCustomer();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();

            //Check if the customer already exists in NetSuite by their Student Ref
            if (matchedCustomer?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddSearchParameter(null, "custentityclient_studentno", Operator.IS, netSuiteCustomer?.CustEntityClientStudentNo);
                searchParameters.Add(param);

                matchedCustomer = await FindCustomer(searchParameters, RecordMatchType.ByStudentRef);
            }

            //If unsuccessful check if the customer already exists in NetSuite by their ERP ID
            if (matchedCustomer?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddSearchParameter(null, "custentity_crm_applicantid", Operator.IS, netSuiteCustomer?.CustEntityCRMApplicantID);
                searchParameters.Add(param);

                matchedCustomer = await FindCustomer(searchParameters, RecordMatchType.ByERPID);
            }

            //If unsuccessful then attempt to match on name/email/mobile if all have values
            if (matchedCustomer?.ID == null && !(string.IsNullOrEmpty(netSuiteCustomer?.FirstName) || string.IsNullOrEmpty(netSuiteCustomer?.LastName) || string.IsNullOrEmpty(netSuiteCustomer?.Email) || string.IsNullOrEmpty(netSuiteCustomer?.Phone)))
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddSearchParameter(null, "firstName", Operator.IS, netSuiteCustomer?.FirstName);
                searchParameters.Add(param);

                param = AddSearchParameter(Operand.AND, "lastName", Operator.IS, netSuiteCustomer?.LastName);
                searchParameters.Add(param);

                param = AddSearchParameter(Operand.AND, "email", Operator.IS, netSuiteCustomer?.Email);
                searchParameters.Add(param);

                param = AddSearchParameter(Operand.AND, "phone", Operator.IS, netSuiteCustomer?.Phone);
                searchParameters.Add(param);

                matchedCustomer = await FindCustomer(searchParameters, RecordMatchType.ByPersonalDetails);
            }

            return matchedCustomer ?? new NetSuiteCustomer();
        }

        public async Task<NetSuiteCustomer> FindCustomer(IList<NetSuiteSearchParameter>? searchParameters, RecordMatchType recordMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteCustomer? matchedCustomer = new NetSuiteCustomer();

            try
            {
                //Perform the search
                searchResults = await _netsuite?.Search<NetSuiteSearchResult>("customer", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    matchedCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCustomer != null)
                {
                    matchedCustomer.RecordMatchType = recordMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindCustomer: {ex.Message}");
                matchedCustomer = null;
            }

            return matchedCustomer ?? new NetSuiteCustomer();
        }

        public async Task<NetSuiteCustomer> UpdateCustomer(NetSuiteCustomer netSuiteCustomer, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteCustomer?.ID ?? "0") > 0)
                    {
                        NetSuiteCustomer? updatedNetSuiteCustomer = await _netsuite.Update<NetSuiteCustomer>("customer", int.Parse(netSuiteCustomer?.ID ?? "0"), netSuiteCustomer);

                        if (updatedNetSuiteCustomer != null)
                            updatedNetSuiteCustomer.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Customer: {updatedNetSuiteCustomer?.ID}");
                        return updatedNetSuiteCustomer ?? new NetSuiteCustomer();
                    }
                    else
                    {
                        NetSuiteCustomer? insertedNetSuiteCustomer = await _netsuite.Add<NetSuiteCustomer>("customer", netSuiteCustomer);

                        if (insertedNetSuiteCustomer != null)
                            insertedNetSuiteCustomer.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Customer: {insertedNetSuiteCustomer?.ID}");
                        return insertedNetSuiteCustomer ?? new NetSuiteCustomer();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateCustomer: {ex.Message}");
                    return netSuiteCustomer ?? new NetSuiteCustomer();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Customer");
                return netSuiteCustomer ?? new NetSuiteCustomer();
            }
        }

        public async Task<IList<NetSuiteAddressBook>> GetAddresses(NetSuiteCustomer netSuiteCustomer)
        {
            NetSuiteSearchResult? netSuiteSearchResult = new NetSuiteSearchResult();
            IList<NetSuiteAddressBook>? netSuiteAddressBookEntries = new List<NetSuiteAddressBook>();

            if (netSuiteCustomer?.ID != null)
            {
                //Get the addresses for this customer
                netSuiteSearchResult = await _netsuite.GetAll<NetSuiteSearchResult>($"customer/{netSuiteCustomer?.ID}/addressBook");
                //_log?.Information($"Found {netSuiteSearchResult?.TotalResults} Addresses for Customer: {netSuiteCustomer?.ID} in NetSuite");
            }
            else
            {
                //_log?.Information($"No Addresses Found for Customer ID: {netSuiteCustomer?.ID}");
            }

            if (netSuiteSearchResult != null && netSuiteSearchResult?.Items != null)
            {
                try
                {
                    foreach (NetSuiteSearchResultItem? address in netSuiteSearchResult.Items)
                    {
                        if (address != null)
                        {
                            NetSuiteAddressBook? netSuiteAddressBook = await _netsuite.Get<NetSuiteAddressBook>($"customer/{netSuiteCustomer?.ID}/addressBook", address?.IDFromIDAndLink ?? 0);
                            NetSuiteAddress? netSuiteAddress = await _netsuite.GetAll<NetSuiteAddress>($"customer/{netSuiteCustomer?.ID}/addressBook/{address?.IDFromIDAndLink}/addressBookAddress");
                            if (netSuiteAddressBook != null && netSuiteAddress != null)
                            {
                                netSuiteAddressBook.Address = netSuiteAddress;
                                netSuiteAddressBookEntries.Add(netSuiteAddressBook);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in GetAddresses: {ex.Message}");
                    netSuiteAddressBookEntries = null;
                }
            }

            return netSuiteAddressBookEntries ?? new List<NetSuiteAddressBook>();
        }

        public ICollection<NetSuiteAddressBook> CheckAddresses(NetSuiteCustomer netSuiteCustomer, NetSuiteCustomer? matchedCustomer)
        {
            int? numUniteAddresses = netSuiteCustomer?.Addresses?.Where(a => a.Label != null).DistinctBy(a => a.Label).ToList().Count ?? 0;

            if (netSuiteCustomer != null && netSuiteCustomer.Addresses != null)
            {
                int? addressNum = 0;
                foreach (NetSuiteAddressBook? address in netSuiteCustomer!.Addresses)
                {
                    if (address != null)
                    {
                        //If the address type is null then skip it as it is not a valid address
                        if (address.Label == null)
                            continue;

                        //If the address type is the same as the default billing address type in UNIT-e then skip it as it is the same address
                        if (address.Label == netSuiteCustomer?.Addresses?.Where(a => a.DefaultBilling == true).FirstOrDefault()?.Label
                            && address.DefaultBilling != true)
                            continue;

                        addressNum++;

                        if (matchedCustomer != null)
                        {
                            foreach (NetSuiteAddressBook? matchedAddress in matchedCustomer!.Addresses)
                            {
                                if (matchedAddress != null)
                                {
                                    //Check if the address exists and has not already been matched to an existing record
                                    if (address?.Address?.Zip == matchedAddress?.Address?.Zip
                                        && netSuiteCustomer?.Addresses.Any(a => a?.AddressID == matchedAddress?.AddressID) == false)
                                    {
                                        //Update the ID of the record
                                        address!.AddressID = matchedAddress?.AddressID;

                                        //Check if the right address is the default
                                        if (address?.DefaultBilling == true
                                            && matchedAddress?.DefaultBilling == true
                                            && matchedAddress?.DefaultShipping == true
                                            && matchedAddress?.IsResidential == true)
                                        {
                                            //Address is the default in NetSuite too so no action needed
                                            address!.AddressID = matchedAddress?.AddressID;
                                            address.RecordActionType = RecordActionType.None;
                                            _log?.Information($"Address {addressNum} of {numUniteAddresses}: Default {address.Label} address at {address?.Address?.Zip} found in NetSuite with ID: {matchedAddress?.AddressID} and is default");
                                        }
                                        else if (address?.DefaultBilling == true)
                                        {
                                            address!.AddressID = matchedAddress?.AddressID;
                                            address.RecordActionType = RecordActionType.Update;
                                            _log?.Information($"Address {addressNum} of {numUniteAddresses}: Default {address.Label} address at {address?.Address?.Zip} found in NetSuite with ID: {matchedAddress?.AddressID} but is not default or not set as residential");
                                        }
                                        else
                                        {
                                            address!.AddressID = matchedAddress?.AddressID;
                                            address.RecordActionType = RecordActionType.Update;
                                            _log?.Information($"Address {addressNum} of {numUniteAddresses}: Additional {address.Label} address at {address?.Address?.Zip} found in NetSuite with ID: {matchedAddress?.AddressID}");
                                        }
                                    }
                                    else
                                    {
                                        address!.RecordActionType = RecordActionType.Insert;
                                        _log?.Information($"Address {addressNum} of {numUniteAddresses}: {address.Label} address at {address?.Address?.Zip} not found in NetSuite so need to add");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //This would be for new customers
                            address!.RecordActionType = RecordActionType.Insert;
                            _log?.Information($"Address {addressNum} of {numUniteAddresses}: {address.Label} address at {address?.Address?.Zip} not found in NetSuite so need to add");
                        }
                        
                    }
                }
            }

            return netSuiteCustomer?.Addresses ?? new List<NetSuiteAddressBook>();
        }

        public async Task<bool?> UpdateAddresses(NetSuiteCustomer netSuiteCustomer, bool? readOnly)
        {
            bool? isOK = true;
            if (readOnly != true)
            {
                if (netSuiteCustomer?.Addresses != null && _netsuite != null)
                {
                    try
                    {
                        foreach (NetSuiteAddressBook? address in netSuiteCustomer.Addresses)
                        {
                            if (address != null)
                            {
                                //If the address is not null and has an ID then update it
                                if (address.AddressID != null && address.RecordActionType == RecordActionType.Update)
                                {
                                    NetSuiteAddressBook? updatedAddress = await _netsuite.Update<NetSuiteAddressBook>($"customer/{netSuiteCustomer?.ID}/addressBook", int.Parse(address.AddressID ?? "0"), address);
                                    _log?.Information($"Updated Address {updatedAddress?.AddressID} for Customer {netSuiteCustomer?.ID}");
                                }
                                else if (address.RecordActionType == RecordActionType.Insert)
                                {
                                    NetSuiteAddressBook? insertedAddress = await _netsuite.Add<NetSuiteAddressBook>($"customer/{netSuiteCustomer?.ID}/addressBook", address);
                                    _log?.Information($"Inserted Address {insertedAddress?.AddressID} for Customer {netSuiteCustomer?.ID}");
                                }
                                else if (address.RecordActionType == RecordActionType.None)
                                {
                                    _log?.Information($"No Changes Made to Address {address?.AddressID} for Customer {netSuiteCustomer?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"Error determining action for Address {address?.AddressID} for Customer {netSuiteCustomer?.ID}");
                                    isOK = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log?.Error($"Error updating addresses: {ex.Message}");
                        isOK = false;
                    }

                }
            }

            return isOK;
        }
    }
}
