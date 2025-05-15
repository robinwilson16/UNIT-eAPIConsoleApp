using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using NetSuiteIntegration.Models;
using System.Text.RegularExpressions;

namespace NetSuiteIntegration.Services
{
    public static class OAuthHelper
    {
        public static string GenerateOAuth1Header(string url, string method, ApplicationSettings appSettings)
        {
            //Used for debugging where it will write out the OAuth parameters and signature base string to the console
            bool? debug = true;

            string consumerKey = appSettings.NetSuiteConsumerKey ?? "";
            string consumerSecret = appSettings.NetSuiteConsumerSecret ?? "";
            string token = appSettings.NetSuiteTokenID ?? "";
            string tokenSecret = appSettings.NetSuiteTokenSecret ?? "";
            string realm = appSettings.NetSuiteAccountID + "_SB1" ?? "";

            string nonce = Guid.NewGuid().ToString("N");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            //Extract URL parameters from the URL
            string urlParamsString = url.LastIndexOf("?") > 0 ? url.Substring(url.LastIndexOf("?") + 1) : "";
            string[] urlParams = urlParamsString.Split("&");

            string urlWithoutParams = url.Substring(0, url.LastIndexOf("?") > 0 ? url.LastIndexOf("?") : url.Length);

            SortedDictionary<string, string> oauthParams = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_token", token },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_timestamp", timestamp },
                { "oauth_nonce", nonce },
                { "oauth_version", "1.0" }
            };

            List<KeyValuePair<string, string>> allParams = new List<KeyValuePair<string, string>>(
                oauthParams.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
            );

            //Add URL parameters to the oauthParams dictionary
            if (!string.IsNullOrEmpty(urlParamsString))
            {
                var queryParams = HttpUtility.ParseQueryString(urlParamsString);
                if (queryParams != null)
                {
                    foreach (string key in queryParams)
                    {
                        if (key != null)
                        {
                            allParams.Add(new KeyValuePair<string, string>(key, queryParams[key]));
                        }
                    }
                }
            }

            // Sort parameters by key and value
            var sortedParams = allParams
                .OrderBy(p => p.Key)
                .ThenBy(p => p.Value)
                .ToList();

            if (debug == true)
            {
                Console.WriteLine($"\nFull URL: {url}");
                Console.WriteLine($"\nURL Without Params: {urlWithoutParams}");
                Console.WriteLine($"\nConsumer/Client Key: {consumerKey}");
                Console.WriteLine($"Consumer/Client Secret: {consumerSecret}");
                Console.WriteLine($"Consumer/Token: {token}");
                Console.WriteLine($"Consumer/Token Secret: {tokenSecret}");
                Console.WriteLine($"Timestamp: {timestamp}");
                Console.WriteLine($"Nonce: {nonce}");
                Console.WriteLine($"Query Params: {urlParamsString}");
            }


            string oauthParamsString = string.Join("&", sortedParams.Select(kvp =>
                $"{URLEncodeUppercase(kvp.Key)}={URLEncodeUppercase(kvp.Value)}"
            ));

            if (debug == true)
                Console.WriteLine($"\nNetSuite OAuth Parameters: {oauthParamsString}");

            string signatureBaseString = $"{method.ToUpper()}&{URLEncodeUppercase(urlWithoutParams)}&{URLEncodeUppercase(oauthParamsString)}";

            if (debug == true)
                Console.WriteLine($"\nNetSuite Signature Base String: {signatureBaseString}");

            string signingKey = $"{URLEncodeUppercase(consumerSecret)}&{URLEncodeUppercase(tokenSecret)}";
            using var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
            string signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString)));

            if (debug == true)
                Console.WriteLine($"\nNetSuite OAuth Signature: {signature}");

            oauthParams.Add("realm", realm); // Needs to not be part of the signature so is added after
            oauthParams.Add("oauth_signature", signature);

            string authHeader = "OAuth " + string.Join(", ", oauthParams.Select(kvp => $"{kvp.Key}=\"{URLEncodeUppercase(kvp.Value)}\""));

            if (debug == true)
                Console.WriteLine($"\nNetSuite Authorisation Header: {authHeader}");

            return authHeader;
        }

        public static string? URLEncodeUppercase(string? value)
        {
            if (value != null)
            {
                //var encoded = HttpUtility.UrlEncode(value);
                //Regex reg = new Regex(@"%[a-f0-9]{2}");
                //string encodedUppercase = reg.Replace(encoded, m => m.Value.ToUpperInvariant());
                //return encodedUppercase;
                var encoded = Uri.EscapeDataString(value);
                return encoded;
            }
            else
            {
                return null;
            }
        }
    }
}
