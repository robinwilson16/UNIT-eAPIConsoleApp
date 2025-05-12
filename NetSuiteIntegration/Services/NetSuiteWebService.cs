using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UNITe.XMLExporter.Entities;

namespace NetSuiteIntegration.Services
{
    public class NetSuiteWebService(ILogger log, ApplicationSettings applicationSettings) : IFinanceWebService
    {
        ApplicationSettings _Settings = applicationSettings;
        ILogger _Log = log;

        public async Task<T?> GetNetSuiteRecord<T>(string? recordType, int? recordID)
        {
            T? reportData = default;

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");


                string? recordURL = $"record/v1/{recordType}/{recordID}";
                //string? recordURL = $"https://7383276-sb1.suitetalk.api.netsuite.com/services/rest/record/v1/customer/{recordID}";
                //reportData = await _httpClient.GetFromJsonAsync<T>(recordURL);
                HttpResponseMessage response = await _httpClient.GetAsync(recordURL);

                if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    _Log.Error($"Failed to load the {recordType} record {recordID}");
                    return default;
                }
            }
            catch (HttpRequestException e)
            {
                _Log.Error($"Failed to load the {recordType} record {recordID}");
                return default;
            }
        }
    }
}
