﻿using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
//using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
//using Formatting = Newtonsoft.Json.Formatting;

namespace NetSuiteIntegration.Services
{
    public class UniteWebService(ILogger log, ApplicationSettings applicationSettings) : ISRSWebServicecs
    {
        ApplicationSettings _Settings = applicationSettings;
        ILogger _Log = log;

        #region Utility Methods

        /// <inheritdoc />
        public async Task<string?> GetGuid()
        {
            string apiToken = string.Empty;

            try
            {
                HttpClient _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _Settings.UniteAPIKey);
                apiToken = await _httpClient.GetStringAsync(_Settings.UniteTokenURL);
                return apiToken;
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to get GUID due to error {ex.Message}", ex.Message);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> InvalidateSession(string guid)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync("invalidatesession");
                string jsonString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    bool jsonBool = false;
                    bool.TryParse(jsonString, out jsonBool);
                    return jsonBool;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to invalidate session due to error ({response.StatusCode}): {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Warning($"Failed to invalidate session due to error {ex.Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T?> ExportReport<T>(string reportName)
        {
            T? reportData;

            try
            {
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return default;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync($"report/export/json/{reportName}");
                await InvalidateSession(guid);


                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    //If no records then this is not necessarily an error
                    _Log.Information($"No records found for report {reportName}. Skipping.");
                    return default;
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to export report {reportName} due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to export report {reportName} due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<T?> ExportReport<T>(string reportName, string? guid)
        {
            T? reportData;

            try
            {
                if (guid == null)
                {
                    guid = await GetGuid();
                }

                if (guid == null)
                {
                    return default;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync($"report/export/json/{reportName}");
                //await InvalidateSession(guid);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    //If no records then this is not necessarily an error
                    _Log.Information($"No records found for report {reportName}. Skipping.");
                    return default;
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to export report {reportName} due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to export report {reportName} due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<DataSet?> ExportReportDataSet(string reportName)
        {
            DataTable? dataTable = new DataTable();


            try
            {
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return null;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                HttpResponseMessage response = await _httpClient.GetAsync($"report/export/json/{reportName}");
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    dataTable = await response.Content.ReadFromJsonAsync<DataTable>();
                    DataSet ds = new DataSet();
                    ds.Tables.Add(dataTable ?? new DataTable());
                    return ds;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to export report {reportName} due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to export report {reportName} due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <summary>
        /// Serialises an object to a string content object for use in a POST request
        /// </summary>
        /// <typeparam name="T">Unit-e Business Class</typeparam>
        /// <param name="t">Object to serialise</param>
        /// <returns></returns>
        private StringContent SerialiseObject<T>(T t)
        {
            string jsonAddress = JsonSerializer.Serialize(t);
            //fix issues when working between .Net Framework and .Net. Not sure if this is still needed.
            jsonAddress = jsonAddress.Replace("System.Private.CoreLib", "mscorlib");
            var prestringContent = HttpUtility.UrlEncode(jsonAddress);
            StringContent stringContent = new StringContent("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");
            return stringContent;
        }

        #endregion

        #region Generic CRUD methods

        //These make the assumption that the class name of the type in Unit-e is the same as the class name in the endpoint
        //which seems to be the case in practice. If not then overloaded versions of these methods would be needed

        /// <inheritdoc />
        public async Task<T?> Get<T>(string id)
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return default;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync($"class/get/{resource}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var entity = await response.Content.ReadFromJsonAsync<T>();
                    return entity;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to get record due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to get record due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<T?> Create<T>()
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return default;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync($"class/create/{resource}/");
                if (response.IsSuccessStatusCode)
                {
                    var blankrecord = await response.Content.ReadFromJsonAsync<T>();
                    return blankrecord;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to create record due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to create record due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Insert<T>(T insert)
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return false;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                string jsonAddress = JsonSerializer.Serialize(insert);
                var prestringContent = HttpUtility.UrlEncode(jsonAddress);
                StringContent stringContent = new StringContent("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync($"class/insert/{resource}", stringContent);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to insert record due to error ({response.StatusCode}): {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to insert record due to error {ex.Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<List<T>> Find<T, U>(U FindParameter)
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return default;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                StringContent stringContent = SerialiseObject(FindParameter);

                var response = await _httpClient.PostAsync($"class/find/{resource}", stringContent);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    List<T>? foundrecords = await response.Content.ReadFromJsonAsync<List<T>?>();
                    return foundrecords ?? new List<T>();
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to find record due to error ({response.StatusCode}): {errorMessage}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to find record due to error {ex.Message}", ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<bool> Update<T>(T record)
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string guid = await GetGuid();
                if (guid == null)
                {
                    return false;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                string jsonAddress = JsonSerializer.Serialize(record);

                //Encode any special characters in the JSON string
                var prestringContent = HttpUtility.UrlEncode(jsonAddress);

                //Correctly format the string content for the POST request as using application/x-www-form-urlencoded
                StringContent stringContent = new StringContent("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");

                //Update unite
                var response = await _httpClient.PostAsync($"class/update/{resource}", stringContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to update record due to error ({response.StatusCode}): {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to update record due to error {ex.Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateProperties<T, U>(U record)
        {
            try
            {
                string resource = typeof(T).Name.ToLower();
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return false;
                }

                HttpClient _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                //May need to not include type information
                string jsonAddress = JsonSerializer.Serialize(record);
                jsonAddress = jsonAddress.Replace("System.Private.CoreLib", "mscorlib");
                string prestringContent = HttpUtility.UrlEncode(jsonAddress);
                StringContent stringContent = new("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");


                //Update unite
                HttpResponseMessage response = await _httpClient.PostAsync($"class/updateproperty/{resource}/", stringContent);

                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to update record due to error ({response.StatusCode}): {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error($"Failed to update record due to error {ex.Message}", ex.Message);
                return false;
            }
        }

        #endregion
    }
}
