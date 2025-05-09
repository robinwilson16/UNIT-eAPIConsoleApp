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
            string consumerKey = appSettings.NetSuiteConsumerKey ?? "";
            string consumerSecret = appSettings.NetSuiteConsumerSecret ?? "";
            string token = appSettings.NetSuiteTokenID ?? "";
            string tokenSecret = appSettings.NetSuiteTokenSecret ?? "";
            string realm = appSettings.NetSuiteAccountID + "_SB1" ?? "";

            string nonce = Guid.NewGuid().ToString("N");
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_token", token },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_timestamp", timestamp },
                { "oauth_nonce", nonce },
                { "oauth_version", "1.0" }
            };

            string parameterString = string.Join("&", parameters.Select(kvp => $"{URLEncodeUppercase(kvp.Key)}={URLEncodeUppercase(kvp.Value)}"));
            Console.WriteLine($"\nNetSuite OAuth Parameters: {parameterString}");

            string signatureBaseString = $"{method.ToUpper()}&{URLEncodeUppercase(url)}&{URLEncodeUppercase(parameterString)}";
            Console.WriteLine($"\nNetSuite Signature Base String: {signatureBaseString}");

            string signingKey = $"{URLEncodeUppercase(consumerSecret)}&{URLEncodeUppercase(tokenSecret)}";
            using var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
            string signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString)));

            parameters.Add("realm", realm); // Needs to not be part of the signature so is added after
            parameters.Add("oauth_signature", signature);

            string authHeader = "OAuth " + string.Join(", ", parameters.Select(kvp => $"{kvp.Key}=\"{URLEncodeUppercase(kvp.Value)}\""));
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
