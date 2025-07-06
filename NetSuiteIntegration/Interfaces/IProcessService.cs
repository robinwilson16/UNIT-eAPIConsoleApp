using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSuiteIntegration.Models;

namespace NetSuiteIntegration.Interfaces
{
    public interface IProcessService
    {
        Task<bool?> Process(ICollection<UNITeRepGen>? repGens, bool? readOnly, bool? firstRecordOnly, bool? forceInsertCustomer);
        Task<bool?> Testing();
    }
}
