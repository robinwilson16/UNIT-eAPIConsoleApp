using NetSuiteIntegration.Interfaces;
using Serilog;

namespace NetSuiteIntegration.Services
{
    public class ProcessService(ISRSWebServicecs unnitewebservice, IFinanceWebService netsuitewebservice,  ILogger logger): IProcessService
    {
        ISRSWebServicecs _unnitewebservice = unnitewebservice;
        IFinanceWebService _netsuitewebservice = netsuitewebservice;
        ILogger _Log = logger;
    }
}
