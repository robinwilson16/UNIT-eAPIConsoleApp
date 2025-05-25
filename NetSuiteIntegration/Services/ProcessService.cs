using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Shared;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Services
{
    public class ProcessService(ISRSWebServicecs unite, IFinanceWebService netsuite, ILogger logger) : IProcessService
    {
        ISRSWebServicecs? _unite = unite;
        IFinanceWebService? _netsuite = netsuite;
        ILogger? _log = logger;
        string? _divider = new string('#', 20);

        public async Task<bool?> Process(ICollection<UNITeRepGen>? repGens, bool? readOnly, bool? firstRecordOnly)
        {
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

            if (repGens == null || repGens.Count == 0)
            {
                _log?.Error("List of RepGen Reports is null/empty. This should reference the RepGen Reports used to extract the data from UNIT-e.");
                return false;
            }

            if (readOnly == true)
                _log?.Information("** Running in Read Only Mode. No changes will be made to NetSuite. **");

            if (firstRecordOnly == true)
                _log?.Information("** Running in First Record Only Mode. Only the first record will be processed (helpful for faster debugging). **");

            bool? isOK = true;

            //Process the data
            isOK = await DoImport(repGens, readOnly,firstRecordOnly);

            return isOK;
        }

        public async Task<bool?> Testing()
        {
            //Test function to use to test getting, updating, inserting and deleting a single NetSuite Customer

            NetSuiteCustomer? existingNetSuiteCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", 5753);
            _log?.Information($"\nNetSuite Customer: {existingNetSuiteCustomer?.EntityID} - {existingNetSuiteCustomer?.FirstName} {existingNetSuiteCustomer?.LastName}");

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

            return true;
        }

        public async Task<bool?> DoImport(ICollection<UNITeRepGen>? repGens, bool? readOnly, bool? firstRecordOnly)
        {
            bool? isOK = true;

            //Get RepGens used to extract the data from UNIT-e
            string? enrolmentRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Enrolment)?.Reference;
            string? courseRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Course)?.Reference;
            string? feeRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Fee)?.Reference;
            string? refundRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Refund)?.Reference;

            //Process the data
            isOK = await ProcessEnrolments(enrolmentRepGen, readOnly, firstRecordOnly);
            isOK = isOK == true ? await ProcessCourses(courseRepGen, readOnly, firstRecordOnly) : isOK;
            isOK = isOK == true ? await ProcessFees(feeRepGen, readOnly, firstRecordOnly) : isOK;
            isOK = isOK == true ? await ProcessRefunds(refundRepGen, readOnly, firstRecordOnly) : isOK;

            return isOK;
        }

        public async Task<bool?> ProcessEnrolments(string? enrolmentRepGen, bool? readOnly, bool? firstRecordOnly)
        {
            _log?.Information($"\n{_divider}");

            if (enrolmentRepGen != null && enrolmentRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Enrolments using UNIT-e RepGen Report: \"{enrolmentRepGen}\"");
            }
            else
            {
                _log?.Error("Enrolment RepGen is null/empty. Skipping Enrolment Import");
                return true;
            }

            bool? isOK = true;

            IList<UNITeStudent>? uniteStudents = new List<UNITeStudent>();
            IList<UNITeEnrolment>? uniteEnrolments = new List<UNITeEnrolment>();
            IList<NetSuiteCustomer>? uniteNetSuiteCustomers = new List<NetSuiteCustomer>();

            try
            {
                if (_unite != null)
                    uniteEnrolments = await _unite.ExportReport<List<UNITeEnrolment>>(enrolmentRepGen ?? "");

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

                    if (uniteNetSuiteCustomers != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteCustomer? uniteNetSuiteCustomer in uniteNetSuiteCustomers!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteCustomers.Count}: Searching for {uniteNetSuiteCustomer?.LastName}, {uniteNetSuiteCustomer?.FirstName} ({uniteNetSuiteCustomer?.CustEntityClientStudentNo}) in NetSuite");

                            #region Find Customer and Related Datasets
                            //Find this customer in NetSuite
                            NetSuiteCustomer? matchedCustomer = await GetNetSuiteCustomer(uniteNetSuiteCustomer ?? new NetSuiteCustomer());

                            if (matchedCustomer?.CustomerMatchType == CustomerMatchType.ByStudentRef)
                                _log?.Information($"Customer Found in NetSuite by Student Ref with NetSuite Customer ID: {matchedCustomer?.ID}");
                            else if (matchedCustomer?.CustomerMatchType == CustomerMatchType.ByERPID)
                                _log?.Information($"Customer Found in NetSuite by ERP ID with NetSuite Customer ID: {matchedCustomer?.ID}");
                            else if (matchedCustomer?.CustomerMatchType == CustomerMatchType.ByPersonalDetails)
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
                                    matchedCustomer.Addresses = await GetNetSuiteAddresses(matchedCustomer);

                                _log?.Information($"\nFound {numUniteAddresses} addresses for customer in UNIT-e");
                                _log?.Information($"Found {matchedCustomer?.Addresses?.Count ?? 0} addresses for customer in NetSuite");

                                if (matchedCustomer?.Addresses?.Count > 0)
                                {

                                }
                            }

                            //_log?.Information($"Main Address Type {uniteNetSuiteCustomer?.Addresses?.Where(a => a.DefaultBilling == true).FirstOrDefault()?.Label}");

                            //Check if addresses are up to date and flag each one for insert or update
                            if (uniteNetSuiteCustomer != null)
                                uniteNetSuiteCustomer.Addresses = CheckNetSuiteAddresses(uniteNetSuiteCustomer, matchedCustomer);

                            #endregion

                            #region Perform Updates to NetSuite Customer
                            //Update or add the Customer record in NetSuite
                            NetSuiteCustomer updatedCustomer = await UpdateNetSuiteCustomer(uniteNetSuiteCustomer ?? new NetSuiteCustomer(), readOnly);

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
                                addressesUpdated = await UpdateNetSuiteAddresses(uniteNetSuiteCustomer, readOnly);

                            #endregion

                            if (firstRecordOnly == true)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Enrolments: {ex.Message}");
                isOK = false;
            }

            return isOK;
        }

        public async Task<bool?> ProcessCourses(string? courseRepGen, bool? readOnly, bool? firstRecordOnly)
        {
            _log?.Information($"\n{_divider}");

            if (courseRepGen != null && courseRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Courses using UNIT-e RepGen Report: \"{courseRepGen}\"");
            }
            else
            {
                _log?.Error("Course RepGen is null/empty. Skipping Course Import");
                return true;
            }

            bool? isOK = true;

            IList<UNITeCourse>? uniteCourses = new List<UNITeCourse>();
            IList<NetSuiteNonInventorySaleItem>? uniteNetSuiteNonInventorySaleItems = new List<NetSuiteNonInventorySaleItem>();

            try
            {
                if (_unite != null)
                    uniteCourses = await _unite.ExportReport<List<UNITeCourse>>(courseRepGen ?? "");

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

                    if (uniteCourses != null)
                        uniteNetSuiteNonInventorySaleItems = MapUNITeCoursesToNetSuiteNonInventorySaleItems(uniteCourses);

                    if (uniteNetSuiteNonInventorySaleItems != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteNonInventorySaleItem? saleItem in uniteNetSuiteNonInventorySaleItems!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteNonInventorySaleItems.Count}: Searching for {saleItem?.ItemID} - {saleItem?.DisplayName} in NetSuite");

                            #region Find Non-Inventory Sale Item
                            //Find this course (non-inventory sale item) in NetSuite
                            NetSuiteNonInventorySaleItem? matchedSaleItem = await GetNetSuiteNonInventorySaleItem(saleItem ?? new NetSuiteNonInventorySaleItem());

                            if (matchedSaleItem?.NonInventorySaleItemMatchType == NonInventorySaleItemMatchType.ByCourseCode)
                                _log?.Information($"Course Found in NetSuite by Course Code with NetSuite Non-Inventory Sale Item ID: {matchedSaleItem?.ID}");
                            else
                                _log?.Information($"Course Not Found in NetSuite");

                            if (matchedSaleItem?.ID != null)
                            {
                                //Update the ID of the record that came from UNIT-e so it can be used to update the record in NetSuite
                                if (saleItem != null)
                                    saleItem.ID = matchedSaleItem?.ID;
                            }
                            #endregion

                            #region Perform Updates to NetSuite Non-Inventory Sale Item
                            //Update or add the Non-Inventory Sale record in NetSuite
                            NetSuiteNonInventorySaleItem updatedNetSuiteNonInventorySaleItem = await UpdateNetSuiteNonInventorySaleItem(saleItem ?? new NetSuiteNonInventorySaleItem(), readOnly);

                            //Update the ID of the record that came from UNIT-e so it matches the newly inserted record if not updating an existing NetSuite record
                            if (saleItem != null)
                            {
                                if (updatedNetSuiteNonInventorySaleItem?.RecordActionType == RecordActionType.Insert)
                                {
                                    //Add the ID of the newly inserted record to the UNIT-e record
                                    saleItem.ID = updatedNetSuiteNonInventorySaleItem?.ID;
                                    _log?.Information($"Inserted New NetSuite Non-Inventory Sale Item: {saleItem?.ID}");
                                }
                                else if (updatedNetSuiteNonInventorySaleItem?.RecordActionType == RecordActionType.Update)
                                {
                                    _log?.Information($"Synced Existing NetSuite Non-Inventory Sale Item: {saleItem?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"No Changes Made to NetSuite Non-Inventory Sale Item: {saleItem?.ID}");
                                }
                            }
                            #endregion

                            if (firstRecordOnly == true)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Courses: {ex.Message}");
                return false;
            }

            return isOK;
        }

        public async Task<bool?> ProcessFees(string? feeRepGen, bool? readOnly, bool? firstRecordOnly)
        {
            bool? isOK = true;

            return isOK;
        }

        public async Task<bool?> ProcessRefunds(string? refundRepGen, bool? readOnly, bool? firstRecordOnly)
        {
            bool? isOK = true;

            return isOK;
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

        public IList<NetSuiteNonInventorySaleItem> MapUNITeCoursesToNetSuiteNonInventorySaleItems(IList<UNITeCourse> uniteCourses)
        {
            IList<NetSuiteNonInventorySaleItem>? netSuiteNonInventorySaleItems = new List<NetSuiteNonInventorySaleItem>();

            //Map UNIT-e Courses to NetSuite Non-Inventory Sale Items
            netSuiteNonInventorySaleItems = uniteCourses?.Select(crs => new NetSuiteNonInventorySaleItem
            {
                Class = new NetSuiteNonInventorySaleItemClass
                {
                    RefName = crs.CampusName
                },
                CreatedDate = DateTime.Now,
                CustItem1 = crs.StartDate?.Format("yyyy-MM-dd"),
                CustItem2 = crs.EndDate?.Format("yyyy-MM-dd"),
                CustItemIsPOItem = false,
                CustomForm = new NetSuiteNonInventorySaleItemCustomForm
                {
                    RefName = crs.SubjectName
                },
                DeferredRevenueAccount = new NetSuiteNonInventorySaleItemDeferredRevenueAccount
                {
                    ID = "216",
                    RefName = "30602 Deferred Revenue"
                },
                Department = new NetSuiteNonInventorySaleItemDepartment
                {
                    ID = "26",
                    RefName = "Income : Student Income"
                },
                DirectRevenuePosting = false,
                DisplayName = crs.CourseTitle,
                EnforceMinQTYInternally = true,
                ExternalID = $"CRM_{crs.CourseCode?.Replace("/", "_")}",
                IncludeChildren = false,
                IncomeAccount = new NetSuiteNonInventorySaleItemIncomeAccount
                {
                    ID = "1233",
                    RefName = "50120 Total Net Income : Total Fee Income : Fee Income - Postgrad"
                },
                IsFulfillable = false,
                IsGCOCompliant = false,
                IsInactive = false,
                IsOnline = false,
                ItemID = crs.CourseCode,
                Location = new NetSuiteNonInventorySaleItemLocation
                {
                    RefName = crs.CampusName
                },
                RevenueRecognitionRule = new NetSuiteNonInventorySaleItemRevenueRecognitionRule
                {
                    ID = "4",
                    RefName = "BIMM - Straight Line, Exact Days"
                },
                RevRecForecastRule = new NetSuiteNonInventorySaleItemRevRecForecastRule
                {
                    ID = "4",
                    RefName = "BIMM - Straight Line, Exact Days"
                },
                SalesDescription = crs.CourseTitle,
                UseMarginalRates = false,
                VSOEDelivered = false,
                VSOESopGroup = new NetSuiteNonInventorySaleItemVSOESopGroup
                {
                    ID = "NORMAL",
                    RefName = "Normal"
                },
            }).ToList<NetSuiteNonInventorySaleItem>();

            return netSuiteNonInventorySaleItems ?? new List<NetSuiteNonInventorySaleItem>();
        }

        public NetSuiteSearchParameter AddNetSuiteSearchParameter(Operand? operand, string? fieldName, Operator? op, string? value)
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

        public NetSuiteSearchParameter AddNetSuiteSearchParameter(Operand? operand, string? fieldName, Operator? op, string? value, bool? IncludeOpeningParenthesis, bool? IncludeClosingParenthesis)
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

        public async Task<NetSuiteCustomer> GetNetSuiteCustomer(NetSuiteCustomer netSuiteCustomer)
        {
            NetSuiteCustomer? matchedCustomer = new NetSuiteCustomer();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();

            //Check if the customer already exists in NetSuite by their Student Ref
            if (matchedCustomer?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddNetSuiteSearchParameter(null, "custentityclient_studentno", Operator.IS, netSuiteCustomer?.CustEntityClientStudentNo);
                searchParameters.Add(param);

                matchedCustomer = await FindNetSuiteCustomer(searchParameters, CustomerMatchType.ByStudentRef);
            }

            //If unsuccessful check if the customer already exists in NetSuite by their ERP ID
            if (matchedCustomer?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddNetSuiteSearchParameter(null, "custentity_crm_applicantid", Operator.IS, netSuiteCustomer?.CustEntityCRMApplicantID);
                searchParameters.Add(param);

                matchedCustomer = await FindNetSuiteCustomer(searchParameters, CustomerMatchType.ByERPID);
            }

            //If unsuccessful then attempt to match on name/email/mobile if all have values
            if (matchedCustomer?.ID == null && !(string.IsNullOrEmpty(netSuiteCustomer?.FirstName) || string.IsNullOrEmpty(netSuiteCustomer?.LastName) || string.IsNullOrEmpty(netSuiteCustomer?.Email) || string.IsNullOrEmpty(netSuiteCustomer?.Phone)))
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddNetSuiteSearchParameter(null, "firstName", Operator.IS, netSuiteCustomer?.FirstName);
                searchParameters.Add(param);

                param = AddNetSuiteSearchParameter(Operand.AND, "lastName", Operator.IS, netSuiteCustomer?.LastName);
                searchParameters.Add(param);

                param = AddNetSuiteSearchParameter(Operand.AND, "email", Operator.IS, netSuiteCustomer?.Email);
                searchParameters.Add(param);

                param = AddNetSuiteSearchParameter(Operand.AND, "phone", Operator.IS, netSuiteCustomer?.Phone);
                searchParameters.Add(param);

                matchedCustomer = await FindNetSuiteCustomer(searchParameters, CustomerMatchType.ByPersonalDetails);
            }

            if (matchedCustomer?.ID == null)
            {
                //If no match found then create a new customer
                matchedCustomer = new NetSuiteCustomer();
                matchedCustomer.CustomerMatchType = CustomerMatchType.NotFound;
            }

            return matchedCustomer ?? new NetSuiteCustomer();
        }

        public async Task<NetSuiteCustomer> FindNetSuiteCustomer(IList<NetSuiteSearchParameter>? searchParameters, CustomerMatchType customerMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteCustomer? matchedCustomer = new NetSuiteCustomer();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("customer", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCustomer != null)
                {
                    matchedCustomer.CustomerMatchType = customerMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteCustomer: {ex.Message}");
                matchedCustomer = null;
            }

            return matchedCustomer ?? new NetSuiteCustomer();
        }

        public async Task<NetSuiteCustomer> UpdateNetSuiteCustomer(NetSuiteCustomer netSuiteCustomer, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteCustomer?.ID ?? "0") > 0)
                    {
                        NetSuiteCustomer? updatedNetSuiteCustomer = new NetSuiteCustomer();
                        if (_netsuite != null)
                            updatedNetSuiteCustomer = await _netsuite.Update<NetSuiteCustomer>("customer", int.Parse(netSuiteCustomer?.ID ?? "0"), netSuiteCustomer);

                        if (updatedNetSuiteCustomer != null)
                            updatedNetSuiteCustomer.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Customer: {updatedNetSuiteCustomer?.ID}");
                        return updatedNetSuiteCustomer ?? new NetSuiteCustomer();
                    }
                    else
                    {
                        NetSuiteCustomer? insertedNetSuiteCustomer = new NetSuiteCustomer();
                        if (_netsuite != null)
                            insertedNetSuiteCustomer = await _netsuite.Add<NetSuiteCustomer>("customer", netSuiteCustomer);

                        if (insertedNetSuiteCustomer != null)
                            insertedNetSuiteCustomer.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Customer: {insertedNetSuiteCustomer?.ID}");
                        return insertedNetSuiteCustomer ?? new NetSuiteCustomer();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateNetSuiteCustomer: {ex.Message}");
                    netSuiteCustomer.RecordActionType = RecordActionType.None;
                    return netSuiteCustomer ?? new NetSuiteCustomer();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Customer");
                netSuiteCustomer.RecordActionType = RecordActionType.None;
                return netSuiteCustomer ?? new NetSuiteCustomer();
            }
        }

        public async Task<IList<NetSuiteAddressBook>> GetNetSuiteAddresses(NetSuiteCustomer netSuiteCustomer)
        {
            NetSuiteSearchResult? netSuiteSearchResult = new NetSuiteSearchResult();
            IList<NetSuiteAddressBook>? netSuiteAddressBookEntries = new List<NetSuiteAddressBook>();

            if (netSuiteCustomer?.ID != null)
            {
                //Get the addresses for this customer
                if (_netsuite != null)
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
                            NetSuiteAddressBook? netSuiteAddressBook = new NetSuiteAddressBook();
                            NetSuiteAddress? netSuiteAddress = new NetSuiteAddress();
                            if (_netsuite != null)
                            {
                                netSuiteAddressBook = await _netsuite.Get<NetSuiteAddressBook>($"customer/{netSuiteCustomer?.ID}/addressBook", address?.IDFromIDAndLink ?? 0);
                                netSuiteAddress = await _netsuite.GetAll<NetSuiteAddress>($"customer/{netSuiteCustomer?.ID}/addressBook/{address?.IDFromIDAndLink}/addressBookAddress");
                            }
                                
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
                    _log?.Error($"Error in GetNetSuiteAddresses: {ex.Message}");
                    netSuiteAddressBookEntries = null;
                }
            }

            return netSuiteAddressBookEntries ?? new List<NetSuiteAddressBook>();
        }

        public ICollection<NetSuiteAddressBook> CheckNetSuiteAddresses(NetSuiteCustomer netSuiteCustomer, NetSuiteCustomer? matchedCustomer)
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

                        if (matchedCustomer != null && matchedCustomer.Addresses != null)
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

        public async Task<bool?> UpdateNetSuiteAddresses(NetSuiteCustomer netSuiteCustomer, bool? readOnly)
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

        public async Task<NetSuiteNonInventorySaleItem> GetNetSuiteNonInventorySaleItem(NetSuiteNonInventorySaleItem netSuiteNonInventorySaleItem)
        {
            NetSuiteNonInventorySaleItem? matchedSaleItem = new NetSuiteNonInventorySaleItem();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();

            //Check if the course already exists in NetSuite by the course code
            if (matchedSaleItem?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddNetSuiteSearchParameter(null, "itemID", Operator.IS, netSuiteNonInventorySaleItem?.ItemID);
                searchParameters.Add(param);

                matchedSaleItem = await FindNetSuiteNonInventorySaleItem(searchParameters, NonInventorySaleItemMatchType.ByCourseCode);
            }

            if (matchedSaleItem?.ID == null)
            {
                //If no match found then create a new customer
                matchedSaleItem = new NetSuiteNonInventorySaleItem();
                matchedSaleItem.NonInventorySaleItemMatchType = NonInventorySaleItemMatchType.NotFound;
            }

            return matchedSaleItem ?? new NetSuiteNonInventorySaleItem();
        }

        public async Task<NetSuiteNonInventorySaleItem> FindNetSuiteNonInventorySaleItem(IList<NetSuiteSearchParameter>? searchParameters, NonInventorySaleItemMatchType nonInventorySaleItemMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteNonInventorySaleItem? matchedSaleItem = new NetSuiteNonInventorySaleItem();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();
                
                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("nonInventorySaleItem", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedSaleItem = await _netsuite.Get<NetSuiteNonInventorySaleItem>("nonInventorySaleItem", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedSaleItem != null)
                {
                    matchedSaleItem.NonInventorySaleItemMatchType = nonInventorySaleItemMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteNonInventorySaleItem: {ex.Message}");
                matchedSaleItem = null;
            }

            return matchedSaleItem ?? new NetSuiteNonInventorySaleItem();
        }

        public async Task<NetSuiteNonInventorySaleItem> UpdateNetSuiteNonInventorySaleItem(NetSuiteNonInventorySaleItem netSuiteNonInventorySaleItem, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteNonInventorySaleItem?.ID ?? "0") > 0)
                    {
                        NetSuiteNonInventorySaleItem? updatedNetSuiteNonInventorySaleItem = new NetSuiteNonInventorySaleItem();
                        if (_netsuite != null)
                            updatedNetSuiteNonInventorySaleItem = await _netsuite.Update<NetSuiteNonInventorySaleItem>("nonInventorySaleItem", int.Parse(netSuiteNonInventorySaleItem?.ID ?? "0"), netSuiteNonInventorySaleItem);

                        if (updatedNetSuiteNonInventorySaleItem != null)
                            updatedNetSuiteNonInventorySaleItem.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Non-Inventory Sale Item: {updatedNetSuiteNonInventorySaleItem?.ID}");
                        return updatedNetSuiteNonInventorySaleItem ?? new NetSuiteNonInventorySaleItem();
                    }
                    else
                    {
                        NetSuiteNonInventorySaleItem? insertedNetSuiteNonInventorySaleItem = new NetSuiteNonInventorySaleItem();
                        if (_netsuite != null)
                            insertedNetSuiteNonInventorySaleItem = await _netsuite.Add<NetSuiteNonInventorySaleItem>("nonInventorySaleItem", netSuiteNonInventorySaleItem);

                        if (insertedNetSuiteNonInventorySaleItem != null)
                            insertedNetSuiteNonInventorySaleItem.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Non-Inventory Sale Item: {insertedNetSuiteNonInventorySaleItem?.ID}");
                        return insertedNetSuiteNonInventorySaleItem ?? new NetSuiteNonInventorySaleItem();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateNetSuiteNonInventorySaleItem: {ex.Message}");
                    netSuiteNonInventorySaleItem.RecordActionType = RecordActionType.None;
                    return netSuiteNonInventorySaleItem ?? new NetSuiteNonInventorySaleItem();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Non-Inventory Sale Item");
                netSuiteNonInventorySaleItem.RecordActionType = RecordActionType.None;
                return netSuiteNonInventorySaleItem ?? new NetSuiteNonInventorySaleItem();
            }
        }
    }
}
