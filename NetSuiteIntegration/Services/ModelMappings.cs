using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Shared;
using Serilog;

namespace NetSuiteIntegration.Services
{
    public static class ModelMappings
    {
        public static ICollection<UNITeStudent> MapUNITeEnrolmentsToUNITeStudents(ICollection<UNITeEnrolment> uniteEnrolments)
        {
            ICollection<UNITeStudent>? uniteStudents = new List<UNITeStudent>();

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
                    AcademicYearEndDate = stu.AcademicYearEndDate,
                    CourseID = stu.CourseID,
                    CampusCode = stu.CampusCode,
                    CampusName = stu.CampusName,
                    DepartmentCode = stu.DepartmentCode,
                    DepartmentName = stu.DepartmentName,
                    CourseCode = stu.CourseCode,
                    CourseTitle = stu.CourseTitle,
                    CourseTypeCode = stu.CourseTypeCode,
                    CourseTypeName = stu.CourseTypeName,
                    SubjectCode = stu.SubjectCode,
                    SubjectName = stu.SubjectName,
                    LevelCode = stu.LevelCode,
                    LevelName = stu.LevelName,
                    EnrolmentType = stu.EnrolmentType,
                    StartDateEnrol = stu.StartDateEnrol,
                    ExpectedEndDateEnrol = stu.ExpectedEndDateEnrol,
                    ActualEndDateEnrol = stu.ActualEndDateEnrol,
                    StartDateProgramme = stu.StartDateProgramme,
                    ExpectedEndDateProgramme = stu.ExpectedEndDateProgramme,
                    ActualEndDateProgramme = stu.ActualEndDateProgramme,
                    StartDateCourse = stu.StartDateCourse,
                    EndDateCourse = stu.EndDateCourse,
                    EnrolmentStatusCode = stu.EnrolmentStatusCode,
                    EnrolmentStatusName = stu.EnrolmentStatusName,
                    ProgressionCode = stu.ProgressionCode,
                    ProgressionName = stu.ProgressionName,
                    FeeGross = stu.FeeGross,
                    FeeNet = stu.FeeNet,
                    FeeDiscount = stu.FeeDiscount,
                }).ToList<UNITeStudent>();

            return uniteStudents ?? new List<UNITeStudent>();
        }

        public static ICollection<NetSuiteCustomer> MapUNITeStudentsToNetSuiteCustomers(ICollection<UNITeStudent> uniteStudents)
        {
            ICollection<NetSuiteCustomer>? netSuiteCustomers = new List<NetSuiteCustomer>();

            //Map UNIT-e Students to NetSuite Customers
            netSuiteCustomers = uniteStudents?.Select(cus => new NetSuiteCustomer
            {
                AlcoholRecipientType = new NetSuiteCustomerAlcoholRecipientType
                {
                    ID = "CONSUMER",
                    RefName = "Consumer"
                },
                Balance = 0.0,
                CreditHoldOverride = new NetSuiteCustomerCreditHoldOverride
                {
                    ID = "AUTO",
                    RefName = "Auto"
                },
                Currency = new NetSuiteCustomerCurrency
                {
                    ID = "1",
                    RefName = "GBP"
                },
                CustEntity2663CustomerRefund = false,
                CustEntity2663DirectDebit = false,
                CustEntityCRMApplicantID = cus.ERPID,
                CustEntityEscLastModifiedDate = DateTime.Now.Format("yyyy-MM-dd"),
                CustEntityF3Campus = new NetSuiteCustomerCustEntityF3Campus
                {
                    RefName = cus.CampusName
                },
                CustEntityF3StudentStatus = new NetSuiteCustomerCustEntityF3StudentStatus
                {
                    ID = cus.EnrolmentStatusCode,
                    RefName = cus.EnrolmentStatusName
                },
                CustEntityNawTransNeedApproval = false,
                CustEntityClientStudentNo = cus.StudentRef,
                CustomForm = new NetSuiteCustomerCustomForm
                {
                    ID = "163",
                    RefName = "BIMM - Customers"
                },
                DateCreated = DateTime.Now,
                DaysOverdue = 0,
                DRAccount = new NetSuiteCustomerDRAccount
                {
                    ID = "216",
                    RefName = "30602 Deferred Revenue"
                },
                Email = cus.EmailAddress,
                EmailPreference = new NetSuiteCustomerEmailPreference
                {
                    ID = "DEFAULT",
                    RefName = "Default"
                },
                EmailTransactions = false,
                ExternalID = cus.StudentRef,
                FaxTransactions = false,
                FirstName = cus.Forename,
                GlobalSubscriptionStatus = new NetSuiteCustomerGlobalSubscriptionStatus
                {
                    ID = "2",
                    RefName = "Soft Opt-Out"
                },
                IsAutogeneratedRepresentingEntity = false,
                IsBudgetApproved = false,
                IsInactive = false,
                IsPerson = true,
                Language = new NetSuiteCustomerLanguage
                {
                    ID = "en",
                    RefName = "English (International)"
                },
                LastName = cus.Surname,
                OverdueBalance = 0,
                Phone = cus.Mobile,
                PrintTransactions = false,
                ReceivablesAccount = new NetSuiteCustomerReceivablesAccount
                {
                    ID = "-10",
                    RefName = "Use System Preference"
                },
                ShipComplete = false,
                ShippingCarrier = new NetSuiteCustomerShippingCarrier
                {
                    ID = "nonups",
                    RefName = "FedEx/USPS/More"
                },
                Subsidiary = new NetSuiteCustomerSubsidiary
                {
                    RefName = cus.CampusName
                },
                SyncSalesTeams = false,
                UnbilledOrders = 0,
                Unsubscribe = false,
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
                    } : new NetSuiteAddressBook()
                },
                AcademicYearStartDate = cus.AcademicYearStartDate,
                AcademicYearEndDate = cus.AcademicYearEndDate,
                UNITeStudentID = cus.StudentID
            }).ToList<NetSuiteCustomer>();

            return netSuiteCustomers ?? new List<NetSuiteCustomer>();
        }

        public static ICollection<NetSuiteNonInventorySaleItem> MapUNITeCoursesToNetSuiteNonInventorySaleItems(ICollection<UNITeCourse> uniteCourses)
        {
            ICollection<NetSuiteNonInventorySaleItem>? netSuiteNonInventorySaleItems = new List<NetSuiteNonInventorySaleItem>();

            //Map UNIT-e Courses to NetSuite Non-Inventory Sale Items
            netSuiteNonInventorySaleItems = uniteCourses?.Select(crs => new NetSuiteNonInventorySaleItem
            {
                Class = new NetSuiteNonInventorySaleItemClass
                {
                    RefName = crs.CampusName
                },
                CreatedDate = DateTime.Now,
                CustItem1 = crs.StartDateCourse?.Format("yyyy-MM-dd"),
                CustItem2 = crs.EndDateCourse?.Format("yyyy-MM-dd"),
                CustItemIsPOItem = false,
                CustomForm = new NetSuiteNonInventorySaleItemCustomForm
                {
                    ID = "164",
                    RefName = "MetFilm - Non Inventory"
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
                    ID = crs.EnrolmentType == "PostGrad" ? "1233" : "3003",
                    RefName = crs.EnrolmentType == "PostGrad" ? 
                        "50120 Total Net Income : Total Fee Income : Fee Income - Postgrad"
                        : "50115 Total Net Income : Total Fee Income : Fee Income - Undergrad"
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
                AcademicYearStartDate = crs.AcademicYearStartDate,
                AcademicYearEndDate = crs.AcademicYearEndDate,
                UNITeCourseID = crs.CourseID
            }).ToList<NetSuiteNonInventorySaleItem>();

            return netSuiteNonInventorySaleItems ?? new List<NetSuiteNonInventorySaleItem>();
        }

        public static ICollection<NetSuiteInvoice> MapUNITeFeesToNetSuiteInvoices(ICollection<UNITeFee> uniteFees)
        {
            ICollection<NetSuiteInvoice>? netSuiteInvoices = new List<NetSuiteInvoice>();
            int? numRecordsWithCustomerID = uniteFees.Where(f => f.NetSuiteCustomerID?.Length >= 1).Count();

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
                Custbody15699ExcludeFromEPProcess = false,
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
                    ID = inv.NetSuiteCustomerID //Ensure NetSuite Customer ID has been assigned to the UNIT-e Instance
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
                    RefName = inv.CampusName
                },
                Subtotal = decimal.ToDouble(inv.FeeGross ?? 0),
                ToBeEmailed = false,
                ToBeFaxed = false,
                ToBePrinted = false,
                Total = decimal.ToDouble(inv.FeeGross ?? 0),
                TotalCostEstimate = 0,
                TranDate = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                AcademicYearStartDate = inv.AcademicYearStartDate,
                AcademicYearEndDate = inv.AcademicYearEndDate,
                Items = new List<NetSuiteInvoiceItemDetail>
                {
                    new NetSuiteInvoiceItemDetail
                    {
                        Amount = decimal.ToDouble(inv.FeeGross ?? 0),
                        Class = new NetSuiteInvoiceItemDetailClass
                        {
                            RefName = $"{inv.SubjectName}: {inv.CampusName}"
                        },
                        CostEstimate = 0,
                        CostEstimateRate = 0,
                        CostEstimateType = new NetSuiteInvoiceItemDetailCostEstimateType
                        {
                            ID = "ITEMDEFINED",
                            RefName = "ITEMDEFINED"
                        },
                        Custcol1 = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2 = inv.ExpectedEndDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2663IsPerson = false,
                        Custcol5892EUTriangulation = false,
                        CustcolStatisticalValueBaseCurr = 0,
                        Department = new NetSuiteInvoiceItemDetailDepartment
                        {
                            ID = "26",
                            RefName = "Income : Student Income"
                        },
                        Description = inv.CourseCode,
                        Item = new NetSuiteInvoiceItemDetailItem
                        {
                            ID = inv.NetSuiteNonInventorySaleItemID //Ensure NetSuite Course ID has been assigned to the UNIT-e Instance
                        },
                        ItemSubtype = new NetSuiteInvoiceItemDetailItemSubtype
                        {
                            ID = "Sale",
                            RefName = "Sale"
                        },
                        ItemType = new NetSuiteInvoiceItemDetailItemType
                        {
                            ID = "NonInvtPart",
                            RefName = "NonInvtPart"
                        },
                        //Line = 1,
                        Location = new NetSuiteInvoiceItemDetailLocation
                        {
                            RefName = inv.CampusName
                        },
                        Marginal = false,
                        Price = new NetSuiteInvoiceItemDetailPrice
                        {
                            ID = "-1",
                            RefName = ""
                        },
                        PrintItems = false,
                        Quantity = 1.0,
                        QuantityOnHand = 0.0,
                        Rate = decimal.ToDouble(inv.FeeGross ?? 0),
                        IsMainInvoiceLine = true
                    },
                    inv.FeeDiscount != null ? new NetSuiteInvoiceItemDetail
                    {
                        Amount = decimal.ToDouble(inv.FeeDiscount ?? 0),
                        Class = new NetSuiteInvoiceItemDetailClass
                        {
                            RefName = $"{inv.SubjectName}: {inv.CampusName}"
                        },
                        CostEstimate = 0,
                        CostEstimateRate = 0,
                        CostEstimateType = new NetSuiteInvoiceItemDetailCostEstimateType
                        {
                            ID = "ITEMDEFINED",
                            RefName = "ITEMDEFINED"
                        },
                        Custcol1 = inv.StartDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2 = inv.ExpectedEndDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2663IsPerson = false,
                        Custcol5892EUTriangulation = false,
                        CustcolStatisticalValueBaseCurr = 0,
                        Department = new NetSuiteInvoiceItemDetailDepartment
                        {
                            ID = "26",
                            RefName = "Income : Student Income"
                        },
                        Description = "CREDIT_TUT_GBP",
                        Item = new NetSuiteInvoiceItemDetailItem
                        {
                            ID = "5457" //50220 Discounts
                        },
                        ItemSubtype = new NetSuiteInvoiceItemDetailItemSubtype
                        {
                            ID = "Sale",
                            RefName = "Sale"
                        },
                        ItemType = new NetSuiteInvoiceItemDetailItemType
                        {
                            ID = "NonInvtPart",
                            RefName = "NonInvtPart"
                        },
                        //Line = 2,
                        Location = new NetSuiteInvoiceItemDetailLocation
                        {
                            RefName = inv.CampusName
                        },
                        Marginal = false,
                        Price = new NetSuiteInvoiceItemDetailPrice
                        {
                            ID = "-1",
                            RefName = ""
                        },
                        PrintItems = false,
                        Quantity = 1.0,
                        QuantityOnHand = 0.0,
                        Rate = decimal.ToDouble(inv.FeeDiscount ?? 0),
                        IsMainInvoiceLine = false
                    } : new NetSuiteInvoiceItemDetail()
                },
                UNITeStudentID = inv.StudentID,
                UNITeEnrolmentID = inv.EnrolmentID
            }).ToList<NetSuiteInvoice>();

            return netSuiteInvoices ?? new List<NetSuiteInvoice>();
        }

        public static ICollection<NetSuiteCreditMemo> MapUNITeCreditNotesToNetSuiteCreditMemos(ICollection<UNITeCreditNote> uniteCreditNotes)
        {
            ICollection<NetSuiteCreditMemo>? netSuiteCreditMemos = new List<NetSuiteCreditMemo>();
            int? numRecordsWithCustomerID = uniteCreditNotes.Where(f => f.NetSuiteCustomerID?.Length >= 1).Count();

            //Map UNIT-e Credit Notes to NetSuite Credit Memos
            netSuiteCreditMemos = uniteCreditNotes?.Select(cre => new NetSuiteCreditMemo
            {
                Account = new NetSuiteCreditMemoAccount
                {
                    ID = "456",
                    RefName = "20709 Debtors Control Account"
                },
                AmountPaid = 0,
                AmountRemaining = decimal.ToDouble(cre.FeeGross ?? 0),
                Applied = 0,
                AsOfDate = cre.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                BillAddress = cre.AddressMainEncoded,
                BillAddressList = new NetSuiteCreditMemoBillAddressList
                {
                    RefName = cre.AddressMainType
                },
                BillingAddressText = cre.AddressMainEncoded,
                CanHaveStackable = false,
                Class = new NetSuiteCreditMemoClass
                {
                    RefName = $"{cre.SubjectName}: {cre.CampusName}"
                },
                CreatedDate = DateTime.Now,
                Currency = new NetSuiteCreditMemoCurrency
                {
                    ID = "1",
                    RefName = "GBP"
                },
                Custbody15699ExcludeFromEPProcess = false,
                CustbodyAtlasExistCustHdn = new NetSuiteCreditMemoCustbodyAtlasExistCustHdn
                {
                    ID = "2",
                    RefName = "Existing Customer"
                },
                CustbodyAtlasNewCustHdn = new NetSuiteCreditMemoCustbodyAtlasNewCustHdn
                {
                    ID = "1",
                    RefName = "New Customer"
                },
                CustbodyAtlasNoHdn = new NetSuiteCreditMemoCustbodyAtlasNoHdn
                {
                    ID = "2",
                    RefName = "No"
                },
                CustbodyAtlasYesHdn = new NetSuiteCreditMemoCustbodyAtlasYesHdn
                {
                    ID = "1",
                    RefName = "Yes"
                },
                CustbodyEmeaTransactionType = "custcred",
                CustbodyEscCreatedDate = DateTime.Now.Format("yyyy-MM-dd"),
                CustbodyExternalID = $"ENR_{cre.EnrolmentID.ToString()}",
                CustbodyF3IntercompanyInternalVb = new NetSuiteCreditMemoCustbodyF3IntercompanyInternalVb
                {
                    ID = "2",
                    RefName = "No"
                },
                CustbodyReportTimestamp = DateTime.Now.Format("yyyy-MM-ddTHH:mm:ssZ"),
                CustbodySiiArticle61d = false,
                CustbodySiiArticle7273 = false,
                CustbodySiiIsThirdParty = false,
                CustbodySiiNotReportedInTime = false,
                CustbodyZncGbpEquivNet = decimal.ToDouble(cre.FeeGross ?? 0),
                CustbodyZncGbpEquivTotal = decimal.ToDouble(cre.FeeGross ?? 0),
                CustbodyZncGbpEquivVat = 0,
                CustomForm = new NetSuiteCreditMemoCustomForm
                {
                    ID = "210",
                    RefName = "BIMM - Credit Memo"
                },
                Department = new NetSuiteCreditMemoDepartment
                {
                    ID = "26",
                    RefName = "Income : Student Income"
                },
                DiscountTotal = 0,
                Email = cre.EmailAddress,
                Entity = new NetSuiteCreditMemoEntity
                {
                    ID = cre.NetSuiteCustomerID //Ensure NetSuite Customer ID has been assigned to the UNIT-e Instance
                },
                EstGrossProfit = decimal.ToDouble(cre.FeeGross ?? 0),
                EstGrossProfitPercent = 100.0,
                ExchangeRate = 1.0,
                ExcludeFromGLNumbering = false,
                ExternalID = $"ENR_{cre.EnrolmentID.ToString()}",
                Location = new NetSuiteCreditMemoLocation
                {
                    RefName = cre.CampusName
                },
                Memo = $"{cre.CourseCode}, CREDIT_TUT_GBP, {(cre.Forename?.Length >= 3? cre.Forename?.Substring(0, 3) : cre.Forename)}{(cre.Surname?.Length >= 3 ? cre.Surname?.Substring(0, 3) : cre.Surname)}, UNIT-e Ref={cre.StudentRef}",
                Originator = "UNIT-e",
                PostingPeriod = new NetSuiteCreditMemoPostingPeriod
                {
                    RefName = cre.ActualEndDateEnrol?.Format("MMM yyyy"),
                },
                PrevDate = cre.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                SalesEffectiveDate = cre.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                ShipAddress = cre.AddressMainEncoded,
                ShipAddressList = new NetSuiteCreditMemoShipAddressList
                {
                    RefName = cre.AddressMainType
                },
                ShipIsResidential = true,
                ShipOverride = false,
                ShippingAddressText = cre.AddressMainEncoded,
                Source = new NetSuiteCreditMemoSource
                {
                    RefName = "UNIT-e"
                },
                Status = new NetSuiteCreditMemoStatus
                {
                    ID = "Open",
                    RefName = "Open"
                },
                Subsidiary = new NetSuiteCreditMemoSubsidiary
                {
                    RefName = cre.CampusName
                },
                Subtotal = decimal.ToDouble(cre.FeeGross ?? 0),
                ToBeEmailed = false,
                ToBeFaxed = false,
                ToBePrinted = false,
                Total = decimal.ToDouble(cre.FeeGross ?? 0),
                TotalCostEstimate = 0,
                TranDate = cre.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                Unapplied = decimal.ToDouble(cre.FeeGross ?? 0),
                AcademicYearStartDate = cre.AcademicYearStartDate,
                AcademicYearEndDate = cre.AcademicYearEndDate,
                Items = new List<NetSuiteCreditMemoItemDetail>
                {
                    new NetSuiteCreditMemoItemDetail
                    {
                        Account = new NetSuiteCreditMemoItemDetailAccount
                        {
                            ID = "2950",
                            RefName = "2950"
                        },
                        Amount = decimal.ToDouble(cre.FeeGross ?? 0),
                        CostEstimate = 0.0,
                        CostEstimateRate = 0.0,
                        CostEstimateType = new NetSuiteCreditMemoItemDetailCostEstimateType
                        {
                            ID = "ITEMDEFINED",
                            RefName = "ITEMDEFINED"
                        },
                        Custcol1 = cre.StartDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2 = cre.ExpectedEndDateEnrol?.Format("yyyy-MM-dd"),
                        Custcol2663IsPerson = false,
                        Custcol5892EUTriangulation = false,
                        CustcolStatisticalValueBaseCurr = 0,
                        Department = new NetSuiteCreditMemoItemDetailDepartment
                        {
                            ID = "46",
                            RefName = "Central : Finance"
                        },
                        Description = cre.CourseCode,
                        IsDropShipment = false,
                        Item = new NetSuiteCreditMemoItemDetailItem
                        {
                            ID = "5307",
                            RefName = "Opening Balance Item - AR-For Upload"
                        },
                        ItemSubtype = new NetSuiteCreditMemoItemDetailItemSubtype
                        {
                            ID = "Sale",
                            RefName = "Sale"
                        },
                        ItemType = new NetSuiteCreditMemoItemDetailItemType
                        {
                            ID = "NonInvtPart",
                            RefName = "NonInvtPart"
                        },
                        //Line = 1,
                        Location = new NetSuiteCreditMemoItemDetailLocation
                        {
                            RefName = cre.CampusName
                        },
                        Marginal = false,
                        Price = new NetSuiteCreditMemoItemDetailPrice
                        {
                            ID = "-1",
                            RefName = ""
                        },
                        PrintItems = false,
                        Quantity = 1.0,
                        IsMainCreditMemoLine = true
                    }
                },
                UNITeStudentID = cre.StudentID,
                UNITeEnrolmentID = cre.EnrolmentID
            }).ToList<NetSuiteCreditMemo>();

            return netSuiteCreditMemos ?? new List<NetSuiteCreditMemo>();
        }

        public static ICollection<NetSuiteCustomerRefund> MapUNITeRefundsToNetSuiteCustomerRefunds(ICollection<UNITeRefund> uniteRefunds)
        {
            ICollection<NetSuiteCustomerRefund>? netSuiteCustomerRefunds = new List<NetSuiteCustomerRefund>();
            int? numRecordsWithCustomerID = uniteRefunds.Where(f => f.NetSuiteCustomerID?.Length >= 1).Count();

            //Map UNIT-e Refunds to NetSuite Customer Refunds
            netSuiteCustomerRefunds = uniteRefunds?.Select(re => new NetSuiteCustomerRefund
            {
                Account = new NetSuiteCustomerRefundAccount
                {
                    ID = "2760",
                    RefName = "20105 Total Bank and Cash in Hand : BIMM UNIVERSITY LIMI 28187253"
                },
                Address = "Sage Balance Account (LTD)\nBIMM Central - 2 Bartholomews\nBristol  BN1 1HG\nUnited Kingdom",
                Aracct = new NetSuiteCustomerRefundAracct
                {
                    ID = "456",
                    RefName = "20709 Debtors Control Account"
                },
                Balance = decimal.ToDouble(re.FeeGross ?? 0),
                Cleared = false,
                ClearedDate = null,
                CreatedDate = DateTime.Now,
                Currency = new NetSuiteCustomerRefundCurrency
                {
                    ID = "1",
                    RefName = "GBP"
                },
                Custbody9997AutocashAssertionField = false,
                Custbody9997IsForEpDd = false,
                CustbodyAtlasNoHdn = new NetSuiteCustomerRefundCustbodyAtlasNoHdn
                {
                    ID = "2",
                    RefName = "No"
                },
                CustbodyAtlasYesHdn = new NetSuiteCustomerRefundCustbodyAtlasYesHdn
                {
                    ID = "1",
                    RefName = "Yes"
                },
                CustbodyAznCurrentUser = new NetSuiteCustomerRefundCustbodyAznCurrentUser
                {
                    ID = "90",
                    RefName = "Malcolm Weller"
                },
                Customer = new NetSuiteCustomerRefundCustomer
                {
                    ID = re.NetSuiteCustomerID //Ensure NetSuite Customer ID has been assigned to the UNIT-e Instance
                },
                CustomForm = new NetSuiteCustomerRefundCustomForm
                {
                    ID = "41",
                    RefName = "Standard Customer Refund"
                },
                ExchangeRate = 1.0,
                ExcludeFromGLNumbering = false,
                Memo = $"{(re.Forename?.Length >= 1 ? re.Forename?.Substring(0, 1) : re.Forename)?.ToUpper()} {re.Surname?.ToUpper()} REFUND",
                PayeeAddressText = "Sage Balance Account (LTD)\nBIMM Central - 2 Bartholomews\nBristol  BN1 1HG\nUnited Kingdom",
                PayeeAddressList = new NetSuiteCustomerRefundPayeeAddressList
                {
                    ID = "175198",
                    RefName = "LTD"
                },
                PaymentOperation = new NetSuiteCustomerRefundPaymentOperation
                {
                    ID = "CREDIT",
                    RefName = "Credit"
                },
                PostingPeriod = new NetSuiteCustomerRefundPostingPeriod
                {
                    RefName = re.ActualEndDateEnrol?.Format("MMM yyyy"),
                },
                PrevDate = re.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                Subsidiary = new NetSuiteCustomerRefundSubsidiary
                {
                    RefName = re.CampusName
                },
                ToBePrinted = false,
                Total = decimal.ToDouble(re.FeeGross ?? 0),
                TranDate = re.ActualEndDateEnrol?.Format("yyyy-MM-dd"),
                AcademicYearStartDate = re.AcademicYearStartDate,
                AcademicYearEndDate = re.AcademicYearEndDate,
                UNITeStudentID = re.StudentID,
                UNITeEnrolmentID = re.EnrolmentID
            }).ToList<NetSuiteCustomerRefund>();

            return netSuiteCustomerRefunds ?? new List<NetSuiteCustomerRefund>();
        }
    }
}
