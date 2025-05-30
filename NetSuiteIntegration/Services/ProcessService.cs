using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Shared;
using Serilog;
using UNITe.Business.Helper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Services
{
    public class ProcessService(NetsuiteContext dbContext, ISRSWebServicecs unite, IFinanceWebService netsuite, ILogger logger) : IProcessService
    {
        NetsuiteContext _dbContext = dbContext;
        ISRSWebServicecs? _unite = unite;
        IFinanceWebService? _netsuite = netsuite;
        ILogger? _log = logger;
        string? _divider = new string('#', 20);

        public async Task<bool?> Process(ICollection<UNITeRepGen>? repGens, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
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
            isOK = await DoImport(repGens, readOnly,firstRecordOnly, forceInsertCustomer);

            return isOK;
        }

        public async Task<bool?> Testing()
        {
            //Test function to use to test getting, updating, inserting and deleting a single NetSuite Customer
            NetSuiteCustomer? existingNetSuiteCustomer = new NetSuiteCustomer();

            if (_netsuite != null)
            {
                existingNetSuiteCustomer = await _netsuite.Get<NetSuiteCustomer>("customer", 5753);
                _log?.Information($"\nNetSuite Customer: {existingNetSuiteCustomer?.EntityID} - {existingNetSuiteCustomer?.FirstName} {existingNetSuiteCustomer?.LastName}");
            }

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

        public async Task<bool?> DoImport(ICollection<UNITeRepGen>? repGens, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            bool? studentOK = true;
            bool? courseOK = true;

            //Get RepGens used to extract the data from UNIT-e
            string? enrolmentRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Enrolment)?.Reference;
            string? courseRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Course)?.Reference;
            string? feeRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Fee)?.Reference;
            string? creditNoteRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.CreditNote)?.Reference;
            string? refundRepGen = repGens?.FirstOrDefault(rg => rg.Type == UNITeRepGenType.Refund)?.Reference;

            //Process the data
            ICollection<NetSuiteCustomer>? uniteNetSuiteCustomers = await ProcessEnrolments(enrolmentRepGen, readOnly, firstRecordOnly, forceInsertCustomer);
            ICollection<NetSuiteNonInventorySaleItem>? uniteNetSuiteNonInventorySaleItems = await ProcessCourses(courseRepGen, readOnly, firstRecordOnly, forceInsertCustomer);

            //If empty lists are returned then there has been an error as there should always be students and courses in scope
            if (uniteNetSuiteCustomers == null || uniteNetSuiteCustomers.Count == 0)
            {
                studentOK = false;
            }
            if (uniteNetSuiteNonInventorySaleItems == null || uniteNetSuiteNonInventorySaleItems.Count == 0)
            {
                courseOK = false;
            }

            //As long as students has items then process the fees, credit notes and refunds
            studentOK = studentOK == true ? await ProcessFees(uniteNetSuiteCustomers, feeRepGen, readOnly, firstRecordOnly, forceInsertCustomer) : studentOK;
            studentOK = studentOK == true ? await ProcessCreditNotes(uniteNetSuiteCustomers, creditNoteRepGen, readOnly, firstRecordOnly, forceInsertCustomer) : studentOK;

            //Not currently processing refunds as is part of stage 2 dev
            //studentOK = studentOK == true ? await ProcessRefunds(uniteNetSuiteCustomers, refundRepGen, readOnly, firstRecordOnly, forceInsertCustomer) : studentOK;

            if (studentOK == true && courseOK == true)
                return true;
            else
                return false;
        }

        public async Task<ICollection<NetSuiteCustomer>?> ProcessEnrolments(string? enrolmentRepGen, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            //Returns the Customers with the ID added so can be used in other methods

            _log?.Information($"\n{_divider}");

            ICollection<NetSuiteCustomer>? uniteNetSuiteCustomers = new List<NetSuiteCustomer>();
            ICollection<UNITeStudent>? uniteStudents = new List<UNITeStudent>();
            IList<UNITeEnrolment>? uniteEnrolments = new List<UNITeEnrolment>();
            NetSuiteCustomer? matchedCustomer = new NetSuiteCustomer();

            if (enrolmentRepGen != null && enrolmentRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Enrolments using UNIT-e RepGen Report: \"{enrolmentRepGen}\"");
            }
            else
            {
                _log?.Error("Enrolment RepGen is null/empty. Skipping Enrolment Import");
            }

            try
            {
                if (_unite != null)
                    uniteEnrolments = await _unite.ExportReport<List<UNITeEnrolment>>(enrolmentRepGen ?? "");

                if (uniteEnrolments == null)
                {
                    _log?.Information("No UNIT-e Enrolments found.");
                }
                else if (uniteEnrolments?.Count == 0)
                {
                    _log?.Information("No UNIT-e Enrolments To Be Imported Currently.");
                }
                else
                {
                    _log?.Information($"Loaded {uniteEnrolments?.Count} UNIT-e Enrolments");

                    if (_unite != null && _dbContext != null && _netsuite != null && _log != null)
                    {
                        var netSuiteLookups = new NetSuiteLookups(_dbContext, _unite, _netsuite, _log);
                        uniteEnrolments = await netSuiteLookups.GetCampusMappings<UNITeEnrolment>(uniteEnrolments);
                        _log?.Information($"First Mapped Location: {uniteEnrolments?.FirstOrDefault()?.NetSuiteLocationID}");
                    }

                    if (uniteEnrolments != null)
                        uniteStudents = ModelMappings.MapUNITeEnrolmentsToUNITeStudents(uniteEnrolments);

                    _log?.Information($"Loaded {uniteStudents?.Count} Distinct UNIT-e Students");

                    if (uniteStudents != null)
                        uniteNetSuiteCustomers = ModelMappings.MapUNITeStudentsToNetSuiteCustomers(uniteStudents);

                    if (uniteNetSuiteCustomers != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteCustomer? uniteNetSuiteCustomer in uniteNetSuiteCustomers!)
                        {
                            if (uniteNetSuiteCustomer == null)
                                continue; //Skip null customers

                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteCustomers.Count}: Searching for {uniteNetSuiteCustomer?.LastName}, {uniteNetSuiteCustomer?.FirstName} ({uniteNetSuiteCustomer?.CustEntityClientStudentNo}) in NetSuite");

                            #region Find Customer and Related Datasets
                            //Find this student (customer) in NetSuite

                            if (forceInsertCustomer == true)
                            {
                                _log?.Information($"**Force Insert Customer**: Will insert a new customer record even if one exists in NetSuite");
                                uniteNetSuiteCustomer!.LastName = $"{uniteNetSuiteCustomer.LastName}_Inserted";
                                uniteNetSuiteCustomer!.FirstName = $"{uniteNetSuiteCustomer.FirstName}_Inserted";
                                matchedCustomer = new NetSuiteCustomer();
                            }
                            else
                            {
                                matchedCustomer = await GetNetSuiteCustomer(uniteNetSuiteCustomer ?? new NetSuiteCustomer());
                            }

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
                                else if (readOnly == true)
                                {
                                    _log?.Information($"**Read Only Mode**: No Changes Made to NetSuite Customer: {uniteNetSuiteCustomer?.ID}");
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
            }

            return uniteNetSuiteCustomers;
        }

        public async Task<ICollection<NetSuiteNonInventorySaleItem>?> ProcessCourses(string? courseRepGen, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            //Returns the sale items with the ID added so can be used in other methods

            _log?.Information($"\n{_divider}");

            ICollection<NetSuiteNonInventorySaleItem>? uniteNetSuiteNonInventorySaleItems = new List<NetSuiteNonInventorySaleItem>();
            ICollection<UNITeCourse>? uniteCourses = new List<UNITeCourse>();
            NetSuiteNonInventorySaleItem? matchedSaleItem = new NetSuiteNonInventorySaleItem();

            if (courseRepGen != null && courseRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Courses using UNIT-e RepGen Report: \"{courseRepGen}\"");
            }
            else
            {
                _log?.Error("Course RepGen is null/empty. Skipping Course Import");
            }

            try
            {
                if (_unite != null)
                    uniteCourses = await _unite.ExportReport<List<UNITeCourse>>(courseRepGen ?? "");

                if (uniteCourses == null)
                {
                    _log?.Information("No UNIT-e Courses found.");
                }
                else if (uniteCourses?.Count == 0)
                {
                    _log?.Information("No UNIT-e Courses To Be Imported Currently.");
                }
                else
                {
                    _log?.Information($"Loaded {uniteCourses?.Count} UNIT-e Courses");

                    if (uniteCourses != null)
                        uniteNetSuiteNonInventorySaleItems = ModelMappings.MapUNITeCoursesToNetSuiteNonInventorySaleItems(uniteCourses);

                    if (uniteNetSuiteNonInventorySaleItems != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteNonInventorySaleItem? saleItem in uniteNetSuiteNonInventorySaleItems!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteNonInventorySaleItems.Count}: Searching for {saleItem?.ItemID} - {saleItem?.DisplayName} in NetSuite");

                            #region Find Non-Inventory Sale Item
                            //Find this course (non-inventory sale item) in NetSuite
                            matchedSaleItem = await GetNetSuiteNonInventorySaleItem(saleItem ?? new NetSuiteNonInventorySaleItem());

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
                                else if (readOnly == true)
                                {
                                    _log?.Information($"**Read Only Mode**: No Changes Made to NetSuite Non-Inventory Sale Item: {saleItem?.ID}");
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
            }

            return uniteNetSuiteNonInventorySaleItems;
        }

        public async Task<bool?> ProcessFees(ICollection<NetSuiteCustomer>? customers, string? feeRepGen, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            _log?.Information($"\n{_divider}");

            ICollection<UNITeFee>? uniteFees = new List<UNITeFee>();
            ICollection<UNITeFee>? uniteFeesWithCustomerID = new List<UNITeFee>();
            ICollection<NetSuiteInvoice>? uniteNetSuiteInvoices = new List<NetSuiteInvoice>();
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();

            if (feeRepGen != null && feeRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Fees using UNIT-e RepGen Report: \"{feeRepGen}\"");
            }
            else
            {
                _log?.Error("Fee RepGen is null/empty. Skipping Fee Import");
                return true;
            }

            bool? isOK = true;

            try
            {
                if (_unite != null)
                    uniteFees = await _unite.ExportReport<List<UNITeFee>>(feeRepGen ?? "");

                if (uniteFees == null)
                {
                    _log?.Information("No UNIT-e Fees found.");
                    return true;
                }
                else if (uniteFees?.Count == 0)
                {
                    _log?.Information("No UNIT-e Fees To Be Imported Currently.");
                    return true;
                }
                else
                {
                    _log?.Information($"Loaded {uniteFees?.Count} UNIT-e Fees");

                    //Add Customer ID from related Customer Record to UNITe Fees
                    if (customers != null && customers.Count > 0)
                    {
                        foreach (UNITeFee? fee in uniteFees!)
                        {
                            if (fee != null)
                            {
                                fee.NetSuiteCustomerID = customers
                                    .Where(c => c.UNITeStudentID == fee.StudentID)
                                    .Select(c => c.ID)
                                    .FirstOrDefault();
                            }
                        }
                    }

                    uniteFeesWithCustomerID = uniteFees?
                        .Where(f => f.NetSuiteCustomerID != null)
                        .ToList();

                    _log?.Information($"{uniteFeesWithCustomerID?.Count} UNIT-e Fees Linked Back to NetSuite Customers");

                    if (uniteFees != null)
                        uniteNetSuiteInvoices = ModelMappings.MapUNITeFeesToNetSuiteInvoices(uniteFeesWithCustomerID ?? new List<UNITeFee>());

                    if (uniteNetSuiteInvoices != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteInvoice? invoice in uniteNetSuiteInvoices!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteInvoices.Count}: Searching for invoice to Customer ID {invoice?.Entity?.ID} for {invoice?.Total?.Format("C2")} in NetSuite");

                            #region Find Invoice
                            //Find this fee (invoice) in NetSuite
                            //matchedInvoice = await GetNetSuiteInvoiceByCustomer(invoice ?? new NetSuiteInvoice());
                            matchedInvoice = await GetNetSuiteSQLInvoiceByCustomer(invoice ?? new NetSuiteInvoice());

                            if (matchedInvoice?.InvoiceMatchType == InvoiceMatchType.ByCustomerIDAndAmount)
                                _log?.Information($"Invoice Found in NetSuite by Customer ID and Total Amount with NetSuite Invoice Item ID: {matchedInvoice?.ID}");
                            else
                                _log?.Information($"Invoice Not Found in NetSuite");

                            int? numUniteInvoiceLines = invoice?.Items?.Where(i => i.Amount != null).ToList().Count ?? 0;

                            if (matchedInvoice?.ID != null)
                            {
                                //Update the ID of the record that came from UNIT-e so it can be used to update the record in NetSuite
                                if (invoice != null)
                                    invoice.ID = matchedInvoice?.ID;

                                //Get invoice items
                                if (matchedInvoice != null && matchedInvoice?.ID != null)
                                    matchedInvoice.Items = await GetNetSuiteInvoiceItems(matchedInvoice);

                                _log?.Information($"\nFound {numUniteInvoiceLines} invoice lines for customer in UNIT-e");
                                _log?.Information($"Found {matchedInvoice?.Items?.Count ?? 0} invoice lines for NetSuite Invoice Item ID: {matchedInvoice?.ID}");
                            }

                            //_log?.Information($"Main Invoice Line {invoice?.Items?.Where(i => i.IsMainInvoiceLine == true).FirstOrDefault()?.Description} for {invoice?.Items?.Where(i => i.IsMainInvoiceLine == true).FirstOrDefault()?.Amount?.Format("C2")}");

                            //Check if invoice lines are up to date and flag each one for insert or update
                            if (invoice != null)
                                invoice.Items = CheckNetSuiteInvoiceItems(invoice, matchedInvoice);

                            #endregion

                            #region Perform Updates to NetSuite Invoice
                            //Update or add the Invoice record in NetSuite
                            NetSuiteInvoice updatedNetSuiteInvoice = await UpdateNetSuiteInvoice(invoice ?? new NetSuiteInvoice(), readOnly);

                            //Update the ID of the record that came from UNIT-e so it matches the newly inserted record if not updating an existing NetSuite record
                            if (invoice != null)
                            {
                                if (updatedNetSuiteInvoice?.RecordActionType == RecordActionType.Insert)
                                {
                                    //Add the ID of the newly inserted record to the UNIT-e record
                                    invoice.ID = updatedNetSuiteInvoice?.ID;
                                    _log?.Information($"Inserted New NetSuite Invoice: {invoice?.ID}");
                                }
                                else if (updatedNetSuiteInvoice?.RecordActionType == RecordActionType.Update)
                                {
                                    _log?.Information($"Synced Existing NetSuite Invoice: {invoice?.ID}");
                                }
                                else if (readOnly == true)
                                {
                                    _log?.Information($"**Read Only Mode**: No Changes Made to NetSuite Invoice: {invoice?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"No Changes Made to NetSuite Invoice: {invoice?.ID}");
                                }
                            }
                            #endregion

                            //Update the invoice lines in NetSuite
                            bool? invoiceLinesUpdated = false;
                            if (invoice != null)
                                invoiceLinesUpdated = await UpdateNetSuiteInvoiceItems(invoice, readOnly);

                            if (firstRecordOnly == true)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Fees: {ex.Message}");
                return false;
            }

            return isOK;
        }

        public async Task<bool?> ProcessCreditNotes(ICollection<NetSuiteCustomer>? customers, string? creditNoteRepGen, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            _log?.Information($"\n{_divider}");

            ICollection<UNITeCreditNote>? uniteCreditNotes = new List<UNITeCreditNote>();
            ICollection<UNITeCreditNote>? uniteCreditNotesWithCustomerID = new List<UNITeCreditNote>();
            ICollection<NetSuiteCreditMemo>? uniteNetSuiteCreditMemos = new List<NetSuiteCreditMemo>();
            NetSuiteCreditMemo? matchedCreditMemo = new NetSuiteCreditMemo();

            if (creditNoteRepGen != null && creditNoteRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Credit Notes using UNIT-e RepGen Report: \"{creditNoteRepGen}\"");
            }
            else
            {
                _log?.Error("Credit Note RepGen is null/empty. Skipping Credit Note Import");
                return true;
            }

            bool? isOK = true;

            try
            {
                if (_unite != null)
                    uniteCreditNotes = await _unite.ExportReport<List<UNITeCreditNote>>(creditNoteRepGen ?? "");

                if (uniteCreditNotes == null)
                {
                    _log?.Information("No UNIT-e Credit Notes found.");
                    return true;
                }
                else if (uniteCreditNotes?.Count == 0)
                {
                    _log?.Information("No UNIT-e Credit Notes To Be Imported Currently.");
                    return true;
                }
                else
                {
                    _log?.Information($"Loaded {uniteCreditNotes?.Count} UNIT-e Credit Notes");

                    //Add Customer ID from related Customer Record to UNITe Fees
                    if (customers != null && customers.Count > 0)
                    {
                        foreach (UNITeCreditNote? fee in uniteCreditNotes!)
                        {
                            if (fee != null)
                            {
                                fee.NetSuiteCustomerID = customers
                                    .Where(c => c.UNITeStudentID == fee.StudentID)
                                    .Select(c => c.ID)
                                    .FirstOrDefault();
                            }
                        }
                    }

                    uniteCreditNotesWithCustomerID = uniteCreditNotes?
                        .Where(f => f.NetSuiteCustomerID != null)
                        .ToList();

                    _log?.Information($"{uniteCreditNotesWithCustomerID?.Count} UNIT-e Fees Linked Back to NetSuite Customers");

                    if (uniteCreditNotes != null)
                        uniteNetSuiteCreditMemos = ModelMappings.MapUNITeCreditNotesToNetSuiteCreditMemos(uniteCreditNotesWithCustomerID ?? new List<UNITeCreditNote>());

                    if (uniteNetSuiteCreditMemos != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteCreditMemo? creditMemo in uniteNetSuiteCreditMemos!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteCreditMemos.Count}: Searching for credit note to Customer {creditMemo?.Entity?.RefName} for {creditMemo?.Total?.Format("C2")} in NetSuite");

                            #region Find Credit Note
                            //Find this credit note (credit memo) in NetSuite
                            matchedCreditMemo = await GetNetSuiteSQLCreditMemo(creditMemo ?? new NetSuiteCreditMemo());

                            if (matchedCreditMemo?.CreditMemoMatchType == CreditMemoMatchType.ByCustomerIDAndAmount)
                                _log?.Information($"Credit Note Found in NetSuite by Customer ID and Total Amount with NetSuite Credit Memo Item ID: {matchedCreditMemo?.ID}");
                            else
                                _log?.Information($"Credit Note Not Found in NetSuite");

                            int? numUniteCreditMemoLines = creditMemo?.Items?.Where(i => i.Amount != null).ToList().Count ?? 0;

                            if (matchedCreditMemo?.ID != null)
                            {
                                //Update the ID of the record that came from UNIT-e so it can be used to update the record in NetSuite
                                if (creditMemo != null)
                                    creditMemo.ID = matchedCreditMemo?.ID;

                                //Get credit memo items
                                if (matchedCreditMemo != null && matchedCreditMemo?.ID != null)
                                    matchedCreditMemo.Items = await GetNetSuiteCreditMemoItems(matchedCreditMemo);

                                _log?.Information($"\nFound {numUniteCreditMemoLines} credit memo lines for customer in UNIT-e");
                                _log?.Information($"Found {matchedCreditMemo?.Items?.Count ?? 0} credit memo lines for NetSuite Credit Memo Item ID: {matchedCreditMemo?.ID}");
                            }

                            //_log?.Information($"Main Credit Memo Line {creditMemo?.Items?.Where(i => i.IsMainCreditMemoLine == true).FirstOrDefault()?.Description} for {creditMemo?.Items?.Where(i => i.IsMainCreditMemoLine == true).FirstOrDefault()?.Amount?.Format("C2")}");

                            //Check if credit memo lines are up to date and flag each one for insert or update
                            if (creditMemo != null)
                                creditMemo.Items = CheckNetSuiteCreditMemoItems(creditMemo, matchedCreditMemo);

                            #endregion

                            #region Perform Updates to NetSuite Credit Memo
                            //Update or add the Credit Memo record in NetSuite
                            NetSuiteCreditMemo updatedNetSuiteCreditMemo = await UpdateNetSuiteCreditMemo(creditMemo ?? new NetSuiteCreditMemo(), readOnly);

                            //Update the ID of the record that came from UNIT-e so it matches the newly inserted record if not updating an existing NetSuite record
                            if (creditMemo != null)
                            {
                                if (updatedNetSuiteCreditMemo?.RecordActionType == RecordActionType.Insert)
                                {
                                    //Add the ID of the newly inserted record to the UNIT-e record
                                    creditMemo.ID = updatedNetSuiteCreditMemo?.ID;
                                    _log?.Information($"Inserted New NetSuite Credit Memo: {creditMemo?.ID}");
                                }
                                else if (updatedNetSuiteCreditMemo?.RecordActionType == RecordActionType.Update)
                                {
                                    _log?.Information($"Synced Existing NetSuite Credit Memo: {creditMemo?.ID}");
                                }
                                else if (readOnly == true)
                                {
                                    _log?.Information($"**Read Only Mode**: No Changes Made to NetSuite Credit Memo: {creditMemo?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"No Changes Made to NetSuite Credit Memo: {creditMemo?.ID}");
                                }
                            }
                            #endregion

                            //Update the credit memo in NetSuite
                            bool? creditMemoLinesUpdated = false;
                            if (creditMemo != null)
                                creditMemoLinesUpdated = await UpdateNetSuiteCreditMemoItems(creditMemo, readOnly);

                            if (firstRecordOnly == true)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error Processing Credit Notes: {ex.Message}");
                return false;
            }

            return isOK;
        }

        public async Task<bool?> ProcessRefunds(ICollection<NetSuiteCustomer>? customers, string? refundRepGen, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer)
        {
            _log?.Information($"\n{_divider}");

            ICollection<UNITeRefund>? uniteRefunds = new List<UNITeRefund>();
            ICollection<UNITeRefund>? uniteRefundsWithCustomerID = new List<UNITeRefund>();
            ICollection<NetSuiteCustomerRefund>? uniteNetSuiteCustomerRefunds = new List<NetSuiteCustomerRefund>();
            NetSuiteCustomerRefund? matchedCustomerRefund = new NetSuiteCustomerRefund();

            if (refundRepGen != null && refundRepGen.Length > 0)
            {
                _log?.Information($"Processing UNIT-e Refunds using UNIT-e RepGen Report: \"{refundRepGen}\"");
            }
            else
            {
                _log?.Error("Refund RepGen is null/empty. Skipping Refund Import");
                return true;
            }

            bool? isOK = true;

            try
            {
                if (_unite != null)
                    uniteRefunds = await _unite.ExportReport<List<UNITeRefund>>(refundRepGen ?? "");

                if (uniteRefunds == null)
                {
                    _log?.Information("No UNIT-e Refunds found.");
                    return true;
                }
                else if (uniteRefunds?.Count == 0)
                {
                    _log?.Information("No UNIT-e Refunds To Be Imported Currently.");
                    return true;
                }
                else
                {
                    _log?.Information($"Loaded {uniteRefunds?.Count} UNIT-e Refunds");

                    //Add Customer ID from related Customer Record to UNITe Fees
                    if (customers != null && customers.Count > 0)
                    {
                        foreach (UNITeRefund? fee in uniteRefunds!)
                        {
                            if (fee != null)
                            {
                                fee.NetSuiteCustomerID = customers
                                    .Where(c => c.UNITeStudentID == fee.StudentID)
                                    .Select(c => c.ID)
                                    .FirstOrDefault();
                            }
                        }
                    }

                    uniteRefundsWithCustomerID = uniteRefunds?
                        .Where(f => f.NetSuiteCustomerID != null)
                        .ToList();

                    _log?.Information($"{uniteRefundsWithCustomerID?.Count} UNIT-e Refunds Linked Back to NetSuite Customers");

                    if (uniteRefunds != null)
                        uniteNetSuiteCustomerRefunds = ModelMappings.MapUNITeRefundsToNetSuiteCustomerRefunds(uniteRefundsWithCustomerID ?? new List<UNITeRefund>());

                    if (uniteNetSuiteCustomerRefunds != null)
                    {
                        int rowNumber = 0;
                        foreach (NetSuiteCustomerRefund? refund in uniteNetSuiteCustomerRefunds!)
                        {
                            rowNumber++;
                            _log?.Information($"\nRecord {rowNumber} of {uniteNetSuiteCustomerRefunds.Count}: Searching for refund to Customer {refund?.Customer?.RefName} for {refund?.Total?.Format("C2")} in NetSuite");

                            #region Find Refund
                            //Find this refund (customer refund) in NetSuite
                            matchedCustomerRefund = await GetNetSuiteSQLCustomerRefund(refund ?? new NetSuiteCustomerRefund());

                            if (matchedCustomerRefund?.CustomerRefundMatchType == CustomerRefundMatchType.ByCustomerIDAndAmount)
                                _log?.Information($"Refund Found in NetSuite by Customer ID and Total Amount with NetSuite Customer Refund Item ID: {matchedCustomerRefund?.ID}");
                            else
                                _log?.Information($"Refund Not Found in NetSuite");

                            if (matchedCustomerRefund?.ID != null)
                            {
                                //Update the ID of the record that came from UNIT-e so it can be used to update the record in NetSuite
                                if (refund != null)
                                    refund.ID = matchedCustomerRefund?.ID;
                            }
                            #endregion

                            #region Perform Updates to NetSuite Customer Refund
                            //Update or add the Customer Refund record in NetSuite
                            NetSuiteCustomerRefund updatedNetSuiteCustomerRefund = await UpdateNetSuiteCustomerRefund(refund ?? new NetSuiteCustomerRefund(), readOnly);

                            //Update the ID of the record that came from UNIT-e so it matches the newly inserted record if not updating an existing NetSuite record
                            if (refund != null)
                            {
                                if (updatedNetSuiteCustomerRefund?.RecordActionType == RecordActionType.Insert)
                                {
                                    //Add the ID of the newly inserted record to the UNIT-e record
                                    refund.ID = updatedNetSuiteCustomerRefund?.ID;
                                    _log?.Information($"Inserted New NetSuite Customer Refund: {refund?.ID}");
                                }
                                else if (updatedNetSuiteCustomerRefund?.RecordActionType == RecordActionType.Update)
                                {
                                    _log?.Information($"Synced Existing NetSuite Customer Refund: {refund?.ID}");
                                }
                                else if (readOnly == true)
                                {
                                    _log?.Information($"**Read Only Mode**: No Changes Made to NetSuite Customer Refund: {refund?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"No Changes Made to NetSuite Customer Refund: {refund?.ID}");
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
                _log?.Error($"Error Processing Refunds: {ex.Message}");
                return false;
            }

            return isOK;
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

        public async Task<ICollection<NetSuiteAddressBook>> GetNetSuiteAddresses(NetSuiteCustomer netSuiteCustomer)
        {
            NetSuiteSearchResult? netSuiteSearchResult = new NetSuiteSearchResult();
            ICollection<NetSuiteAddressBook>? netSuiteAddressBookEntries = new List<NetSuiteAddressBook>();

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
                //If no match found then create a new Non-Inventory Sale Item
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

        public async Task<NetSuiteInvoice> GetNetSuiteInvoiceByCustomer(NetSuiteInvoice netSuiteInvoice)
        {
            //Check if the invoice already exists in NetSuite by first returning all invoices for the academic year (to avoid returning too many records)
            //and then filtering by the customer and the amount as this is not possible in the NetSuite REST API directly as it does not support
            //querying sub-elements

            ICollection<NetSuiteInvoice>? allCustomerInvoices = new List<NetSuiteInvoice>();
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();

            searchParameters = new List<NetSuiteSearchParameter>();

            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_AFTER, netSuiteInvoice?.AcademicYearStartDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);
            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_BEFORE, netSuiteInvoice?.AcademicYearEndDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);

            allCustomerInvoices = await FindNetSuiteInvoices(searchParameters, InvoiceMatchType.ByAcademicYear);
            _log?.Information($"Found {allCustomerInvoices?.Count} invoices between {netSuiteInvoice?.AcademicYearStartDate?.Format("dd/MM/yyyy")} and {netSuiteInvoice?.AcademicYearEndDate?.Format("dd/MM/yyyy")}");

            if (allCustomerInvoices != null)
            {
                foreach (NetSuiteInvoice? invoice in allCustomerInvoices)
                {
                    if (invoice != null && invoice.Entity != null)
                    {
                        //Check if the invoice matches the customer ID and total amount
                        if (invoice.Entity.ID == netSuiteInvoice?.Entity?.ID
                            && invoice.Total == netSuiteInvoice?.Total)
                        {
                            matchedInvoice = invoice;
                            matchedInvoice.InvoiceMatchType = InvoiceMatchType.ByCustomerIDAndAmount;
                            break; //Exit loop as match is found
                        }
                    }
                }
            }

            if (matchedInvoice?.ID == null)
            {
                //If no match found then create a new invoice
                matchedInvoice = new NetSuiteInvoice();
                matchedInvoice.InvoiceMatchType = InvoiceMatchType.NotFound;
            }

            return matchedInvoice ?? new NetSuiteInvoice();
        }

        public async Task<NetSuiteInvoice> GetNetSuiteSQLInvoiceByCustomer(NetSuiteInvoice netSuiteInvoice)
        {
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();
            NetSuiteSQLQuery sqlQuery = new NetSuiteSQLQuery();

            //Check if the fee already exists in NetSuite by the customer ID
            if (matchedInvoice?.ID == null)
            {
                sqlQuery = new NetSuiteSQLQuery
                {
                    Q = @$"
                    SELECT 
                        T.* 
                    FROM Transaction T 
                    INNER JOIN TransactionLine TL 
                        ON TL.Transaction = T.ID
                    WHERE 
                        T.Entity = {netSuiteInvoice.Entity?.ID} 
                        AND T.AbbrevType = 'INV' 
                        AND T.TranDate >= '{netSuiteInvoice?.AcademicYearStartDate?.Format("dd/MM/yyyy")}' 
                        AND T.TranDate <= '{netSuiteInvoice?.AcademicYearEndDate?.Format("dd/MM/yyyy")}'
                        AND T.Memo = '{netSuiteInvoice?.Memo}'
                        AND TL.Amount = {netSuiteInvoice?.Total}
                    "
                };

                matchedInvoice = await FindNetSuiteSQLInvoice(sqlQuery, InvoiceMatchType.ByCustomerIDAndAmount);
            }

            if (matchedInvoice?.ID == null)
            {
                //If no match found then create a new invoice
                matchedInvoice = new NetSuiteInvoice();
                matchedInvoice.InvoiceMatchType = InvoiceMatchType.NotFound;
            }

            return matchedInvoice ?? new NetSuiteInvoice();
        }

        public async Task<NetSuiteInvoice> GetNetSuiteInvoicebyEmail(NetSuiteInvoice netSuiteInvoice)
        {
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();

            //Check if the fee already exists in NetSuite by the course code
            if (matchedInvoice?.ID == null)
            {
                searchParameters = new List<NetSuiteSearchParameter>();

                param = AddNetSuiteSearchParameter(null, "email", Operator.IS, netSuiteInvoice?.Email);
                searchParameters.Add(param);
                param = AddNetSuiteSearchParameter(null, "memo", Operator.IS, netSuiteInvoice?.Memo);
                searchParameters.Add(param);

                matchedInvoice = await FindNetSuiteInvoice(searchParameters, InvoiceMatchType.ByEmail);
            }

            if (matchedInvoice?.ID == null)
            {
                //If no match found then create a new invoice
                matchedInvoice = new NetSuiteInvoice();
                matchedInvoice.InvoiceMatchType = InvoiceMatchType.NotFound;
            }

            return matchedInvoice ?? new NetSuiteInvoice();
        }

        public async Task<NetSuiteInvoice> FindNetSuiteInvoice(IList<NetSuiteSearchParameter>? searchParameters, InvoiceMatchType invoiceMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("invoice", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedInvoice = await _netsuite.Get<NetSuiteInvoice>("invoice", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedInvoice != null)
                {
                    matchedInvoice.InvoiceMatchType = invoiceMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteInvoice: {ex.Message}");
                matchedInvoice = null;
            }

            return matchedInvoice ?? new NetSuiteInvoice();
        }

        public async Task<NetSuiteInvoice> FindNetSuiteSQLInvoice(NetSuiteSQLQuery sqlQuery, InvoiceMatchType invoiceMatchType)
        {
            NetSuiteSQLTransaction? searchResults = new NetSuiteSQLTransaction();
            NetSuiteInvoice? matchedInvoice = new NetSuiteInvoice();

            try
            {
                //Perform the search
                if (_netsuite != null)
                    searchResults = await _netsuite.SearchSQL<NetSuiteSQLTransaction>("transaction", sqlQuery);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedInvoice = await _netsuite.Get<NetSuiteInvoice>("invoice", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedInvoice != null)
                {
                    matchedInvoice.InvoiceMatchType = invoiceMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteSQLInvoice: {ex.Message}");
                matchedInvoice = null;
            }

            return matchedInvoice ?? new NetSuiteInvoice();
        }

        public async Task<ICollection<NetSuiteInvoice>> FindNetSuiteInvoices(IList<NetSuiteSearchParameter>? searchParameters, InvoiceMatchType invoiceMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            ICollection<NetSuiteInvoice>? matchedInvoices = new List<NetSuiteInvoice>();
            NetSuiteInvoice? invoice = new NetSuiteInvoice();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("invoice", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null && searchResults.Items != null)
                    {
                        foreach (NetSuiteSearchResultItem netSuiteSearchResult in searchResults.Items)
                        {
                            if (netSuiteSearchResult != null)
                            {
                                invoice = await _netsuite.Get<NetSuiteInvoice>("invoice", int.Parse(netSuiteSearchResult.ID ?? "0"));
                                if (invoice != null)
                                {
                                    invoice.InvoiceMatchType = invoiceMatchType;
                                    matchedInvoices.Add(invoice);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteInvoices: {ex.Message}");
                matchedInvoices = null;
            }

            return matchedInvoices ?? new List<NetSuiteInvoice>();
        }

        public async Task<NetSuiteInvoice> UpdateNetSuiteInvoice(NetSuiteInvoice netSuiteInvoice, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteInvoice?.ID ?? "0") > 0)
                    {
                        NetSuiteInvoice? updatedNetSuiteInvoice = new NetSuiteInvoice();
                        if (_netsuite != null)
                            updatedNetSuiteInvoice = await _netsuite.Update<NetSuiteInvoice>("invoice", int.Parse(netSuiteInvoice?.ID ?? "0"), netSuiteInvoice);

                        if (updatedNetSuiteInvoice != null)
                            updatedNetSuiteInvoice.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Invoice: {updatedNetSuiteInvoice?.ID}");
                        return updatedNetSuiteInvoice ?? new NetSuiteInvoice();
                    }
                    else
                    {
                        NetSuiteInvoice? insertedNetSuiteInvoice = new NetSuiteInvoice();
                        if (_netsuite != null)
                            insertedNetSuiteInvoice = await _netsuite.Add<NetSuiteInvoice>("invoice", netSuiteInvoice);

                        if (insertedNetSuiteInvoice != null)
                            insertedNetSuiteInvoice.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Invoice: {insertedNetSuiteInvoice?.ID}");
                        return insertedNetSuiteInvoice ?? new NetSuiteInvoice();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateNetSuiteInvoice: {ex.Message}");
                    netSuiteInvoice.RecordActionType = RecordActionType.None;
                    return netSuiteInvoice ?? new NetSuiteInvoice();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Invoice");
                netSuiteInvoice.RecordActionType = RecordActionType.None;
                return netSuiteInvoice ?? new NetSuiteInvoice();
            }
        }

        public async Task<ICollection<NetSuiteInvoiceItemDetail>> GetNetSuiteInvoiceItems(NetSuiteInvoice netSuiteInvoice)
        {
            NetSuiteSearchResult? netSuiteSearchResult = new NetSuiteSearchResult();
            ICollection<NetSuiteInvoiceItemDetail>? netSuiteInvoiceItems = new List<NetSuiteInvoiceItemDetail>();

            if (netSuiteInvoice?.ID != null)
            {
                //Get the invoice items for this invoice
                if (_netsuite != null)
                    netSuiteSearchResult = await _netsuite.GetAll<NetSuiteSearchResult>($"invoice/{netSuiteInvoice?.ID}/item");
                //_log?.Information($"Found {netSuiteSearchResult?.TotalResults} Invoice Items for Invoice: {netSuiteInvoice?.ID} in NetSuite");
            }
            else
            {
                //_log?.Information($"No Invoice Items Found for Invoice ID: {netSuiteInvoice?.ID}");
            }

            if (netSuiteSearchResult != null && netSuiteSearchResult?.Items != null)
            {
                try
                {
                    foreach (NetSuiteSearchResultItem? invoiceItem in netSuiteSearchResult.Items)
                    {
                        if (invoiceItem != null)
                        {
                            NetSuiteInvoiceItemDetail? netSuiteInvoiceItem = new NetSuiteInvoiceItemDetail();

                            if (_netsuite != null)
                            {
                                netSuiteInvoiceItem = await _netsuite.Get<NetSuiteInvoiceItemDetail>($"invoice/{netSuiteInvoice?.ID}/item", invoiceItem?.IDFromIDAndLink ?? 0);
                            }

                            if (netSuiteInvoiceItem != null)
                            {
                                netSuiteInvoiceItems.Add(netSuiteInvoiceItem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in GetNetSuiteInvoiceItems: {ex.Message}");
                    netSuiteInvoiceItems = null;
                }
            }

            return netSuiteInvoiceItems ?? new List<NetSuiteInvoiceItemDetail>();
        }

        public ICollection<NetSuiteInvoiceItemDetail> CheckNetSuiteInvoiceItems(NetSuiteInvoice netSuiteInvoice, NetSuiteInvoice? matchedInvoice)
        {
            int? numUniteInvoiceLines = netSuiteInvoice?.Items?.Where(i => i.Amount != null).ToList().Count ?? 0;

            if (netSuiteInvoice != null && netSuiteInvoice.Items != null)
            {
                int? invoiceItemNum = 0;
                foreach (NetSuiteInvoiceItemDetail? invoiceItem in netSuiteInvoice!.Items)
                {
                    if (invoiceItem != null && invoiceItem.Amount != null)
                    {
                        invoiceItemNum++;

                        if (matchedInvoice != null && matchedInvoice.Items != null)
                        {
                            foreach (NetSuiteInvoiceItemDetail? matchedInvoiceItem in matchedInvoice!.Items)
                            {
                                if (matchedInvoiceItem != null)
                                {
                                    //_log?.Information($"UNIT-e Invoice Line {invoiceItem?.Line} - {invoiceItem?.Description} for {invoiceItem?.Amount?.Format("C2")} vs NetSuite Invoice Line {matchedInvoiceItem?.Line} - {matchedInvoiceItem?.Description} for {matchedInvoiceItem?.Amount?.Format("C2")}");
                                    
                                    //Check if the invoice line exists and has not already been matched to an existing record
                                    if (invoiceItem?.Amount == matchedInvoiceItem?.Amount
                                        && netSuiteInvoice?.Items.Any(a => a?.Line == matchedInvoiceItem?.Line) == false)
                                    {
                                        //If this is the main invoice line and the course ID is the same then this is a match
                                        if (invoiceItem?.IsMainInvoiceLine == true
                                            && invoiceItem?.Item?.ID == matchedInvoiceItem?.Item?.ID)
                                        {
                                            //Main invoice line found with same amount so no action needed
                                            invoiceItem!.RecordActionType = RecordActionType.None;
                                        }
                                        else
                                        {
                                            //Additional invoice line found (as amount matches) so also no action required
                                            invoiceItem!.RecordActionType = RecordActionType.None;
                                        }

                                        //Update the ID of the record
                                        invoiceItem!.Line = matchedInvoiceItem?.Line;
                                    }
                                    else
                                    {
                                        invoiceItem!.RecordActionType = RecordActionType.Insert;
                                        _log?.Information($"Invoice Line {invoiceItemNum} of {numUniteInvoiceLines}: {invoiceItem?.Description} for {invoiceItem?.Amount.Format("C2")} not found in NetSuite so need to add");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //This would be for new customers
                            invoiceItem!.RecordActionType = RecordActionType.Insert;
                            _log?.Information($"Invoice Line {invoiceItemNum} of {numUniteInvoiceLines}: {invoiceItem?.Description} for {invoiceItem?.Amount.Format("C2")} not found in NetSuite so need to add");
                        }

                    }
                }
            }

            return netSuiteInvoice?.Items ?? new List<NetSuiteInvoiceItemDetail>();
        }

        public async Task<bool?> UpdateNetSuiteInvoiceItems(NetSuiteInvoice netSuiteInvoice, bool? readOnly)
        {
            bool? isOK = true;
            if (readOnly != true)
            {
                if (netSuiteInvoice?.Items != null && _netsuite != null)
                {
                    try
                    {
                        foreach (NetSuiteInvoiceItemDetail? invoiceItem in netSuiteInvoice.Items)
                        {
                            if (invoiceItem != null)
                            {
                                //If the invoice item is not null and has an ID then update it
                                if (invoiceItem.Line != null && invoiceItem.RecordActionType == RecordActionType.Update)
                                {
                                    NetSuiteInvoiceItemDetail? updatedInvoiceItem = await _netsuite.Update<NetSuiteInvoiceItemDetail>($"invoice/{netSuiteInvoice?.ID}/item", invoiceItem.Line ?? 0, invoiceItem);
                                    _log?.Information($"Updated Invoice Line {updatedInvoiceItem?.Line} for Invoice {netSuiteInvoice?.ID}");
                                }
                                else if (invoiceItem.RecordActionType == RecordActionType.Insert)
                                {
                                    NetSuiteInvoiceItemDetail? insertedInvoiceItem = await _netsuite.Add<NetSuiteInvoiceItemDetail>($"invoice/{netSuiteInvoice?.ID}/item", invoiceItem);
                                    _log?.Information($"Inserted Invoice Line {insertedInvoiceItem?.Line} for Invoice {netSuiteInvoice?.ID}");
                                }
                                else if (invoiceItem.RecordActionType == RecordActionType.None)
                                {
                                    _log?.Information($"No Changes Made to Invoice Line {invoiceItem?.Line} for Invoice {netSuiteInvoice?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"Error determining action for Invoice Line {invoiceItem?.Line} for Invoice {netSuiteInvoice?.ID}");
                                    isOK = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log?.Error($"Error updating Invoice Lines: {ex.Message}");
                        isOK = false;
                    }

                }
            }

            return isOK;
        }

        public async Task<NetSuiteCreditMemo> GetNetSuiteCreditMemo(NetSuiteCreditMemo netSuiteCreditMemo)
        {
            //Check if the credit memo already exists in NetSuite by first returning all credit memos for the academic year (to avoid returning too many records)
            //and then filtering by the customer and the amount as this is not possible in the NetSuite REST API directly as it does not support
            //querying sub-elements

            ICollection<NetSuiteCreditMemo>? allCustomerCredits = new List<NetSuiteCreditMemo>();
            NetSuiteCreditMemo? matchedCredit = new NetSuiteCreditMemo();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();

            searchParameters = new List<NetSuiteSearchParameter>();

            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_AFTER, netSuiteCreditMemo?.AcademicYearStartDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);
            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_BEFORE, netSuiteCreditMemo?.AcademicYearEndDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);

            allCustomerCredits = await FindNetSuiteCreditMemos(searchParameters, CreditMemoMatchType.ByAcademicYear);
            _log?.Information($"Found {allCustomerCredits?.Count} credits between {netSuiteCreditMemo?.AcademicYearStartDate?.Format("dd/MM/yyyy")} and {netSuiteCreditMemo?.AcademicYearEndDate?.Format("dd/MM/yyyy")}");

            if (allCustomerCredits != null)
            {
                foreach (NetSuiteCreditMemo? credit in allCustomerCredits)
                {
                    if (credit != null && credit.Entity != null)
                    {
                        //Check if the credit memo matches the customer ID and total amount
                        if (credit.Entity.ID == netSuiteCreditMemo?.Entity?.ID
                            && credit.Total == netSuiteCreditMemo?.Total)
                        {
                            matchedCredit = credit;
                            matchedCredit.CreditMemoMatchType = CreditMemoMatchType.ByCustomerIDAndAmount;
                            break; //Exit loop as match is found
                        }
                    }
                }
            }

            if (matchedCredit?.ID == null)
            {
                //If no match found then create a new credit memo
                matchedCredit = new NetSuiteCreditMemo();
                matchedCredit.CreditMemoMatchType = CreditMemoMatchType.NotFound;
            }

            return matchedCredit ?? new NetSuiteCreditMemo();
        }

        public async Task<NetSuiteCreditMemo> GetNetSuiteSQLCreditMemo(NetSuiteCreditMemo netSuiteCreditMemo)
        {
            NetSuiteCreditMemo? matchedCreditMemo = new NetSuiteCreditMemo();
            NetSuiteSQLQuery sqlQuery = new NetSuiteSQLQuery();

            //Check if the credit memo already exists in NetSuite by the customer ID
            if (matchedCreditMemo?.ID == null)
            {
                sqlQuery = new NetSuiteSQLQuery
                {
                    Q = @$"
                    SELECT 
                        T.* 
                    FROM Transaction T 
                    INNER JOIN TransactionLine TL 
                        ON TL.Transaction = T.ID
                    WHERE 
                        T.Entity = {netSuiteCreditMemo.Entity?.ID} 
                        AND T.AbbrevType = 'CREDMEM' 
                        AND T.TranDate >= '{netSuiteCreditMemo?.AcademicYearStartDate?.Format("dd/MM/yyyy")}' 
                        AND T.TranDate <= '{netSuiteCreditMemo?.AcademicYearEndDate?.Format("dd/MM/yyyy")}'
                        AND TL.Amount = {netSuiteCreditMemo?.Total}
                    "
                };

                matchedCreditMemo = await FindNetSuiteSQLCreditMemo(sqlQuery, CreditMemoMatchType.ByCustomerIDAndAmount);
            }

            if (matchedCreditMemo?.ID == null)
            {
                //If no match found then create a new credit memo
                matchedCreditMemo = new NetSuiteCreditMemo();
                matchedCreditMemo.CreditMemoMatchType = CreditMemoMatchType.NotFound;
            }

            return matchedCreditMemo ?? new NetSuiteCreditMemo();
        }

        public async Task<NetSuiteCreditMemo> FindNetSuiteCreditMemo(IList<NetSuiteSearchParameter>? searchParameters, CreditMemoMatchType creditMemoMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteCreditMemo? matchedCreditMemo = new NetSuiteCreditMemo();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("creditMemo", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedCreditMemo = await _netsuite.Get<NetSuiteCreditMemo>("creditMemo", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCreditMemo != null)
                {
                    matchedCreditMemo.CreditMemoMatchType = creditMemoMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteCreditMemo: {ex.Message}");
                matchedCreditMemo = null;
            }

            return matchedCreditMemo ?? new NetSuiteCreditMemo();
        }

        public async Task<NetSuiteCreditMemo> FindNetSuiteSQLCreditMemo(NetSuiteSQLQuery sqlQuery, CreditMemoMatchType creditMemoMatchType)
        {
            NetSuiteSQLTransaction? searchResults = new NetSuiteSQLTransaction();
            NetSuiteCreditMemo? matchedCreditMemo = new NetSuiteCreditMemo();

            try
            {
                //Perform the search
                if (_netsuite != null)
                    searchResults = await _netsuite.SearchSQL<NetSuiteSQLTransaction>("transaction", sqlQuery);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedCreditMemo = await _netsuite.Get<NetSuiteCreditMemo>("creditMemo", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCreditMemo != null)
                {
                    matchedCreditMemo.CreditMemoMatchType = creditMemoMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteSQLCreditMemo: {ex.Message}");
                matchedCreditMemo = null;
            }

            return matchedCreditMemo ?? new NetSuiteCreditMemo();
        }

        public async Task<ICollection<NetSuiteCreditMemo>> FindNetSuiteCreditMemos(IList<NetSuiteSearchParameter>? searchParameters, CreditMemoMatchType creditMemoMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            ICollection<NetSuiteCreditMemo>? matchedCustomerCreditNotes = new List<NetSuiteCreditMemo>();
            NetSuiteCreditMemo? credit = new NetSuiteCreditMemo();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("creditMemo", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null && searchResults.Items != null)
                    {
                        foreach (NetSuiteSearchResultItem netSuiteSearchResult in searchResults.Items)
                        {
                            if (netSuiteSearchResult != null)
                            {
                                credit = await _netsuite.Get<NetSuiteCreditMemo>("creditMemo", int.Parse(netSuiteSearchResult.ID ?? "0"));
                                if (credit != null)
                                {
                                    credit.CreditMemoMatchType = creditMemoMatchType;
                                    matchedCustomerCreditNotes.Add(credit);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteCreditMemos: {ex.Message}");
                matchedCustomerCreditNotes = null;
            }

            return matchedCustomerCreditNotes ?? new List<NetSuiteCreditMemo>();
        }

        public async Task<NetSuiteCreditMemo> UpdateNetSuiteCreditMemo(NetSuiteCreditMemo netSuiteCreditMemo, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteCreditMemo?.ID ?? "0") > 0)
                    {
                        NetSuiteCreditMemo? updatedNetSuiteCreditMemo = new NetSuiteCreditMemo();
                        if (_netsuite != null)
                            updatedNetSuiteCreditMemo = await _netsuite.Update<NetSuiteCreditMemo>("creditMemo", int.Parse(netSuiteCreditMemo?.ID ?? "0"), netSuiteCreditMemo);

                        if (updatedNetSuiteCreditMemo != null)
                            updatedNetSuiteCreditMemo.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Credit Memo: {updatedNetSuiteCreditMemo?.ID}");
                        return updatedNetSuiteCreditMemo ?? new NetSuiteCreditMemo();
                    }
                    else
                    {
                        NetSuiteCreditMemo? insertedNetSuiteCreditMemo = new NetSuiteCreditMemo();
                        if (_netsuite != null)
                            insertedNetSuiteCreditMemo = await _netsuite.Add<NetSuiteCreditMemo>("creditMemo", netSuiteCreditMemo);

                        if (insertedNetSuiteCreditMemo != null)
                            insertedNetSuiteCreditMemo.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Credit Memo: {insertedNetSuiteCreditMemo?.ID}");
                        return insertedNetSuiteCreditMemo ?? new NetSuiteCreditMemo();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateNetSuiteCreditMemo: {ex.Message}");
                    netSuiteCreditMemo.RecordActionType = RecordActionType.None;
                    return netSuiteCreditMemo ?? new NetSuiteCreditMemo();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Credit Memo");
                netSuiteCreditMemo.RecordActionType = RecordActionType.None;
                return netSuiteCreditMemo ?? new NetSuiteCreditMemo();
            }
        }

        public async Task<ICollection<NetSuiteCreditMemoItemDetail>> GetNetSuiteCreditMemoItems(NetSuiteCreditMemo netSuiteCreditMemo)
        {
            NetSuiteSearchResult? netSuiteSearchResult = new NetSuiteSearchResult();
            ICollection<NetSuiteCreditMemoItemDetail>? netSuiteCreditMemoItems = new List<NetSuiteCreditMemoItemDetail>();

            if (netSuiteCreditMemo?.ID != null)
            {
                //Get the credit memo items for this credit memo
                if (_netsuite != null)
                    netSuiteSearchResult = await _netsuite.GetAll<NetSuiteSearchResult>($"creditMemo/{netSuiteCreditMemo?.ID}/item");
                //_log?.Information($"Found {netSuiteSearchResult?.TotalResults} Credit Memo Items for Credit Memo: {netSuiteCreditMemo?.ID} in NetSuite");
            }
            else
            {
                //_log?.Information($"No Credit Memo Items Found for Credit Memo ID: {netSuiteCreditMemo?.ID}");
            }

            if (netSuiteSearchResult != null && netSuiteSearchResult?.Items != null)
            {
                try
                {
                    foreach (NetSuiteSearchResultItem? creditNoteItem in netSuiteSearchResult.Items)
                    {
                        if (creditNoteItem != null)
                        {
                            NetSuiteCreditMemoItemDetail? netSuitecreditMemoItem = new NetSuiteCreditMemoItemDetail();

                            if (_netsuite != null)
                            {
                                netSuitecreditMemoItem = await _netsuite.Get<NetSuiteCreditMemoItemDetail>($"creditMemo/{netSuiteCreditMemo?.ID}/item", creditNoteItem?.IDFromIDAndLink ?? 0);
                            }

                            if (netSuitecreditMemoItem != null)
                            {
                                netSuiteCreditMemoItems.Add(netSuitecreditMemoItem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in GetNetSuiteCreditMemoItems: {ex.Message}");
                    netSuiteCreditMemoItems = null;
                }
            }

            return netSuiteCreditMemoItems ?? new List<NetSuiteCreditMemoItemDetail>();
        }

        public ICollection<NetSuiteCreditMemoItemDetail> CheckNetSuiteCreditMemoItems(NetSuiteCreditMemo netSuiteCreditMemo, NetSuiteCreditMemo? matchedCreditMemo)
        {
            int? numUniteCreditMemoLines = netSuiteCreditMemo?.Items?.Where(i => i.Amount != null).ToList().Count ?? 0;

            if (netSuiteCreditMemo != null && netSuiteCreditMemo.Items != null)
            {
                int? creditMemoItemNum = 0;
                foreach (NetSuiteCreditMemoItemDetail? creditMemoItem in netSuiteCreditMemo!.Items)
                {
                    if (creditMemoItem != null && creditMemoItem.Amount != null)
                    {
                        creditMemoItemNum++;

                        if (matchedCreditMemo != null && matchedCreditMemo.Items != null)
                        {
                            foreach (NetSuiteCreditMemoItemDetail? matchedCreditMemoItem in matchedCreditMemo!.Items)
                            {
                                if (matchedCreditMemoItem != null)
                                {
                                    //_log?.Information($"UNIT-e Credit Memo Line {creditMemoItem?.Line} - {creditMemoItem?.Description} for {creditMemoItem?.Amount?.Format("C2")} vs NetSuite Credit Memo Line {matchedCreditMemoItem?.Line} - {matchedCreditMemoItem?.Description} for {matchedCreditMemoItem?.Amount?.Format("C2")}");

                                    //Check if the credit memo line exists and has not already been matched to an existing record
                                    if (creditMemoItem?.Amount == matchedCreditMemoItem?.Amount
                                        && netSuiteCreditMemo?.Items.Any(a => a?.Line == matchedCreditMemoItem?.Line) == false)
                                    {
                                        //If this is the main credit memo line then this is a match
                                        if (creditMemoItem?.IsMainCreditMemoLine == true)
                                        {
                                            //Main credit memo line found with same amount so no action needed
                                            creditMemoItem!.RecordActionType = RecordActionType.None;
                                        }
                                        else
                                        {
                                            //Additional credit memo line found (as amount matches) so also no action required
                                            creditMemoItem!.RecordActionType = RecordActionType.None;
                                        }

                                        //Update the ID of the record
                                        creditMemoItem!.Line = matchedCreditMemoItem?.Line;
                                    }
                                    else
                                    {
                                        creditMemoItem!.RecordActionType = RecordActionType.Insert;
                                        _log?.Information($"Credit Memo Line {creditMemoItemNum} of {numUniteCreditMemoLines}: {creditMemoItem?.Description} for {creditMemoItem?.Amount.Format("C2")} not found in NetSuite so need to add");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //This would be for new customers
                            creditMemoItem!.RecordActionType = RecordActionType.Insert;
                            _log?.Information($"Credit Memo Line {creditMemoItemNum} of {numUniteCreditMemoLines}: {creditMemoItem?.Description} for {creditMemoItem?.Amount.Format("C2")} not found in NetSuite so need to add");
                        }

                    }
                }
            }

            return netSuiteCreditMemo?.Items ?? new List<NetSuiteCreditMemoItemDetail>();
        }

        public async Task<bool?> UpdateNetSuiteCreditMemoItems(NetSuiteCreditMemo netSuiteCreditMemo, bool? readOnly)
        {
            bool? isOK = true;
            if (readOnly != true)
            {
                if (netSuiteCreditMemo?.Items != null && _netsuite != null)
                {
                    try
                    {
                        foreach (NetSuiteCreditMemoItemDetail? creditMemoItem in netSuiteCreditMemo.Items)
                        {
                            if (creditMemoItem != null)
                            {
                                //If the credit memo item is not null and has an ID then update it
                                if (creditMemoItem.Line != null && creditMemoItem.RecordActionType == RecordActionType.Update)
                                {
                                    NetSuiteCreditMemoItemDetail? updatedCreditMemoItem = await _netsuite.Update<NetSuiteCreditMemoItemDetail>($"creditMemo/{netSuiteCreditMemo?.ID}/item", creditMemoItem.Line ?? 0, creditMemoItem);
                                    _log?.Information($"Updated Credit Memo Line {updatedCreditMemoItem?.Line} for Credit Memo {netSuiteCreditMemo?.ID}");
                                }
                                else if (creditMemoItem.RecordActionType == RecordActionType.Insert)
                                {
                                    NetSuiteCreditMemoItemDetail? insertedCreditMemoItem = await _netsuite.Add<NetSuiteCreditMemoItemDetail>($"creditMemo/{netSuiteCreditMemo?.ID}/item", creditMemoItem);
                                    _log?.Information($"Inserted Credit Memo Line {insertedCreditMemoItem?.Line} for Credit Memo {netSuiteCreditMemo?.ID}");
                                }
                                else if (creditMemoItem.RecordActionType == RecordActionType.None)
                                {
                                    _log?.Information($"No Changes Made to Credit Memo Line {creditMemoItem?.Line} for Credit Memo {netSuiteCreditMemo?.ID}");
                                }
                                else
                                {
                                    _log?.Information($"Error determining action for Credit Memo Line {creditMemoItem?.Line} for Credit Memo {netSuiteCreditMemo?.ID}");
                                    isOK = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log?.Error($"Error updating Credit Memo Lines: {ex.Message}");
                        isOK = false;
                    }

                }
            }

            return isOK;
        }

        public async Task<NetSuiteCustomerRefund> GetNetSuiteCustomerRefund(NetSuiteCustomerRefund netSuiteCustomerRefund)
        {
            //Check if the refund already exists in NetSuite by first returning all refunds for the academic year (to avoid returning too many records)
            //and then filtering by the customer and the amount as this is not possible in the NetSuite REST API directly as it does not support
            //querying sub-elements

            ICollection<NetSuiteCustomerRefund>? allCustomerRefunds = new List<NetSuiteCustomerRefund>();
            NetSuiteCustomerRefund? matchedCustomerRefund = new NetSuiteCustomerRefund();
            IList<NetSuiteSearchParameter> searchParameters = new List<NetSuiteSearchParameter>();
            NetSuiteSearchParameter param = new NetSuiteSearchParameter();

            searchParameters = new List<NetSuiteSearchParameter>();

            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_AFTER, netSuiteCustomerRefund?.AcademicYearStartDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);
            param = AddNetSuiteSearchParameter(null, "tranDate", Operator.ON_OR_BEFORE, netSuiteCustomerRefund?.AcademicYearEndDate?.Format("dd/MM/yyyy"));
            searchParameters.Add(param);

            allCustomerRefunds = await FindNetSuiteCustomerRefunds(searchParameters, CustomerRefundMatchType.ByAcademicYear);
            _log?.Information($"Found {allCustomerRefunds?.Count} refunds between {netSuiteCustomerRefund?.AcademicYearStartDate?.Format("dd/MM/yyyy")} and {netSuiteCustomerRefund?.AcademicYearEndDate?.Format("dd/MM/yyyy")}");

            if (allCustomerRefunds != null)
            {
                foreach (NetSuiteCustomerRefund? refund in allCustomerRefunds)
                {
                    if (refund != null && refund.Customer != null)
                    {
                        //Check if the refund matches the customer ID and total amount
                        if (refund.Customer.ID == netSuiteCustomerRefund?.Customer?.ID
                            && refund.Total == netSuiteCustomerRefund?.Total)
                        {
                            matchedCustomerRefund = refund;
                            matchedCustomerRefund.CustomerRefundMatchType = CustomerRefundMatchType.ByCustomerIDAndAmount;
                            break; //Exit loop as match is found
                        }
                    }
                }
            }

            if (matchedCustomerRefund?.ID == null)
            {
                //If no match found then create a new customer refund
                matchedCustomerRefund = new NetSuiteCustomerRefund();
                matchedCustomerRefund.CustomerRefundMatchType = CustomerRefundMatchType.NotFound;
            }

            return matchedCustomerRefund ?? new NetSuiteCustomerRefund();
        }

        public async Task<NetSuiteCustomerRefund> GetNetSuiteSQLCustomerRefund(NetSuiteCustomerRefund netSuiteCustomerRefund)
        {
            NetSuiteCustomerRefund? matchedCustomerRefund = new NetSuiteCustomerRefund();
            NetSuiteSQLQuery sqlQuery = new NetSuiteSQLQuery();

            //Check if the customer refund already exists in NetSuite by the customer ID
            if (matchedCustomerRefund?.ID == null)
            {
                sqlQuery = new NetSuiteSQLQuery
                {
                    Q = @$"
                    SELECT 
                        T.* 
                    FROM Transaction T 
                    INNER JOIN TransactionLine TL 
                        ON TL.Transaction = T.ID
                    WHERE 
                        T.Entity = {netSuiteCustomerRefund.Customer?.ID} 
                        AND T.AbbrevType = 'RFND' 
                        AND T.TranDate >= '{netSuiteCustomerRefund?.AcademicYearStartDate?.Format("dd/MM/yyyy")}' 
                        AND T.TranDate <= '{netSuiteCustomerRefund?.AcademicYearEndDate?.Format("dd/MM/yyyy")}'
                        AND TL.Amount = {netSuiteCustomerRefund?.Total}
                    "
                };

                matchedCustomerRefund = await FindNetSuiteSQLCustomerRefund(sqlQuery, CustomerRefundMatchType.ByCustomerIDAndAmount);
            }

            if (matchedCustomerRefund?.ID == null)
            {
                //If no match found then create a new customer refund
                matchedCustomerRefund = new NetSuiteCustomerRefund();
                matchedCustomerRefund.CustomerRefundMatchType = CustomerRefundMatchType.NotFound;
            }

            return matchedCustomerRefund ?? new NetSuiteCustomerRefund();
        }

        public async Task<NetSuiteCustomerRefund> FindNetSuiteCustomerRefund(IList<NetSuiteSearchParameter>? searchParameters, CustomerRefundMatchType customerRefundMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            NetSuiteCustomerRefund? matchedCustomerRefund = new NetSuiteCustomerRefund();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("customerRefund", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedCustomerRefund = await _netsuite.Get<NetSuiteCustomerRefund>("customerRefund", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCustomerRefund != null)
                {
                    matchedCustomerRefund.CustomerRefundMatchType = customerRefundMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteCustomerRefund: {ex.Message}");
                matchedCustomerRefund = null;
            }

            return matchedCustomerRefund ?? new NetSuiteCustomerRefund();
        }

        public async Task<NetSuiteCustomerRefund> FindNetSuiteSQLCustomerRefund(NetSuiteSQLQuery sqlQuery, CustomerRefundMatchType customerRefundMatchType)
        {
            NetSuiteSQLTransaction? searchResults = new NetSuiteSQLTransaction();
            NetSuiteCustomerRefund? matchedCustomerRefund = new NetSuiteCustomerRefund();

            try
            {
                //Perform the search
                if (_netsuite != null)
                    searchResults = await _netsuite.SearchSQL<NetSuiteSQLTransaction>("transaction", sqlQuery);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null)
                        matchedCustomerRefund = await _netsuite.Get<NetSuiteCustomerRefund>("customerRefund", int.Parse(searchResults?.Items?.FirstOrDefault()?.ID ?? "0"));
                }

                if (matchedCustomerRefund != null)
                {
                    matchedCustomerRefund.CustomerRefundMatchType = customerRefundMatchType;
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteSQLCreditMemo: {ex.Message}");
                matchedCustomerRefund = null;
            }

            return matchedCustomerRefund ?? new NetSuiteCustomerRefund();
        }

        public async Task<ICollection<NetSuiteCustomerRefund>> FindNetSuiteCustomerRefunds(IList<NetSuiteSearchParameter>? searchParameters, CustomerRefundMatchType customerRefundMatchType)
        {
            NetSuiteSearchResult? searchResults = new NetSuiteSearchResult();
            ICollection<NetSuiteCustomerRefund>? matchedCustomerRefunds = new List<NetSuiteCustomerRefund>();
            NetSuiteCustomerRefund? refund = new NetSuiteCustomerRefund();

            try
            {
                //Perform the search
                if (searchParameters == null)
                    searchParameters = new List<NetSuiteSearchParameter>();

                if (_netsuite != null)
                    searchResults = await _netsuite.Search<NetSuiteSearchResult>("customerRefund", searchParameters);

                if (searchResults?.Count > 0)
                {
                    //Get record details if it matches as should only ever be one match here
                    if (_netsuite != null && searchResults.Items != null)
                    {
                        foreach (NetSuiteSearchResultItem netSuiteSearchResult in searchResults.Items)
                        {
                            if (netSuiteSearchResult != null)
                            {
                                refund = await _netsuite.Get<NetSuiteCustomerRefund>("customerRefund", int.Parse(netSuiteSearchResult.ID ?? "0"));
                                if (refund != null)
                                {
                                    refund.CustomerRefundMatchType = customerRefundMatchType;
                                    matchedCustomerRefunds.Add(refund);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"Error in FindNetSuiteCustomerRefund: {ex.Message}");
                matchedCustomerRefunds = null;
            }

            return matchedCustomerRefunds ?? new List<NetSuiteCustomerRefund>();
        }

        public async Task<NetSuiteCustomerRefund> UpdateNetSuiteCustomerRefund(NetSuiteCustomerRefund netSuiteCustomerRefund, bool? readOnly)
        {
            if (readOnly != true)
            {
                try
                {
                    if (int.Parse(netSuiteCustomerRefund?.ID ?? "0") > 0)
                    {
                        NetSuiteCustomerRefund? updatedNetSuiteCustomerRefund = new NetSuiteCustomerRefund();
                        if (_netsuite != null)
                            updatedNetSuiteCustomerRefund = await _netsuite.Update<NetSuiteCustomerRefund>("customerRefund", int.Parse(netSuiteCustomerRefund?.ID ?? "0"), netSuiteCustomerRefund);

                        if (updatedNetSuiteCustomerRefund != null)
                            updatedNetSuiteCustomerRefund.RecordActionType = RecordActionType.Update;

                        //_log?.Information($"Synced Existing NetSuite Customer Refund: {updatedNetSuiteCustomerRefund?.ID}");
                        return updatedNetSuiteCustomerRefund ?? new NetSuiteCustomerRefund();
                    }
                    else
                    {
                        NetSuiteCustomerRefund? insertedNetSuiteCustomerRefund = new NetSuiteCustomerRefund();
                        if (_netsuite != null)
                            insertedNetSuiteCustomerRefund = await _netsuite.Add<NetSuiteCustomerRefund>("customerRefund", netSuiteCustomerRefund);

                        if (insertedNetSuiteCustomerRefund != null)
                            insertedNetSuiteCustomerRefund.RecordActionType = RecordActionType.Insert;

                        //_log?.Information($"Inserted New NetSuite Customer Refund: {insertedNetSuiteCustomerRefund?.ID}");
                        return insertedNetSuiteCustomerRefund ?? new NetSuiteCustomerRefund();
                    }
                }
                catch (Exception ex)
                {
                    _log?.Error($"Error in UpdateNetSuiteCustomerRefund: {ex.Message}");
                    netSuiteCustomerRefund.RecordActionType = RecordActionType.None;
                    return netSuiteCustomerRefund ?? new NetSuiteCustomerRefund();
                }
            }
            else
            {
                //_log?.Information($"ReadOnly Mode: No Changes Made to Credit Memo");
                netSuiteCustomerRefund.RecordActionType = RecordActionType.None;
                return netSuiteCustomerRefund ?? new NetSuiteCustomerRefund();
            }
        }
    }
}
