using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSuiteIntegration.Models;

namespace NetSuiteIntegration.Services
{
    public class OAuth1Handler : DelegatingHandler
    {
        private readonly ApplicationSettings _appSettings;

        public OAuth1Handler(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string authHeader = OAuthHelper.GenerateOAuth1Header(request.RequestUri?.ToString() ?? "", request.Method.Method, _appSettings);
            request.Headers.Add("Authorization", authHeader);
            InnerHandler = new HttpClientHandler(); // Ensure the inner handler is set to a valid handler for the response

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
