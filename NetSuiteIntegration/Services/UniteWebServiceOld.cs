using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using System.Text;
using System.Web;
using Formatting = Newtonsoft.Json.Formatting;

namespace NetSuiteIntegration.Services
{
    public class UniteWebServiceOld(ILogger log, ApplicationSettings applicationSettings) : ISRSWebServicecs
    {
        ApplicationSettings _Settings = applicationSettings;
        ILogger _Log = log;

        #region Utility Methods

        /// <inheritdoc />
        public async Task<string?> GetGuid()
        {
            try
            {
                var _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", _Settings.UniteAPIKey);
                var result = await _httpClient.GetAsync(_Settings.UniteTokenURL);
                return result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to get GUID", ex.Message);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> InvalidateSession(string guid)
        {
            try
            {
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync("invalidatesession");
                string jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    return Convert.ToBoolean(jsonString);
                }
                else
                {
                    _Log.Error("Failed to invalidate session");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Warning("Failed to invalidate session", ex.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T?> ExportReport<T>(string reportName)
        {
            try
            {
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return default;
                }

                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync("report/export/json/" + reportName);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                else
                {
                    _Log.Error("Failed to export report " + reportName);
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to export report " + reportName, ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<T?> ExportReport<T>(string reportName, string? guid)
        {
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

                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = await _httpClient.GetAsync("report/export/json/" + reportName);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                else
                {
                    _Log.Error("Failed to export report " + reportName);
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to export report " + reportName, ex.Message);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<DataSet?> ExportReportDataSet(string reportName)
        {
            try
            {
                string? guid = await GetGuid();
                if (guid == null)
                {
                    return null;
                }
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                HttpResponseMessage response = await _httpClient.GetAsync("report/export/json/" + reportName);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    DataSet ds = new DataSet();
                    ds.Tables.Add(JsonConvert.DeserializeObject<DataTable>(jsonString));
                    return ds;
                }
                else
                {
                    _Log.Error("Failed to export report " + reportName);
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to export report " + reportName, ex.Message);
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
            var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
            string jsonAddress = JsonConvert.SerializeObject(t, jsonSettings);
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
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = _httpClient.GetAsync("class/get/" + resource + "/" + id).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
                    var entity = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), jsonSettings);
                    return entity;
                }
                else
                {
                    _Log.Error("Failed to get record");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to get record", ex.Message);
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
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                HttpResponseMessage response = _httpClient.GetAsync("class/create/" + resource + "/").GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var blankrecord = JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    return blankrecord;
                }
                else
                {
                    _Log.Error("Failed to create record");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to create record", ex.Message);
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

                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                string jsonAddress = JsonConvert.SerializeObject(insert);
                var prestringContent = HttpUtility.UrlEncode(jsonAddress);
                StringContent stringContent = new StringContent("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");

                var postResponse = await _httpClient.PostAsync("class/insert/" + resource, stringContent);
                await InvalidateSession(guid);

                if (postResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _Log.Error("Failed to insert record");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to insert record", ex.Message);
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

                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);
                var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

                StringContent stringContent = SerialiseObject(FindParameter);

                var response = await _httpClient.PostAsync("class/find/" + resource, stringContent);
                await InvalidateSession(guid);

                if (response.IsSuccessStatusCode)
                {
                    List<T> foundrecords = JsonConvert.DeserializeObject<List<T>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), jsonSettings);
                    return foundrecords;
                }
                else
                {
                    _Log.Error("Failed to find record");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to find record", ex.Message);
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
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                var jsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, PreserveReferencesHandling = PreserveReferencesHandling.All, Formatting = Formatting.Indented };
                string jsonAddress = JsonConvert.SerializeObject(record, jsonSettings);

                //Encode any special characters in the JSON string
                var prestringContent = HttpUtility.UrlEncode(jsonAddress);

                //Correctly format the string content for the POST request as using application/x-www-form-urlencoded
                StringContent stringContent = new StringContent("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");

                //Update unite
                var postResponse = await _httpClient.PostAsync("class/update/" + resource, stringContent);

                if (postResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _Log.Error("Failed to update record");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to update record", ex.Message);
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
                var _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(_Settings.UniteBaseURL);
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("APISessionKey", guid);

                //May need to not include type information
                JsonSerializerSettings jsonSettings = new() { TypeNameHandling = TypeNameHandling.None };
                string jsonAddress = JsonConvert.SerializeObject(record, jsonSettings);
                jsonAddress = jsonAddress.Replace("System.Private.CoreLib", "mscorlib");
                string prestringContent = HttpUtility.UrlEncode(jsonAddress);
                StringContent stringContent = new("=" + prestringContent, Encoding.UTF8, @"application/x-www-form-urlencoded");


                //Update unite
                HttpResponseMessage postResponse = await _httpClient.PostAsync("class/updateproperty/" + resource + "/", stringContent);

                await InvalidateSession(guid);

                if (postResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _Log.Error("Failed to update record");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Failed to update record", ex.Message);
                return false;
            }
        }

        #endregion
    }
}
