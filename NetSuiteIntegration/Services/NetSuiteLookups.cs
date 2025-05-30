using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IList<T>?> GetCampusMappings<T>(IList<T> uniteObjects)
        {
            //Get the UNIT-e Campus Mappings for NetSuite
            //T? uniteObjects = default;

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

                        var NetSuiteLocationID = obj?.GetType().GetProperty("NetSuiteLocationID");
                        if (NetSuiteLocationID != null && NetSuiteLocationID.CanWrite)
                        {
                            NetSuiteLocationID.SetValue(obj, matchedCampus?.NetSuiteLocationID);
                        }
                    }
                }

                //uniteCampusCode = uniteObject?.GetType().GetProperty("CampusFromCourseCode")?.GetValue(uniteObject)?.ToString();

                //matchedCampus = lookupCampuses?
                //    .Where(c => c.UNITeCampusCode == uniteCampusCode)
                //    .FirstOrDefault();

                //var property = uniteObject?.GetType().GetProperty("NetSuiteLocationID");
                //if (property != null && property.CanWrite)
                //{
                //    property.SetValue(uniteObject, matchedCampus?.NetSuiteLocationID);
                //}
            }
            else
            {
                _log?.Error("Error Loading UNIT-e Campus Mappings.");
            }
            return uniteObjects;
        }
    }
}
