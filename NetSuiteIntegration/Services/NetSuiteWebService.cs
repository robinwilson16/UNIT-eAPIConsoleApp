﻿using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using NetSuiteIntegration.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UNITe.XMLExporter.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetSuiteIntegration.Services
{
    public class NetSuiteWebService(ILogger log, ApplicationSettings applicationSettings) : IFinanceWebService
    {
        ApplicationSettings _Settings = applicationSettings;
        ILogger _Log = log;

        /// <inheritdoc />
        public async Task<T?> Get<T>(string? objectType, int? objectID)
        {
            T? reportData = default(T);

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");


                string? objectURL = $"record/v1/{objectType}/{objectID}";
                //string? recordURL = $"https://7383276-sb1.suitetalk.api.netsuite.com/services/rest/record/v1/customer/{objectID}";
                //reportData = await _httpClient.GetFromJsonAsync<T>(recordURL);
                HttpResponseMessage response = await _httpClient.GetAsync(objectURL);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to get the {objectType} record {objectID} due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to get the {objectType} record {objectID} due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<T?> GetAll<T>(string? objectType)
        {
            T? reportData = default;

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");


                string? objectURL = $"record/v1/{objectType}";
                HttpResponseMessage response = await _httpClient.GetAsync(objectURL);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to get the {objectType} records due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to get the {objectType} records due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<T?> Search<T>(string? objectType, IList<NetSuiteSearchParameter> searchParameters)
        {
            //Usage:
            //?q=companyName EMPTY
            //?q=id BETWEEN_NOT [1, 42]
            //?q=id ANY_OF [1, 2, 3, 4, 5]
            //?q=email START_WITH barbara
            //?q=companyname START_WITH "Another Company"
            //?q=isinactive IS true
            //?q=dateCreated ON_OR_AFTER "1/1/2019" AND dateCreated BEFORE "1/1/2020"
            //?q=creditlimit GREATER_OR_EQUAL 1000 OR creditlimit LESS_OR_EQUAL 10
            //Use parenthesis to group the search parameters

            bool debug = false;

            T? reportData = default;
            int? numOpeningParenthesis = 0;
            int? numClosingParenthesis = 0;

            try
            {
                string? searchParamsString = string.Empty;

                if (searchParameters == null || searchParameters.Count == 0)
                {
                    _Log.Error($"The search parameters for {objectType} are not valid");
                    return default(T);
                }

                if (searchParameters != null)
                {
                    int paramNum = 0;
                    foreach (NetSuiteSearchParameter? param in searchParameters)
                    {
                        if (param?.FieldName != null && param?.Operator != null)
                        {
                            paramNum++;

                            //If first parameter then add ?q= to the start of the search string otherwise add the operand (AND/OR)
                            string? joiningCharacter = "";

                            if (paramNum == 1)
                            {
                                joiningCharacter = "?q=";
                            }
                            else
                            {
                                //If operand not specified then default to AND
                                if (param?.Operand == null)
                                {
                                    param!.Operand = Operand.AND;
                                }
                                joiningCharacter = $" {param?.Operand} ";
                            }

                            //The search parameters can contain opening and closing parenthesis to group the search parameters
                            //The number of opening and closing parenthesis must match otherwise the search will fail (below)
                            string? queryPrefix = string.Empty;
                            string? querySuffix = string.Empty;

                            string? valuePrefix = string.Empty;
                            string? valueSuffix = string.Empty;

                            if (param?.IncludeOpeningParenthesis == true)
                            {
                                queryPrefix = "(";
                                numOpeningParenthesis++;
                            }

                            if (param?.IncludeClosingParenthesis == true)
                            {
                                querySuffix = ")";
                                numClosingParenthesis++;
                            }

                            //If value is a string or a date then need to add quotes around it so characters such as @ in email do not cause an error
                            string? operatorDisplayName = param?.Operator?.GetEnumDisplayName();

                            if (operatorDisplayName?.Contains("Text") == true || operatorDisplayName?.Contains("Date/Time") == true)
                            {
                                valuePrefix = "\"";
                                valueSuffix = "\"";
                            }

                            if (param?.Value != null)
                            {
                                searchParamsString += $"{joiningCharacter}{queryPrefix}{param?.FieldName} {param?.Operator} {valuePrefix}{param?.Value}{valueSuffix}{querySuffix}";
                            }
                            else
                            {
                                searchParamsString += $"{joiningCharacter}{queryPrefix}{param?.FieldName} {param?.Operator}{querySuffix}";
                            }
                        }
                    }
                }

                if (numOpeningParenthesis != numClosingParenthesis)
                {
                    _Log.Error($"The search parameters for {objectType} are not valid. The number of opening and closing parenthesis do not match");
                    return default(T);
                }

                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");


                string? objectURL = $"record/v1/{objectType}{searchParamsString}";

                if (debug == true)
                {
                    _Log.Information($"Searching for {objectType} with URL: {objectURL}");
                }

                HttpResponseMessage response = await _httpClient.GetAsync(objectURL);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to get the {objectType} records due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to get the {objectType} records due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<T?> SearchSQL<T>(string? objectType, NetSuiteSQLQuery sqlQuery)
        {
            //objectType is only used for display purposes and for consistency with other methods
            //Usage
            //SELECT T.* FROM Transaction T WHERE T.Entity = 88563 AND T.AbbrevType = 'INV' AND T.TranDate >= '01/09/2024' AND T.TranDate <= '31/07/2025'
            bool debug = false;

            T? reportData = default;

            if (sqlQuery == null || string.IsNullOrEmpty(sqlQuery.Q))
            {
                _Log.Error($"The SQL query for {objectType} is not valid");
                return default(T);
            }

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");
                _httpClient.DefaultRequestHeaders.Add("Prefer", "transient");

                string? objectURL = $"query/v1/suiteql";

                if (debug == true)
                {
                    _Log.Information($"Searching for {objectType} with URL: {objectURL}");
                }

                //string sqlQueryString = JsonSerializer.Serialize(sqlQuery);
                //HttpContent httpContent = new StringContent(sqlQueryString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(objectURL, sqlQuery);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                else if (response.IsSuccessStatusCode)
                {
                    reportData = await response.Content.ReadFromJsonAsync<T>();
                    return reportData;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to get the {objectType} records due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to get the {objectType} records due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<T?> Add<T>(string? objectType, T? newObject)
        {
            T? returnedObject = default;

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");

                if (newObject == null)
                {
                    _Log.Error($"The new {objectType} record does not contain any data");
                    return default(T);
                }

                string? recordURL = $"record/v1/{objectType}";
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync<T>(recordURL, newObject!);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    //If no content then return the new object sent
                    return newObject;
                }
                else if (response.IsSuccessStatusCode)
                {
                    returnedObject = await response.Content.ReadFromJsonAsync<T>();
                    return returnedObject;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to add the new {objectType} record due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to load add the new {objectType} record due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<T?> Update<T>(string? objectType, int? objectID, T? updatedObject)
        {
            T? existingObject = default;
            T? returnedObject = default;

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");

                existingObject = await Get<T>(objectType, objectID);

                //Make sure object to be updated exists
                if (existingObject == null)
                {
                    _Log.Error($"The {objectType} record {objectID} does not exist");
                    return default(T);
                }
                else if (updatedObject == null)
                {
                    _Log.Error($"The updated {objectType} record for {objectID} does not contain any data");
                    return default(T);
                }

                string? objectURL = $"record/v1/{objectType}/{objectID}";
                //Put requires objects to be updated to be specified with this API so using Patch instead which works in the normal way
                //If only certain fields are included the rest are left intact and not blanked as is often the case with other APIs
                //HttpResponseMessage response = await _httpClient.PutAsJsonAsync<T>(objectURL, updatedObject!); 
                HttpResponseMessage response = await _httpClient.PatchAsJsonAsync<T>(objectURL, updatedObject!);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    //If no content then return the updated object sent
                    return updatedObject;
                }
                else if (response.IsSuccessStatusCode)
                {
                    returnedObject = await response.Content.ReadFromJsonAsync<T>();
                    return returnedObject;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to update the {objectType} record {objectID} due to error ({response.StatusCode}): {errorMessage}");
                    return default(T);
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to update the {objectType} record {objectID} due to error {ex.Message}");
                return default(T);
            }
        }

        /// <inheritdoc />
        public async Task<bool?> Delete<T>(string? objectType, int? objectID)
        {
            T? existingObject = default;

            try
            {
                HttpClient _httpClient = new HttpClient(new OAuth1Handler(_Settings));
                _httpClient.BaseAddress = new Uri(_Settings.NetSuiteURL ?? "");

                existingObject = await Get<T>(objectType, objectID);

                //Make sure object to be updated exists
                if (existingObject == null)
                {
                    _Log.Error($"The {objectType} record {objectID} does not exist");
                    return false;
                }

                string? objectURL = $"record/v1/{objectType}/{objectID}";
                HttpResponseMessage response = await _httpClient.DeleteAsync(objectURL);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    //If no content this is also valid
                    return true;
                }
                else if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    string? errorMessage = response.Content.ReadAsStringAsync().Result;
                    _Log.Error($"Failed to update the {objectType} record {objectID} due to error ({response.StatusCode}): {errorMessage}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _Log.Error($"Failed to update the {objectType} record {objectID} due to error {ex.Message}");
                return false;
            }
        }
    }
}
