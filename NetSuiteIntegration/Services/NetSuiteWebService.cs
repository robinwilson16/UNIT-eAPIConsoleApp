using NetSuiteIntegration.Interfaces;
using NetSuiteIntegration.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Services
{
    public class NetSuiteWebService(ILogger log, ApplicationSettings applicationSettings) : IFinanceWebService
    {
        ApplicationSettings _Settings = applicationSettings;
        ILogger _Log = log;
    }
}
