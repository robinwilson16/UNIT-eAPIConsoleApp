using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetSuiteIntegration.Data;
using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Serilog;

namespace NetSuiteIntegration.Services
{
    public class NetSuiteLookups(NetsuiteContext dbContext, ISRSWebServicecs unite, IFinanceWebService netsuite, ILogger logger)
    {
        NetsuiteContext _dbContext = dbContext;
        ISRSWebServicecs? _unite = unite;
        IFinanceWebService? _netsuite = netsuite;
        ILogger? _log = logger;

        public async Task<ICollection<T>?> GetCampusMappings<T>(ICollection<T> uniteObjects)
        {
            //Get the UNIT-e Campus Mappings for NetSuite

            ICollection<LookupCampus>? lookupCampuses = new List<LookupCampus>();
            string? uniteCampusCode = string.Empty;
            LookupCampus? matchedCampus = new LookupCampus();

            if (_dbContext != null)
            {
                lookupCampuses = await _dbContext
                    .LookupCampus
                .AsNoTracking()
                .ToListAsync();

                _log?.Information($"Loaded {lookupCampuses?.Count} UNIT-e Campus Mappings from NetSuite");

                if (uniteObjects != null)
                {
                    foreach (T? obj in uniteObjects)
                    {
                        uniteCampusCode = obj?.GetType().GetProperty("CampusFromCourseCode")?.GetValue(obj)?.ToString();

                        matchedCampus = lookupCampuses?
                            .Where(c => c.UNITeCampusCode == uniteCampusCode)
                            .FirstOrDefault();

                        //Get and set the NetSuite properties on the UNIT-e object
                        var NetSuiteLocationID = obj?.GetType().GetProperty("NetSuiteLocationID");
                        if (NetSuiteLocationID != null && NetSuiteLocationID.CanWrite)
                        {
                            NetSuiteLocationID.SetValue(obj, matchedCampus?.NetSuiteLocationID);
                        }
                        var NetSuiteLocationName = obj?.GetType().GetProperty("NetSuiteLocationName");
                        if (NetSuiteLocationName != null && NetSuiteLocationName.CanWrite)
                        {
                            NetSuiteLocationName.SetValue(obj, matchedCampus?.NetSuiteLocationName);
                        }
                        var NetSuiteSubsiduaryID = obj?.GetType().GetProperty("NetSuiteSubsiduaryID");
                        if (NetSuiteSubsiduaryID != null && NetSuiteSubsiduaryID.CanWrite)
                        {
                            NetSuiteSubsiduaryID.SetValue(obj, matchedCampus?.NetSuiteSubsiduaryID);
                        }
                        var NetSuiteFacultyID = obj?.GetType().GetProperty("NetSuiteFacultyID");
                        if (NetSuiteFacultyID != null && NetSuiteFacultyID.CanWrite)
                        {
                            NetSuiteFacultyID.SetValue(obj, matchedCampus?.NetSuiteFacultyID);
                        }
                    }
                }
            }
            else
            {
                _log?.Error("Error Loading UNIT-e Campus Mappings.");
            }
            return uniteObjects;
        }

        public async Task<ICollection<T>?> GetCountryMappings<T>(ICollection<T> uniteObjects)
        {
            //Get the UNIT-e Country Mappings for NetSuite

            ICollection<LookupCountry>? lookupCountries = new List<LookupCountry>();
            string? uniteCountryCodeMain = string.Empty;
            string? uniteCountryCodeTermTime = string.Empty;
            string? uniteCountryCodeHome = string.Empty;
            string? uniteCountryCodeInvoice = string.Empty;
            LookupCountry? matchedCountryMain = new LookupCountry();
            LookupCountry? matchedCountryTermTime = new LookupCountry();
            LookupCountry? matchedCountryHome = new LookupCountry();
            LookupCountry? matchedCountryInvoice = new LookupCountry();

            if (_dbContext != null)
            {
                lookupCountries = await _dbContext
                    .LookupCountry
                .AsNoTracking()
                .ToListAsync();

                _log?.Information($"Loaded {lookupCountries?.Count} UNIT-e Country Mappings from NetSuite");

                if (uniteObjects != null)
                {
                    foreach (T? obj in uniteObjects)
                    {
                        uniteCountryCodeMain = obj?.GetType().GetProperty("CountryCodeMain")?.GetValue(obj)?.ToString();
                        uniteCountryCodeTermTime = obj?.GetType().GetProperty("CountryCodeTermTime")?.GetValue(obj)?.ToString();
                        uniteCountryCodeHome = obj?.GetType().GetProperty("CountryCodeHome")?.GetValue(obj)?.ToString();
                        uniteCountryCodeInvoice = obj?.GetType().GetProperty("CountryCodeInvoice")?.GetValue(obj)?.ToString();

                        matchedCountryMain = lookupCountries?
                            .Where(c => c.UNITeCountryCode == uniteCountryCodeMain)
                            .FirstOrDefault();
                        matchedCountryTermTime = lookupCountries?
                            .Where(c => c.UNITeCountryCode == uniteCountryCodeTermTime)
                            .FirstOrDefault();
                        matchedCountryHome = lookupCountries?
                            .Where(c => c.UNITeCountryCode == uniteCountryCodeHome)
                            .FirstOrDefault();
                        matchedCountryInvoice = lookupCountries?
                            .Where(c => c.UNITeCountryCode == uniteCountryCodeInvoice)
                            .FirstOrDefault();

                        //Get and set the NetSuite properties on the UNIT-e object
                        var NetSuiteCountryNameMain = obj?.GetType().GetProperty("NetSuiteCountryNameMain");
                        if (NetSuiteCountryNameMain != null && NetSuiteCountryNameMain.CanWrite)
                        {
                            NetSuiteCountryNameMain.SetValue(obj, matchedCountryMain?.NetSuiteCountryName);
                        }
                        var NetSuiteCountryNameTermTime = obj?.GetType().GetProperty("NetSuiteCountryNameTermTime");
                        if (NetSuiteCountryNameTermTime != null && NetSuiteCountryNameTermTime.CanWrite)
                        {
                            NetSuiteCountryNameTermTime.SetValue(obj, matchedCountryTermTime?.NetSuiteCountryName);
                        }
                        var NetSuiteCountryNameHome = obj?.GetType().GetProperty("NetSuiteCountryNameHome");
                        if (NetSuiteCountryNameHome != null && NetSuiteCountryNameHome.CanWrite)
                        {
                            NetSuiteCountryNameHome.SetValue(obj, matchedCountryHome?.NetSuiteCountryName);
                        }
                        var NetSuiteCountryNameInvoice = obj?.GetType().GetProperty("NetSuiteCountryNameInvoice");
                        if (NetSuiteCountryNameInvoice != null && NetSuiteCountryNameInvoice.CanWrite)
                        {
                            NetSuiteCountryNameInvoice.SetValue(obj, matchedCountryInvoice?.NetSuiteCountryName);
                        }
                    }
                }
            }
            else
            {
                _log?.Error("Error Loading UNIT-e Country Mappings.");
            }
            return uniteObjects;
        }
    }
}
