using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Shared;
using Serilog;

namespace NetSuiteIntegration.Services
{
    public static class ModelMappings
    {
        public static IList<UNITeStudent> MapUNITeEnrolmentsToUNITeStudents(IList<UNITeEnrolment> uniteEnrolments)
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
                    AcademicYearName = stu.AcademicYearName,
                    AcademicYearStartDate = stu.AcademicYearStartDate,
                    AcademicYearEndDate = stu.AcademicYearEndDate
                }).ToList<UNITeStudent>();

            return uniteStudents ?? new List<UNITeStudent>();
        }

        public static IList<NetSuiteCustomer> MapUNITeStudentsToNetSuiteCustomers(IList<UNITeStudent> uniteStudents)
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
                DepositBalance = decimal.ToDouble(cus.FeeGross ?? 0),
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

        public static IList<NetSuiteNonInventorySaleItem> MapUNITeCoursesToNetSuiteNonInventorySaleItems(IList<UNITeCourse> uniteCourses)
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

        public static IList<NetSuiteInvoice> MapUNITeFeesToNetSuiteInvoices(IList<UNITeFee> uniteFees)
        {
            IList<NetSuiteInvoice>? netSuiteInvoices = new List<NetSuiteInvoice>();
            int? numRecordsWithCustomerID = uniteFees.Where(f => f.NetSuiteCustomerID > 0).Count();

            if (numRecordsWithCustomerID != uniteFees.Count())
            {
                Console.WriteLine($"Not all invoice records are valid as {uniteFees.Count() - numRecordsWithCustomerID} do not have a customer ID assigned");
                return new List<NetSuiteInvoice>();
            }

            //Map UNIT-e Fees to NetSuite Invoices
            netSuiteInvoices = uniteFees?.Select(inv => new NetSuiteInvoice
            {
                Account = new NetSuiteInvoiceAccount
                {
                    ID = "456",
                    RefName = "20709 Debtors Control Account"
                },
                AmountPaid = 0,
                AmountRemaining = decimal.ToDouble(inv.FeeGross ?? 0),
                AmountRemainingTotalBox = decimal.ToDouble(inv.FeeGross ?? 0),
                ApprovalStatus = new NetSuiteInvoiceApprovalStatus
                {
                    ID = "2",
                    RefName = "Approved"
                },
                AsOfDate = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                BillAddress = inv.AddressMainEncoded,
                BillAddressList = new NetSuiteInvoiceBillAddressList
                {
                    RefName = inv.AddressMainType
                },
                BillingAddressText = inv.AddressMainEncoded,
                CanHaveStackable = false,
                Class = new NetSuiteInvoiceClass
                {
                    RefName = $"{inv.SubjectName}: {inv.CampusName}"
                },
                CreatedDate = DateTime.Now,
                Currency = new NetSuiteInvoiceCurrency
                {
                    ID = "1",
                    RefName = "GBP"
                },
                Custbody15699ExcludeFromEpProcess = false,
                CustbodyAtlasExistCustHdn = new NetSuiteInvoiceCustbodyAtlasExistCustHdn
                {
                    ID = "2",
                    RefName = "Existing Customer"
                },
                CustbodyAtlasNewCustHdn = new NetSuiteInvoiceCustbodyAtlasNewCustHdn
                {
                    ID = "1",
                    RefName = "New Customer"
                },
                CustbodyAtlasNoHdn = new NetSuiteInvoiceCustbodyAtlasNoHdn
                {
                    ID = "2",
                    RefName = "No"
                },
                CustbodyAtlasYesHdn = new NetSuiteInvoiceCustbodyAtlasYesHdn
                {
                    ID = "1",
                    RefName = "Yes"
                },
                CustbodyEmeaTransactionType = "custinvc",
                CustbodyEscCreatedDate = DateTime.Now.Format("yyyy-MM-dd"),
                CustbodyExternalID = $"ENR_{inv.EnrolmentID.ToString()}",
                CustbodyF3NextApprovalBy = new NetSuiteInvoiceCustbodyF3NextApprovalBy
                {
                    ID = "1351",
                    RefName = "BIMM - Financial Controller"
                },
                CustbodyReportTimestamp = DateTime.Now.Format("yyyy-MM-ddTHH:mm:ssZ"),
                CustbodySiiArticle61d = false,
                CustbodySiiArticle7273 = false,
                CustbodySiiIsThirdParty = false,
                CustbodySiiNotReportedInTime = false,
                CustbodyStcAmountAfterDiscount = decimal.ToDouble(inv.FeeGross ?? 0),
                CustbodyStcTaxAfterDiscount = 0,
                CustbodyStcTotalAfterDiscount = decimal.ToDouble(inv.FeeGross ?? 0),
                CustbodyZncGbpEquivNet = decimal.ToDouble(inv.FeeGross ?? 0),
                CustbodyZncGbpEquivTotal = decimal.ToDouble(inv.FeeGross ?? 0),
                CustbodyZncGbpEquivVat = 0,
                CustomForm = new NetSuiteInvoiceCustomForm
                {
                    ID = "251",
                    RefName = "BIMM - Invoice"
                },
                DiscountTotal = 0,
                DueDate = inv.StartDateEnrol?.AddDays(30).Format("yyyy-MM-dd"),
                Email = inv.EmailAddress,
                Entity = new NetSuiteInvoiceEntity
                {
                    ID = inv.NetSuiteCustomerID.ToString() //Ensure NetSuite Customer ID has been assigned to the UNIT-e Instance
                },
                EstGrossProfit = decimal.ToDouble(inv.FeeGross ?? 0),
                EstGrossProfitPercent = 100.0,
                ExchangeRate = 1.0,
                ExcludeFromGLNumbering = false,
                ExternalID = $"ENR_{inv.EnrolmentID.ToString()}",
                Location = new NetSuiteInvoiceLocation
                {
                    RefName = inv.CampusName
                },
                Memo = $"{(inv.AcademicYearName?.Length == 9? inv.AcademicYearName?.Substring(2, 2) : inv.AcademicYearName)}/{(inv.AcademicYearName?.Length == 9 ? inv.AcademicYearName?.Substring(7, 2) : inv.AcademicYearName)} INVOICE",
                NextApprover = new NetSuiteInvoiceNextApprover
                {
                    ID = "-1",
                    RefName = ""
                },
                Originator = "UNIT-e",
                PostingPeriod = new NetSuiteInvoicePostingPeriod
                {
                    RefName = inv.StartDateEnrol?.Format("MMM yyyy"),
                },
                PrevDate = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                SalesEffectiveDate = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                ShipAddress = inv.AddressMainEncoded,
                ShipAddressList = new NetSuiteInvoiceShipAddressList
                {
                    RefName = inv.AddressMainType
                },
                ShipIsResidential = true,
                ShipOverride = false,
                ShippingAddressText = inv.AddressMainEncoded,
                Source = new NetSuiteInvoiceSource
                {
                    RefName = "UNIT-e"
                },
                Status = new NetSuiteInvoiceStatus
                {
                    ID = "Open",
                    RefName = "Open"
                },
                Subsidiary = new NetSuiteInvoiceSubsidiary
                {
                    ID = "45",
                    RefName = "BIMM University Limited"
                },
                Subtotal = decimal.ToDouble(inv.FeeGross ?? 0),
                ToBeEmailed = false,
                ToBeFaxed = false,
                ToBePrinted = false,
                Total = decimal.ToDouble(inv.FeeGross ?? 0),
                TotalCostEstimate = 0,
                TranDate = inv.StartDateEnrol?.Format("yyyy-MM-dd")
            }).ToList<NetSuiteInvoice>();

            return netSuiteInvoices ?? new List<NetSuiteInvoice>();
        }

        public static IList<NetSuiteCreditMemo> MapUNITeCreditNotesToNetSuiteCreditMemos(IList<UNITeCreditNote> uniteCreditNotes)
        {
            IList<NetSuiteCreditMemo>? netSuiteCreditMemos = new List<NetSuiteCreditMemo>();
            int? numRecordsWithCustomerID = uniteCreditNotes.Where(f => f.NetSuiteCustomerID > 0).Count();

            if (numRecordsWithCustomerID != uniteCreditNotes.Count())
            {
                Console.WriteLine($"Not all credit note records are valid as {uniteCreditNotes.Count() - numRecordsWithCustomerID} do not have a customer ID assigned");
                return new List<NetSuiteCreditMemo>();
            }

            //Map UNIT-e Credit Notes to NetSuite Credit Memos
            netSuiteCreditMemos = uniteCreditNotes?.Select(re => new NetSuiteCreditMemo
            {
                ExternalID = $"CRM_{re.CourseCode?.Replace("/", "_")}"
            }).ToList<NetSuiteCreditMemo>();

            return netSuiteCreditMemos ?? new List<NetSuiteCreditMemo>();
        }

        public static IList<NetSuiteCustomerRefund> MapUNITeRefundsToNetSuiteCustomerRefunds(IList<UNITeRefund> uniteRefunds)
        {
            IList<NetSuiteCustomerRefund>? netSuiteCustomerRefunds = new List<NetSuiteCustomerRefund>();
            int? numRecordsWithCustomerID = uniteRefunds.Where(f => f.NetSuiteCustomerID > 0).Count();

            if (numRecordsWithCustomerID != uniteRefunds.Count())
            {
                Console.WriteLine($"Not all refund records are valid as {uniteRefunds.Count() - numRecordsWithCustomerID} do not have a customer ID assigned");
                return new List<NetSuiteCustomerRefund>();
            }

            //Map UNIT-e Refunds to NetSuite Customer Refunds
            netSuiteCustomerRefunds = uniteRefunds?.Select(re => new NetSuiteCustomerRefund
            {
                Memo = $"CRM_{re.CourseCode?.Replace("/", "_")}"
            }).ToList<NetSuiteCustomerRefund>();

            return netSuiteCustomerRefunds ?? new List<NetSuiteCustomerRefund>();
        }
    }
}
